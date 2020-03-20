using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class EntryFill
    {
        public string id { get; set; }
        public DateTime timestamp { get; set; }
        public DateTime? modifyts { get; set; }
        public string phone { get; set; }
        public string zip { get; set; }
        public string request { get; set; }
        public int requestid { get; set; }
        public bool marked { get; set; }
    }
}
