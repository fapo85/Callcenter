using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Callcenter.Models;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;

namespace Callcenter.Controllers
{
    public class EntryController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly EntrySave _save;
        public EntryController(ILogger<HomeController> logger, IHubContext<SignalRHub> hubContext, EntrySave save)
        {
            _logger = logger;
            _hubContext = hubContext;
            _save = save;
        }

        public IActionResult Index()
        {
            return View(new Entry());
        }

        [HttpGet("/Entry/Mark/{id}")]
        public IActionResult Mark(string id)
        {
            Console.WriteLine($"Element {id} Marked");
            Entry entry = _save.Find(new ObjectId(id));
            if (entry != null)
            {
                _save.Mark(entry);
            }
            _hubContext.Clients.All.SendAsync("marked", id).Wait();
            return Ok();
        }

        [HttpGet("/Entry/Delete/{id}")]
        public IActionResult Delete(string id)
        {
            Console.WriteLine($"Element {id} Delete");
            Entry entry = _save.Find(new ObjectId(id));
            if (entry != null)
            {
                _save.Remove(new ObjectId(id));
            }
            _hubContext.Clients.All.SendAsync("delete", id).Wait();
            return View("Index", entry);
        }
        [HttpPost("/AddEntry")]
        public IActionResult AddEntry(string phone, string requestText, string zip)
        {
            if (String.IsNullOrWhiteSpace(zip))
            {
                zip = "00000";
            }
            Entry entry = new Entry()
            {
                timestamp = DateTime.Now,
                phone = phone,
                zip = zip,
                requestText = requestText
            };
            _save.Add(entry);
            return Ok();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
