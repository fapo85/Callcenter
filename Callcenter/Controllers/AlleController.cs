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
    public class AlleController : Controller
    {
        private readonly ILogger<AlleController> _logger;
        private readonly EntrySave _save;
        public AlleController(ILogger<AlleController> logger, EntrySave save)
        {
            _logger = logger;
            _save = save;
        }
        public IActionResult Index()
        {
            return View(_save.GetAll());
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
