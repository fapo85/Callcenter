using Callcenter.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Controllers
{
    public class SignalRHub : Hub
    {
        private readonly EntrySave _save;
        public SignalRHub(EntrySave save)
        {
            _save = save;
        }
        public Task FreeEntry(string id)
        {
            Entry entry = _save.Find(new ObjectId(id));
            if (entry != null)
            {
                entry.marked = false;
                _save.Replace(entry);
            }
            return Clients.All.SendAsync("free", id);
        }
        public Task MarkEntry(string id)
        {
            Entry entry = _save.Find(new ObjectId(id));
            if (entry != null)
            {
                entry.marked = true;
                _save.Replace(entry);
            }
            new Task(()=>Clients.All.SendAsync("marked", id)).Start();
            return Clients.Caller.SendAsync("filldata", new EntryFill()
            {
                id = entry.id.ToString(),
                phone = entry.phone,
                zip = entry.zip,
                request = (int)entry.request
            });
        }

        public Task AddOrModifyEntry(string id, string phone, string zip, string request)
        {
            if (String.IsNullOrWhiteSpace(zip))
            {
                zip = "00000";
            }
            Entry entry;
            if (String.IsNullOrWhiteSpace(id) || id.Equals("000000000000000000000000"))
            {
                entry = new Entry()
                {
                    timestamp = DateTime.Now,
                };
            }
            else
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
            return Clients.Caller.SendAsync("SaveOK", new EntryFill()
            {
                id = entry.id.ToString(),
                phone = entry.phone,
                zip = entry.zip,
                request = (int)entry.request
            });
        }


        public Task DeleteEntry(string id)
        {
            Entry entry = _save.Find(new ObjectId(id));
            if (entry == null)
            {
                throw new KeyNotFoundException("Id ist ungültig");
            }
            _save.Remove(new ObjectId(id));
            return Clients.All.SendAsync("delete", id);
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
