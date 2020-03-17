using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Callcenter.Config;
using Microsoft.Extensions.Options;

namespace Callcenter.Models
{
    public class EntrySave
    {
        //private readonly MongoClient client;
        //private readonly IMongoDatabase database;
        private readonly IMongoCollection<Entry> collection;
        public EntrySave(IOptions<MongoDbConf> options)
        {
            var mongoDbConf = options.Value;
            var client = new MongoClient(mongoDbConf.Connection);
            var database = client.GetDatabase(mongoDbConf.DbName);
            collection = database.GetCollection<Entry>("requests");
        }
        public List<Entry> GetAll() => collection.Find(e => true).ToList();
        public List<Entry> GetNoZip() => collection.Find(e => e.zip == "00000").ToList();

        internal void Remove(ObjectId id)=> collection.DeleteOne(e => e.id == id);

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
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
