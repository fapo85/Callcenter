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
            Console.WriteLine($"Element {id} Free");
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
            Console.WriteLine($"Element {id} Marked");
            Entry entry = _save.Find(new ObjectId(id));
            if (entry != null)
            {
                entry.marked = true;
                _save.Replace(entry);
            }
            new Task(()=>Clients.All.SendAsync("marked", id)).Start();
            return Clients.Caller.SendAsync("filldata", entry);
        }
        public void AddEntry(string id, string phone, EntryRequest request, string zip)
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
            entry.request = request;
            _save.Add(entry);
        }
        public Task DeleteEntry(string id)
        {
            Console.WriteLine($"Element {id} Delete");
            Entry entry = _save.Find(new ObjectId(id));
            if (entry == null)
            {
                throw new KeyNotFoundException("Id ist ungültig");
            }
            _save.Remove(new ObjectId(id));
            return Clients.All.SendAsync("delete", id);
        }
    }
}
