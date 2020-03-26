using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Callcenter.Config;
using Callcenter.Controllers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using Callcenter.Erweiterung;
using System.Text;

namespace Callcenter.Models
{
    public class DBConnection
    {
        private readonly IMongoCollection<Entry> requests;
        private readonly IMongoCollection<Captcha> captchas;
        private readonly IMongoCollection<Notifikation> notifications;
        private readonly IMongoCollection<Organization> organisations;
        private readonly NotifikationFactory notifikationFactory;

        public DBConnection(IOptions<MongoDbConf> options, IHubContext<SignalRHub> hubContext)
        {
            var mongoDbConf = options.Value;

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(mongoDbConf.Connection));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);

            var database = mongoClient.GetDatabase(mongoDbConf.DbName);

            requests = database.GetCollection<Entry>("requests");
            captchas = database.GetCollection<Captcha>("captcha");
            organisations = database.GetCollection<Organization>("organisations");
            notifications = database.GetCollection<Notifikation>("notifications");
            CreateIndexOptions<Notifikation> notificationIndexoptions = new CreateIndexOptions<Notifikation>();
            notificationIndexoptions.Unique = true;
            var notificationIndex = new CreateIndexModel<Notifikation>(Builders<Notifikation>.IndexKeys.Combine(
                Builders<Notifikation>.IndexKeys.Ascending(n => n.entry),
                Builders<Notifikation>.IndexKeys.Ascending(n => n.organisation)                
                ), notificationIndexoptions);
            notificationIndex.Options.Unique = true;
            notifications.Indexes.CreateOne(notificationIndex);
            _hubContext = hubContext;
            notifikationFactory = new NotifikationFactory(this);
            Listen();

        }

        internal void UpdateOrganization(Organization entry)
        {
            entry.Verify();
            organisations.ReplaceOne(o => o.id == entry.id, entry);
        }

        internal Organization FindOrganization(string id) => FindOrganization(new ObjectId(id));
        internal Organization FindOrganization(ObjectId id) => organisations.Find(i => i.id == id).SingleOrDefault();

        internal Captcha GetCaptcha(string id) => captchas.Find(e => e.id == new ObjectId(id)).SingleOrDefault();
        internal void AddCaptcha(Captcha captcha)=> captchas.InsertOne(captcha);

        internal void AddOrganization(Organization entry)
        {
            entry.Verify();
            organisations.InsertOne(entry);
        }

        internal void RemoveCaptcha(Captcha captcha) => captchas.DeleteOne(c => c.id == captcha.id);
        internal void CleanupCaptcha() => captchas.DeleteMany(e => e.Timestamp < DateTime.Now.Subtract(CaptchaFactory.GÜLTIGKEIT));
        private async void Listen()
        {
            var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup };
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Entry>>()
                .Match("{ operationType: { $in: [ 'insert','replace', 'update' ] }}")
                .Project("{ fullDocument: 1 }");

            using var cursor = requests.Watch(pipeline, options);
            await cursor.ForEachAsync(change =>
            {
                Entry entry = BsonSerializer.Deserialize<Entry>((BsonDocument)change.Elements.ToList()[1].Value);
                if (entry.zip.Equals("00000"))
                {
                    var send = entry.TrasportModel;
                    _hubContext.Clients.All.SendAsync("ItemChange", send);
                    foreach (Organization organisation in organisations.Find("{ \"zips\": {$in: [ '00000', ]}}").ToList<Organization>())
                    {
                        var notifikation = new Notifikation()
                        {
                            entry = entry.id.ToString(),
                            organisation = organisation.id.ToString(),
                            timestamp = DateTime.Now
                        };
                        if (TryAddNotifkation(notifikation))
                        {
                            notifikationFactory.Send(notifikation, organisation, entry);
                        }
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < entry.zip.Length; i++)
                    {
                        sb.Append('\'');
                        sb.Append(entry.zip.Substring(0, entry.zip.Length - i));
                        sb.Append("', ");
                    }
                    foreach (Organization organisation in organisations.Find($"{{ \"zips\": {{$in: [ {sb.ToString()} ]}}}}").ToList<Organization>())
                    {
                        var notifikation = new Notifikation()
                        {
                            entry = entry.id.ToString(),
                            organisation = organisation.id.ToString(),
                            timestamp = DateTime.Now
                        };
                        if (TryAddNotifkation(notifikation))
                        {
                            notifikationFactory.Send(notifikation, organisation, entry);
                        }
                    }
                }
            });
        }

        //public List<Entry> GetAll() => collection.Find(e => true).SortBy(e => e.timestamp).ToList();
        public List<Entry> GetAll(int skip, int limit)
        {
            var list = requests.Find(e => !e.finishts.HasValue).Skip(skip).Limit(limit).ToList();
            list.Sort(Entry.Compare);
            return list;
        }

        public long CountAll() => requests.Find(e => true).CountDocuments();


        //public List<Entry> GetNoZip() => collection.Find(e => e.zip == "00000").SortBy(e => e.timestamp).ToList();

        public List<Entry> GetNoZip()
        {
            var list = requests.Find(e => !e.finishts.HasValue && e.zip == "00000").ToList();
            list.Sort(Entry.Compare);
            return list;
        }

        public long CountNoZip() => requests.Find(e => e.zip == "00000").CountDocuments();

        internal long CountCallHour() => requests.Find(e => e.timestamp > DateTime.Now.Subtract(TimeSpan.FromMinutes(60))).CountDocuments();
        internal long CountEditHour() => requests.Find(e => e.modifyts.HasValue && e.modifyts > DateTime.Now.Subtract(TimeSpan.FromMinutes(60))).CountDocuments();
        internal long CountCallDay() => requests.Find(e => e.timestamp > DateTime.Now.Subtract(TimeSpan.FromMinutes(1440))).CountDocuments();
        internal long CountEditDay() => requests.Find(e => e.modifyts.HasValue && e.modifyts > DateTime.Now.Subtract(TimeSpan.FromMinutes(1440))).CountDocuments();

        //internal void Remove(ObjectId id) => requests.DeleteOne(e => e.id == id);
        internal void Remove(ObjectId id) => Remove(Find(id));
        internal void Remove(Entry entry)
        {
            entry.finishts = DateTime.Now;
            Replace(entry);
        }

        internal void Add(Entry entry)
        {
            if (entry.id == null)
            {
                entry.id = MongoDB.Bson.ObjectId.GenerateNewId();
            }
            requests.InsertOne(entry);
        }

        public bool TryAddNotifkation(Notifikation notifikation)
        {
            try
            {
                if(notifikation.id == null)
                {
                    notifikation.id = new ObjectId();
                }
                notifikation.timestamp = DateTime.Now;
                notifications.InsertOne(notifikation);
                return true;
            }
            catch
            {
                return false;
            }
        }
        internal Entry Find(ObjectId id) => requests.Find(e => e.id == id).SingleOrDefault();
        internal IEnumerable<Organization> GetOrganisations() => organisations.Find(o => true).ToEnumerable<Organization>();
        internal IEnumerable<Organization> FindOrganisations(string suche, bool zipreverse)
        {
            string filter;
            if (zipreverse)
            {
                filter = $"{{$or: [ {{ \"zips\": {{'$regex': '{suche}'}}}},{{ \"name\": {{'$regex': '{suche}'}}}},{{ \"ansprechpartner\": {{'$regex': '{suche}'}}}},{{ \"email\": {{'$regex': '{suche}'}}}}  ]}}";
            }
            else
            {
                filter = $"{{$or: [ {{ \"zips\": {inreg(suche)}}},{{ \"name\": {{'$regex': '{suche}'}}}},{{ \"ansprechpartner\": {{'$regex': '{suche}'}}}},{{ \"email\": {{'$regex': '{suche}'}}}}  ]}}";
            }
            return organisations.Find(filter).ToEnumerable<Organization>();
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < suche.Length; i++)
            //{ 
            //    sb.Append('\'');
            //    sb.Append(suche.Substring(0, suche.Length - i));
            //    sb.Append("', ");
            //}
            //return organisations.Find($"{{$in: [ {{ \"zips\": {{$in: [ {sb.ToString()} ]}}]}}}}").ToEnumerable<Organization>();
            //organisations.Find($"{{ \"zips\": {{$in: [ {sb.ToString()} ]}}}}").ToList<Organization>()
            //return organisations.Find($"{{ \"zips\": {{'$regex': '{suche}'}}}}").ToEnumerable<Organization>();
            //return organisations.Find(o => o.name.ToLower().StartsWith(suche.ToLower()) || o.ansprechpartner.ToLower().StartsWith(suche.ToLower()) || o.zips.OneStartWith()).ToEnumerable<Organization>();
        }
        private static string inreg(string inpt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{$in: [");
            for (int i = 0; i < inpt.Length; i++)
            {
                sb.Append('\'');
                sb.Append(inpt.Substring(0, inpt.Length - i));
                sb.Append("', ");
            }
            sb.Append("]}");
            return sb.ToString();
        }
        internal void Replace(Entry entry) => requests.ReplaceOne(e => e.id == entry.id, entry);


        //public static Random random = new Random();
        private IHubContext<SignalRHub> _hubContext;

    }
}