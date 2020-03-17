using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class Entry
    {
        public Entry() { }
        public MongoDB.Bson.ObjectId id { get; set; } = MongoDB.Bson.ObjectId.GenerateNewId();
        public DateTime timestamp { get; set; }
        public string phone { get; set; }
        public string zip { get; set; }
        public string requestText { get; set; }
        public bool marked { get; set; }
        public object __v { get; set; }
        public string CString => marked ? "other" : string.Empty;
    }
}
