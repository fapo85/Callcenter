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
    public class FrameController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly EntrySave _save;
        public FrameController(ILogger<HomeController> logger, IHubContext<SignalRHub> hubContext, EntrySave save)
        {
            _logger = logger;
            _hubContext = hubContext;
            _save = save;
        }

        public IActionResult Index()
        {
            return View(new Entry());
        }

        [HttpGet("/Frame/Add")]
        public IActionResult AddFrame()
        {
            return View("Add", new Entry());
        }
        [HttpPost]
        public IActionResult Send(string id, string phone, EntryRequest request, string zip)
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
                    phone = phone,
                    zip = zip,
                    request = request
                };
                _save.Add(entry);
            }
            else
            {
                throw new NotSupportedException("Bearbeiten nicht erlaubt");
            }
            return View(entry);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
