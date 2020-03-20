
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class CaptchaFactory
    {
        public static readonly TimeSpan GÜLTIGKEIT = TimeSpan.FromMinutes(15);
        public const int SECRETLENTH = 5;

        private readonly DBConnection save;
        //private readonly Dictionary<string, Captcha> Save = new Dictionary<string, Captcha>();
        public CaptchaFactory(DBConnection save)
        {
            this.save = save;
        }

        public Captcha Generate()
        {
            string secret = GenSecret();
            Captcha captcha = new Captcha(MongoDB.Bson.ObjectId.GenerateNewId(), secret);
            save.AddCaptcha(captcha);
            return captcha;
        }

        internal byte[] GetImgBytes(string id)
        {
            Captcha captcha = save.GetCaptcha(id);
            if (captcha == null)
                throw new FileNotFoundException("id nicht Gefunden");
            return captcha.CaptchaByteData();
        }

        public bool VerifyAndDelete(string id, string secret)
        {
            save.CleanupCaptcha();
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                return false;
            Captcha captcha = save.GetCaptcha(id);
            if (captcha != null)
            {
                if (secret.ToUpper().Equals(captcha.Secret))
                {
                    Cleanup(captcha);
                    return true;
                }
            }
            return false;
        }

        private void Cleanup(Captcha captcha)
        {
            save.RemoveCaptcha(captcha);
        }


        private string GenSecret()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, SECRETLENTH)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
