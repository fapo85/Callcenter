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
using System.Security.Authentication;

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
                entry.marked = true;
                _save.Replace(entry);
            }
            _hubContext.Clients.All.SendAsync("marked", id).Wait();
            return Ok();
        }
        [HttpGet("/Entry/Free/{id}")]
        public IActionResult UnMark(string id)
        {
            Console.WriteLine($"Element {id} Free");
            Entry entry = _save.Find(new ObjectId(id));
            if (entry != null)
            {
                entry.marked = false;
                _save.Replace(entry);
            }
            _hubContext.Clients.All.SendAsync("free", id).Wait();
            return Ok();
        }

        [HttpGet("/Entry/Delete/{id}")]
        public IActionResult Delete(string id)
        {
            Console.WriteLine($"Element {id} Delete");
            Entry entry = _save.Find(new ObjectId(id));
            //Wird nicht gelöscht
            //if (entry != null)
            //{
            //    _save.Remove(new ObjectId(id));
            //}
            _hubContext.Clients.All.SendAsync("delete", id).Wait();
            return View("Index", entry);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
