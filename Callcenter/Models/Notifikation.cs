using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class Notifikation
    {
        [BsonId]
        public ObjectId id { get; set; }
        public string organisation { get; set; }
        public string entry { get; set; }
        public DateTime timestamp { get; set; }
        public DateTime? gesendet { get; set; }
    }
}
