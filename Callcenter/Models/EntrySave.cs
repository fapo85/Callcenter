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
    public class EntrySave
    {
        //private readonly MongoClient client;
        //private readonly IMongoDatabase database;
        private IMongoCollection<Entry> collection;

        public EntrySave(IOptions<MongoDbConf> options, IHubContext<SignalRHub> hubContext)
        {
            var mongoDbConf = options.Value;

            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(mongoDbConf.Connection));
            settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);

            var database = mongoClient.GetDatabase(mongoDbConf.DbName);

            collection = database.GetCollection<Entry>("requests");
            _hubContext = hubContext;

            Listen();
        }

        public async void Listen()
        {
            try
            {
                //Get the whole document instead of just the changed portion
                var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup };
                var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Entry>>()
                    .Match("{ operationType: { $in: [ 'insert','replace', 'update'  ] }}")
                    .Project("{ fullDocument: 1 }");

                using var cursor = collection.Watch(pipeline, options);
                await cursor.ForEachAsync(change =>
                {
                    var s = change.Elements.ToList()[1].Value.ToString().Replace("ObjectId(", "").Replace("ISODate(", "").Replace("\")", "\"");
                    Console.WriteLine(s);
                    _hubContext.Clients.All.SendAsync("test", s);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Setup failed: \n" + e);
            }
        }

        public List<Entry> GetAll() => collection.Find(e => true)/*.SortBy(e => e.timestamp)*/.ToList();
        public List<Entry> GetNoZip() => collection.Find(e => e.zip == "00000")/*.SortBy(e => e.timestamp)*/.ToList();

        internal void Remove(ObjectId id) => collection.DeleteOne(e => e.id == id);

        internal void Add(Entry entry)
        {
            if (entry.id == null)
            {
                entry.id = MongoDB.Bson.ObjectId.GenerateNewId();
            }

            collection.InsertOne(entry);
        }


        internal Entry Find(ObjectId id) => collection.Find(e => e.id == id).SingleOrDefault();

        internal void Replace(Entry entry) => collection.ReplaceOne(e => e.id == entry.id, entry);


        public static Random random = new Random();
        private IHubContext<SignalRHub> _hubContext;

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}