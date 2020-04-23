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
        //private readonly IMongoCollection<Notifikation> notifications;
        //private readonly IMongoCollection<Organization> organisations;
        //private readonly NotifikationFactory notifikationFactory;

        public DBConnection(IOptions<MongoDbConf> options, IHubContext<SignalRHub> hubContext)
        {
            var mongoDbConf = options.Value;

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(mongoDbConf.Connection));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);

            var database = mongoClient.GetDatabase(mongoDbConf.DbName);

            requests = database.GetCollection<Entry>("requests");
            captchas = database.GetCollection<Captcha>("captcha");
            //organisations = database.GetCollection<Organization>("organisations");
            //notifications = database.GetCollection<Notifikation>("notifications");
            CreateIndexOptions<Notifikation> notificationIndexoptions = new CreateIndexOptions<Notifikation>();
            notificationIndexoptions.Unique = true;
            var notificationIndex = new CreateIndexModel<Notifikation>(Builders<Notifikation>.IndexKeys.Combine(
                Builders<Notifikation>.IndexKeys.Ascending(n => n.entry),
                Builders<Notifikation>.IndexKeys.Ascending(n => n.organisation)                
                ), notificationIndexoptions);
            notificationIndex.Options.Unique = true;
            //notifications.Indexes.CreateOne(notificationIndex);
            _hubContext = hubContext;
            //notifikationFactory = new NotifikationFactory(this);
            Listen();

        }
        /// <summary>
        /// Eine Bearbeitete Organisation Finden und Ersetzen
        /// </summary>
        /// <param name="entry"></param>
        //internal void UpdateOrganization(Organization entry)
        //{
        //    entry.Verify();
        //    organisations.ReplaceOne(o => o.id == entry.id, entry);
        //}
        /// <summary>
        /// Findet eine Orgnisation über die id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //internal Organization FindOrganization(string id) => FindOrganization(new ObjectId(id));
        //internal Organization FindOrganization(ObjectId id) => organisations.Find(i => i.id == id).SingleOrDefault();

        /// <summary>
        /// Findet ein Captcha Element in der Datenbank
        /// </summary>
        /// <returns></returns>
        internal Captcha GetCaptcha(string id) => captchas.Find(e => e.id == new ObjectId(id)).SingleOrDefault();
        /// <summary>
        /// Fügt ein Captcha Element in die Datenbank hinzu
        /// </summary>
        internal void AddCaptcha(Captcha captcha)=> captchas.InsertOne(captcha);
        /// <summary>
        /// Löscht einen einzelnen eintrag in der Datenbank
        /// Dies passiert wenn das captcha erfolgreich "verwendet" wurde
        /// </summary>
        /// <param name="captcha"></param>
        internal void RemoveCaptcha(Captcha captcha) => captchas.DeleteOne(c => c.id == captcha.id);
        /// <summary>
        /// Löscht Alle Captchas aus der Datenbank, deren Laufzeit abgelaufen ist.
        /// </summary>
        internal void CleanupCaptcha() => captchas.DeleteMany(e => e.Timestamp < DateTime.Now.Subtract(CaptchaFactory.GÜLTIGKEIT));
        /// <summary>
        /// Fügt eine Organisation in die Datenbank hinzu
        /// </summary>
        /// <param name="entry"></param>
        //internal void AddOrganization(Organization entry)
        //{
        //    entry.Verify();
        //    organisations.InsertOne(entry);
        //}
        /// <summary>
        /// Listener Für Änderungen in der Entry Datenbank
        /// Sorgt mit hilfe von SignalR dafür, das die Frontends aktualsiert werden.
        /// Prüft ob es eine Organisation gibt, welche eine benachrichtigung z.b. über email aboniert hat und versendert die.
        /// Es ist auch möglich sich auf die PLZ 00000 zu Registrieren.
        /// </summary>
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
                    //foreach (Organization organisation in organisations.Find("{ \"zips\": {$in: [ '00000', ]}}").ToList<Organization>())
                    //{
                    //    var notifikation = new Notifikation()
                    //    {
                    //        entry = entry.id.ToString(),
                    //        organisation = organisation.id.ToString(),
                    //        timestamp = DateTime.Now
                    //    };
                    //    if (TryAddNotifkation(notifikation))
                    //    {
                    //        notifikationFactory.Send(notifikation, organisation, entry);
                    //    }
                    //}
                }
                //else
                //{
                //    StringBuilder sb = new StringBuilder();
                //    for (int i = 0; i < entry.zip.Length; i++)
                //    {
                //        sb.Append('\'');
                //        sb.Append(entry.zip.Substring(0, entry.zip.Length - i));
                //        sb.Append("', ");
                //    }
                //    foreach (Organization organisation in organisations.Find($"{{ \"zips\": {{$in: [ {sb.ToString()} ]}}}}").ToList<Organization>())
                //    {
                //        var notifikation = new Notifikation()
                //        {
                //            entry = entry.id.ToString(),
                //            organisation = organisation.id.ToString(),
                //            timestamp = DateTime.Now
                //        };
                //        if (TryAddNotifkation(notifikation))
                //        {
                //            notifikationFactory.Send(notifikation, organisation, entry);
                //        }
                //    }
                //}
            });
        }

        //public List<Entry> GetAll() => collection.Find(e => true).SortBy(e => e.timestamp).ToList();
        /// <summary>
        /// Gibt alle Einträge Sortiert zurück
        /// </summary>
        /// <param name="skip">Erstes Element</param>
        /// <param name="limit">Anzahl der Elemente</param>
        /// <returns></returns>
        public List<Entry> GetAll(int skip, int limit)
        {
            var list = requests.Find(e => e.finishts==null).Skip(skip).Limit(limit).ToList();
            list.Sort(Entry.Compare);
            return list;
        }
        /// <summary>
        /// Anzahl aller einträge in der datenkbank
        /// </summary>
        /// <returns></returns>
        public long CountAll() => requests.Find(e => true).CountDocuments();


        //public List<Entry> GetNoZip() => collection.Find(e => e.zip == "00000").SortBy(e => e.timestamp).ToList();
        /// <summary>
        /// Gibt alle einträge zurück, welche keine PLZ Besitzen
        /// </summary>
        /// <returns></returns>
        public List<Entry> GetNoZip()
        {
            var list = requests.Find(e => e.finishts == null && e.zip == "00000").ToList();
            list.Sort(Entry.Compare);
            return list;
        }
        /// Anzahl aller Einträge welche keine plz besitzen
        public long CountNoZip() => requests.Find(e => e.zip == "00000").CountDocuments();
        /// <summary>
        /// Anzahl aller Anrufe in den letzten 60 Minuten
        /// </summary>
        /// <returns></returns>
        internal long CountCallHour() => requests.Find(e => e.timestamp > DateTime.Now.Subtract(TimeSpan.FromMinutes(60))).CountDocuments();
        /// <summary>
        /// Anzahl im Frontend Bearbeiteten Einträge in der Letzten Stunde
        /// </summary>
        /// <returns></returns>
        internal long CountEditHour() => requests.Find(e => e.modifyts != null && e.modifyts > DateTime.Now.Subtract(TimeSpan.FromMinutes(60))).CountDocuments();
        /// <summary>
        /// Anzahl aller Anrufe in den letzten 24 Stunden
        /// </summary>
        /// <returns></returns>
        internal long CountCallDay() => requests.Find(e => e.timestamp > DateTime.Now.Subtract(TimeSpan.FromMinutes(1440))).CountDocuments();
        /// <summary>
        /// Anzahl im Frontend Bearbeiteten Einträge in der Letzten 24 Stunden
        /// </summary>
        /// <returns></returns>
        internal long CountEditDay() => requests.Find(e => e.modifyts != null && e.modifyts > DateTime.Now.Subtract(TimeSpan.FromMinutes(1440))).CountDocuments();

        //internal void Remove(ObjectId id) => requests.DeleteOne(e => e.id == id);
        /// <summary>
        /// Löscht einen Telefon Anruf
        /// </summary>
        /// <param name="id"></param>
        internal void Remove(ObjectId id) => Remove(Find(id));
        internal void Remove(Entry entry)
        {
            entry.finishts = DateTime.Now;
            Replace(entry);
        }
        /// <summary>
        /// SPeichert einen Neuene Anruf in der Datenbank
        /// </summary>
        /// <param name="entry"></param>
        internal void Add(Entry entry)
        {
            if (entry.id == null)
            {
                entry.id = MongoDB.Bson.ObjectId.GenerateNewId();
            }
            requests.InsertOne(entry);
        }
        /// <summary>
        /// Versucht einen Neuen Eintrag in die Datenbank zu Speichern.
        /// Die Software ist darauf ausgelegt auf mehreren Servern gleichzeitig zu laufen
        /// in der Methode Listen werden Alle instanzen dieser Software auf einmal informiert.
        /// die Datenbank hat einen Unique Index auf die relevanten Felder (siehe Konstruktor)
        /// Die Schnellste Instanz wird den Eintrag einfügen können die restlichen bekommen eine Exeption
        /// Bei Exception wird false zurück gegeben und bei erfolg true. So wird die Benachrichtigung nur vom Schnellsten Server versendet.
        /// </summary>
        //public bool TryAddNotifkation(Notifikation notifikation)
        //{
        //    try
        //    {
        //        if(notifikation.id == null)
        //        {
        //            notifikation.id = new ObjectId();
        //        }
        //        notifikation.timestamp = DateTime.Now;
        //        notifications.InsertOne(notifikation);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        /// <summary>
        /// Findet einen Telefonanruf
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal Entry Find(ObjectId id) => requests.Find(e => e.id == id).SingleOrDefault();
        /// <summary>
        /// Gibt alle Organisationen Zurück
        /// </summary>
        /// <returns></returns>
        //internal IEnumerable<Organization> GetOrganisations() => organisations.Find(o => true).ToEnumerable<Organization>();
        //internal IEnumerable<Organization> FindOrganisations(string suche, bool zipreverse)
        //{
        //    string filter;
        //    if (zipreverse)
        //    {
        //        filter = $"{{$or: [ {{ \"zips\": {{'$regex': '{suche}'}}}},{{ \"name\": {{'$regex': '{suche}'}}}},{{ \"ansprechpartner\": {{'$regex': '{suche}'}}}},{{ \"email\": {{'$regex': '{suche}'}}}}  ]}}";
        //    }
        //    else
        //    {
        //        filter = $"{{$or: [ {{ \"zips\": {inreg(suche)}}},{{ \"name\": {{'$regex': '{suche}'}}}},{{ \"ansprechpartner\": {{'$regex': '{suche}'}}}},{{ \"email\": {{'$regex': '{suche}'}}}}  ]}}";
        //    }
        //    return organisations.Find(filter).ToEnumerable<Organization>();
        //    //StringBuilder sb = new StringBuilder();
        //    //for (int i = 0; i < suche.Length; i++)
        //    //{ 
        //    //    sb.Append('\'');
        //    //    sb.Append(suche.Substring(0, suche.Length - i));
        //    //    sb.Append("', ");
        //    //}
        //    //return organisations.Find($"{{$in: [ {{ \"zips\": {{$in: [ {sb.ToString()} ]}}]}}}}").ToEnumerable<Organization>();
        //    //organisations.Find($"{{ \"zips\": {{$in: [ {sb.ToString()} ]}}}}").ToList<Organization>()
        //    //return organisations.Find($"{{ \"zips\": {{'$regex': '{suche}'}}}}").ToEnumerable<Organization>();
        //    //return organisations.Find(o => o.name.ToLower().StartsWith(suche.ToLower()) || o.ansprechpartner.ToLower().StartsWith(suche.ToLower()) || o.zips.OneStartWith()).ToEnumerable<Organization>();
        //}
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
        /// <summary>
        /// Ersetzt Einen Telefonanruf in der datenbank
        /// </summary>
        /// <param name="entry"></param>
        internal void Replace(Entry entry) => requests.ReplaceOne(e => e.id == entry.id, entry);


        //public static Random random = new Random();
        private IHubContext<SignalRHub> _hubContext;

    }
}