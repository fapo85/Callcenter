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
        //public Task AddEntry(string id, string phone, string zip, EntryRequest request)
        public Task AddEntry(ToAddRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.zip))
            {
                request.zip = "00000";
            }
            Entry entry;
            if (String.IsNullOrWhiteSpace(request.id) || request.id.Equals("000000000000000000000000"))
            {
                entry = new Entry()
                {
                    timestamp = DateTime.Now,
                };
            }
            else
            {
                var oldvalue = _save.Find(new ObjectId(request.id));
                entry = new Entry()
                {
                    timestamp = oldvalue.timestamp,
                };
            }
            entry.modifyts = DateTime.Now;
            entry.phone = request.phone;
            entry.zip = request.zip;
            entry.request = request.request;
            Task t = new Task(()=>_save.Add(entry));
            t.Start();
            return t;
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
