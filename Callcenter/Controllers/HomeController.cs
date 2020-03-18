using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Callcenter.Models;
using MongoDB.Bson;

namespace Callcenter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EntrySave _save;
        public HomeController(ILogger<HomeController> logger, EntrySave save)
        {
            _logger = logger;
            _save = save;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View(_save.GetNoZip());
        }
        [HttpPost]
        public IActionResult Index(string id, string phone, EntryRequest request, string zip)
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
                entry.Validate();
                _save.Add(entry);
            }
            else
            {
                entry = _save.Find(new ObjectId(id));
                if (entry == null)
                {
                    entry = new Entry();
                    entry.id = new ObjectId(id);
                }
                entry.phone = phone;
                entry.zip = zip;
                entry.request = request;
                entry.Validate();
                _save.Replace(entry);
            }
            return View(_save.GetNoZip());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
