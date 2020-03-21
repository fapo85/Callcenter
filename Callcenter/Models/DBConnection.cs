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

namespace Callcenter.Models
{
    public class DBConnection
    {
        private readonly IMongoCollection<Entry> requests;
        private readonly IMongoCollection<Captcha> captchas;

        public DBConnection(IOptions<MongoDbConf> options, IHubContext<SignalRHub> hubContext)
        {
            var mongoDbConf = options.Value;

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(mongoDbConf.Connection));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);

            var database = mongoClient.GetDatabase(mongoDbConf.DbName);

            requests = database.GetCollection<Entry>("requests");
            captchas = database.GetCollection<Captcha>("captcha");
            _hubContext = hubContext;

            Listen();
        }

        internal Captcha GetCaptcha(string id) => captchas.Find(e => e.id == new ObjectId(id)).SingleOrDefault();
        internal void AddCaptcha(Captcha captcha)=> captchas.InsertOne(captcha);

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


        internal Entry Find(ObjectId id) => requests.Find(e => e.id == id).SingleOrDefault();

        internal void Replace(Entry entry) => requests.ReplaceOne(e => e.id == entry.id, entry);


        //public static Random random = new Random();
        private IHubContext<SignalRHub> _hubContext;

    }
}