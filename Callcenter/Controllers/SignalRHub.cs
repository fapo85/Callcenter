using Callcenter.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Callcenter.Controllers
{
    public class SignalRHub : Hub
    {
        private readonly DBConnection _save;
        public SignalRHub(DBConnection save)
        {
            _save = save;
        }
        public Task FreeEntry(string id)
        {
            Task t = new Task(() =>
            {
                Entry entry = _save.Find(new ObjectId(id));
                if (entry != null)
                {
                    entry.marked = false;
                    _save.Replace(entry);
                }
            });
            t.Start();
            return t;
        }
        public Task MarkEntry(string id)
        {
            Entry entry = _save.Find(new ObjectId(id));
            if (entry != null)
            {
                entry.marked = true;
                _save.Replace(entry);
            }
            return Clients.Caller.SendAsync("filldata", entry.TrasportModel);
        }

        public Task AddOrModifyEntry(string id, string phone, string zip, string request)
        {
            Entry entry = entry = new Entry()
            {
                timestamp = DateTime.Now,
            };
            try
            {
                if (String.IsNullOrWhiteSpace(zip))
                {
                    zip = "00000";
                }
                if (!(String.IsNullOrWhiteSpace(id) || id.Equals("000000000000000000000000")))
                {
                    var oldvalue = _save.Find(new ObjectId(id));
                    entry = new Entry()
                    {
                        timestamp = oldvalue.timestamp,
                    };
                }
                entry.modifyts = DateTime.Now;
                entry.phone = phone;
                entry.zip = zip;
                entry.request = ParseRequest(request);
                entry.Validate();
                _save.Add(entry);
                return Clients.Caller.SendAsync("SaveOK", entry.TrasportModel);
            }catch(Exception e)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Fehler: ");
                DataContractJsonSerializer dcjs = new DataContractJsonSerializer(entry.GetType());
                using (MemoryStream ms = new MemoryStream()){
                    dcjs.WriteObject(ms, entry);
                    sb.AppendLine(Encoding.Default.GetString(ms.ToArray()));
                };
                sb.AppendLine(e.ToString());
                Console.WriteLine(sb.ToString());
                return Clients.Caller.SendAsync("Error", e.Message);
            }
        }


        public Task DeleteEntry(string id)
        {
            Task t = new Task(() =>
            {
                Entry entry = _save.Find(new ObjectId(id));
                if (entry == null)
                {
                    throw new KeyNotFoundException("Id ist ungültig");
                }
                _save.Remove(entry);
            });
            t.Start();
            return t;
        }

        private EntryRequest ParseRequest(string request)
        {
            if(int.TryParse(request, out int v))
            {
                return (EntryRequest)v;
            }
            foreach(EntryRequest er in (EntryRequest[])Enum.GetValues(typeof(EntryRequest)))
            {
                if (request.ToLower().Equals(er.ToString().Trim().ToLower()))
                    return er;
            }
            throw new FormatException($"kann \"{request}\" nicht nach EntryRequest umwandeln");
        }
    }
}
