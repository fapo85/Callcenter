using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class Organization
    {
        [BsonId]
        public ObjectId id { get; set; }
        public string name { get; set; }
        public string ansprechpartner { get; set; }
        public string email { get; set; }
        public List<string> zips { get; set; }
        public List<EntryRequest> notifyrequest { get; set; }
        public DateTime timestamp { get; set; }
        public string GetZipString(bool multiline)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (string str in zips)
            {
                if (multiline)
                {
                    sb.AppendLine(str);
                }
                else
                {
                    if (!first)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(str);
                }
                first = false;
            }
            return sb.ToString();
        }
        internal static List<string> ParseZips(string zip)
        {
            List<string> ret = new List<string>();
            foreach (string i in zip.Split('\n'))
            {
                var str = i.Trim();
                if (!String.IsNullOrWhiteSpace(str) && str.Length <= 5 && int.TryParse(str, out int plz))
                {
                    ret.Add(plz.ToString());
                }
            }
            return ret;
        }
        public string NotifyRequestString()
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach(EntryRequest er in notifyrequest)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                sb.Append(er.ToString());
                first = false;
            }
            return sb.ToString();
        }
        internal void Verify()
        {
            if (id == null)
            {
                throw new ArgumentException("ID darf nicht leer sein");
            }
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name darf nicht leer sein");
            }
            if (String.IsNullOrWhiteSpace(ansprechpartner))
            {
                throw new ArgumentException("Ansprechpartner darf nicht leer sein");
            }
            if (String.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email darf nicht leer sein");
            }
            if (zips == null || zips.Count == 0)
            {
                throw new ArgumentException("Postleitzahl darf nicht leer sein");
            }
        }
    }
}
