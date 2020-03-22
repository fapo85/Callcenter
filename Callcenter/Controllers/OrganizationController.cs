using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Callcenter.Models;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Generic;

namespace Callcenter.Controllers
{
    public class OrganizationController : Controller
    {
        private readonly ILogger<Organization> _logger;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly DBConnection _save;
        public OrganizationController(ILogger<Organization> logger, IHubContext<SignalRHub> hubContext, DBConnection save)
        {
            _logger = logger;
            _hubContext = hubContext;
            _save = save;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/Organization/Add")]
        public IActionResult AddOrganization()
        {

            return AddOrganization(new Organization()
            {
                zips = new List<string>(),
                notifyrequest = new List<EntryRequest>()
            });
        }
        [HttpGet("/Organization/Add/{id}")]
        public IActionResult AddOrganization(string id)
        {
            return AddOrganization(_save.FindOrganization(id));
        }
        [HttpGet("/Organization/Search/")]
        public IEnumerable<OrganizationTrasport> SearchOrganisation()
        {
            return SearchOrganisation(null);
        }
        [HttpGet("/Organization/Search/{search}")]
        public IEnumerable<OrganizationTrasport> SearchOrganisation(string search)
        {
            return SearchRevOrganisation(false, search);
        }
        [HttpGet("/Organization/SearchRev/{zipreserve}")]
        public IEnumerable<OrganizationTrasport> SearchOrganisation(bool zipreserve)
        {
            return SearchRevOrganisation(zipreserve, null);
        }
        [HttpGet("/Organization/SearchRev/{zipreserve}/{search}/")]
        public IEnumerable<OrganizationTrasport> SearchRevOrganisation(bool zipreserve, string search)
        {
            IEnumerable<Organization> liste = string.IsNullOrWhiteSpace(search) ? _save.GetOrganisations() : _save.FindOrganisations(search, zipreserve);
            foreach (Organization orga in liste)
            {
                yield return new OrganizationTrasport()
                {
                    id = orga.id.ToString(),
                    name = orga.name,
                    ansprechpartner = orga.ansprechpartner,
                    zips = orga.GetZipString(false),
                    timestamp = orga.timestamp.ToString("de"),
                    notifyrequest = orga.NotifyRequestString()
                };
            }
        }


        public IActionResult AddOrganization(Organization entry, string msg = null)
        {
            return View("Add", entry);
        }
        [HttpPost]
        public IActionResult Send(string id, string name, string ansprechpartner, string email, string zip, int NotifyRequest1, int NotifyRequest2, int NotifyRequest3, int NotifyRequest4)
        {
            Organization entry = null;
            List<EntryRequest> entryRequests = new List<EntryRequest>();
            if (NotifyRequest1 > 0)
                entryRequests.Add(EntryRequest.Einkäufe);
            if (NotifyRequest2 > 0)
                entryRequests.Add(EntryRequest.Haustiere);
            if (NotifyRequest3 > 0)
                entryRequests.Add(EntryRequest.Reparaturen);
            if (NotifyRequest4 > 0)
                entryRequests.Add(EntryRequest.Sonstiges);
            try
            {
                if (!(String.IsNullOrWhiteSpace(id) || id.Equals("000000000000000000000000")))
                {
                    entry = _save.FindOrganization(id);
                    if (entry != null)
                    {
                        entry.name = name;
                        entry.ansprechpartner = ansprechpartner;
                        entry.email = email;
                        entry.zips = Organization.ParseZips(zip);
                        entry.notifyrequest = entryRequests;
                        _save.UpdateOrganization(entry);
                    }
                }
                if (entry == null)
                {
                    entry = new Organization()
                    {
                        id = MongoDB.Bson.ObjectId.GenerateNewId(),
                        timestamp = DateTime.Now,
                        name = name,
                        ansprechpartner = ansprechpartner,
                        email = email,
                        zips = Organization.ParseZips(zip),
                        notifyrequest = entryRequests
                    };
                    _save.AddOrganization(entry);
                }
                return View(entry);
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
