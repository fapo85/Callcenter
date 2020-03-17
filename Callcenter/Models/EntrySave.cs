using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class EntrySave
    {
        private static readonly string connection = "mongodb://127.0.0.1/";
        private static readonly string dbname = "Callcenter";
        //private readonly MongoClient client;
        //private readonly IMongoDatabase database;
        private readonly IMongoCollection<Entry> collection;
        public EntrySave()
        {
            var client = new MongoClient(connection);
            var database = client.GetDatabase(dbname);
            collection = database.GetCollection<Entry>("requests");
        }
        public List<Entry> GetAll() => collection.Find(e => true).ToList();
        public List<Entry> GetNoZip() => collection.Find(e => e.zip == "00000").ToList();

        internal void Remove(string id)=> collection.DeleteOne(e => e.id.ToString() == id);

        internal void Add(Entry entry)
        {
            if (entry.id == null)
            {
                entry.id = MongoDB.Bson.ObjectId.GenerateNewId();
            }
            collection.InsertOne(entry);
        }


        internal Entry Find(string id) => collection.Find(e => e.id.ToString() == id).SingleOrDefault();

        internal void Mark(Entry entry) => collection.ReplaceOne(e => e.id == entry.id, entry);


        public static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
