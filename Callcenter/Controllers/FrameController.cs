using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Callcenter.Models;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;

namespace Callcenter.Controllers
{
    public class FrameController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly DBConnection _save;
        private readonly CaptchaFactory capatchaFactory;
        public FrameController(ILogger<HomeController> logger, IHubContext<SignalRHub> hubContext, DBConnection save)
        {
            _logger = logger;
            _hubContext = hubContext;
            _save = save;
            capatchaFactory = new CaptchaFactory(save);
        }

        public IActionResult Index()
        {
            return Redirect("https://www.krisenkultur.de");
        }

        [HttpGet("/Frame/Add")]
        public IActionResult AddFrame()
        {

            return AddFrame(new Entry());
        }
        [HttpGet("/Frame/Captcha/{id}")]
        public IActionResult GetCaptcha(string id) => new FileStreamResult(new MemoryStream(capatchaFactory.GetImgBytes(id)), "image/png");

        public IActionResult AddFrame(Entry entry, string msg = null)
        {
            Captcha captcha = capatchaFactory.Generate();
            ViewData["captchaid"] = captcha.id;
            ViewData["msg"] = msg;
            return View("Add", entry);
        }
        [HttpPost]
        public IActionResult Send(string id, string phone, EntryRequest request, string zip, string captchasecret, string captchaid)
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
                    if(capatchaFactory.VerifyAndDelete(captchaid, captchasecret.ToUpper()))
                    {
                        entry.Validate();
                       _save.Add(entry);
                    }else{
                        return AddFrame(entry, "Captcha nicht gelöst");
                    }
                }
                else
                {
                    throw new NotSupportedException("Bearbeiten nicht erlaubt");
                }
                return View(entry);
            }catch(Exception e)
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
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [HttpGet("/Frame/CheckStatus")]
        public IActionResult CheckStatus()
        {
            return Ok();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
      
    }
}
