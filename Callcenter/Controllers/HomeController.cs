using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Callcenter.Models;

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
        public IActionResult Index(string phone, EntryRequest request, string zip)
        {
            if (String.IsNullOrWhiteSpace(zip))
            {
                zip = "00000";
            }
            Entry entry = new Entry()
            {
                //id = RandomString(14),
                timestamp = DateTime.Now,
                phone = phone,
                zip = zip,
                request = request
            };
            _save.Add(entry);
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
