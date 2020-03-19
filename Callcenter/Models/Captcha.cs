using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class Captcha
    {
        public string id { get; }
        public string Secret { get; }
        public DateTime Timestamp { get; }
        public Captcha(string id, string Secret)
        {
            this.id = id;
            this.Secret = Secret;
            this.Timestamp = DateTime.Now;
        }
        public string ImgPath => "/captcha/" + id + ".jpg";
        public string ImgFSPath => "wwwroot/captcha/" + id + ".jpg";
    }
}
