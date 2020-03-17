using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class Entry
    {
        public Entry() { }
        [BsonId]
        public ObjectId id { get; set; }
        public DateTime timestamp { get; set; }
        public string phone { get; set; }
        public string zip { get; set; }
        public string requestText { get; set; }
        public bool marked { get; set; }
        public object __v { get; set; }
        public string CString => marked ? "other" : string.Empty;

        internal static string ParseRequest(string request)
        {
            switch (request.Trim())
            {
                case "1":
                    return "";
                case "2":
                    return "";
                case "3":
                    return "";
                case "4":
                    return "";
                default:
                    return request;
            }
        }
    }
}
