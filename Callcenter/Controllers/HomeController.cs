using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Callcenter.Models;
using MongoDB.Bson;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;

namespace Callcenter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DBConnection _save;
        public HomeController(ILogger<HomeController> logger, DBConnection save)
        {
            _logger = logger;
            _save = save;
        }
        /// <summary>
        /// Gibt eine ÜBersichtsseite Zurück, welche alle einträge ohne Zip Nummer enthält
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View(_save.GetNoZip());
        }
        /// <summary>
        /// Speichert einen Bearbeiteten Eintrag
        /// Es kann sich um einen existierenden sowie einen neuen Eintrag handeln
        /// </summary>
        /// <param name="id">id des eintrages, welcher gesucht werden soll</param>
        /// <param name="phone">telefonnummer</param>
        /// <param name="request">siehe enum EntryRequest</param>
        /// <param name="zip">Postleitzahl</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Index(string id, string phone, EntryRequest request, string zip)
        {
            Entry entry = null;
            try
            {
                if (String.IsNullOrWhiteSpace(zip))
                {
                    zip = "00000";
                }
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
            catch (Exception e)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Fehler: ");
                if (entry != null) { }
                DataContractJsonSerializer dcjs = new DataContractJsonSerializer(entry.GetType());
                using (MemoryStream ms = new MemoryStream())
                {
                    dcjs.WriteObject(ms, entry);
                    sb.AppendLine(Encoding.Default.GetString(ms.ToArray()));
                };
                sb.AppendLine(e.ToString());
                Console.WriteLine(sb.ToString());
                return BadRequest(e.Message);
            }
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
