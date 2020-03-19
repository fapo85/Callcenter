using CaptchaGen.NetCore;
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
        private static readonly TimeSpan GÜLTIGKEIT = TimeSpan.FromMinutes(15);
        private static readonly int IDLENTH = 15;
        private static readonly int SECRETLENTH = 5;

        private readonly Random random = new Random();
        private readonly Dictionary<string, Captcha> Save = new Dictionary<string, Captcha>();
        public Captcha Generate()
        {
            Captcha capacha = GenerateNewCpatcha();
            //sudo apt install libgdiplus
            using (FileStream fs = File.OpenWrite(capacha.ImgFSPath))
            using (Stream picStream = ImageFactory.BuildImage(capacha.Secret, 50, 100, 20, 10, ImageFormatType.Jpeg))
            {
                picStream.CopyTo(fs);
            }
            return capacha;
        }

        
        private Captcha GenerateNewCpatcha()
        {
            string secret = RandomString(SECRETLENTH);
            string id;
            lock (Save)
            {
                do
                {
                    id = RandomString(IDLENTH);
                } while (Save.ContainsKey(id));
                Captcha captcha = new Captcha(id, secret);
                Save.Add(id, captcha);
                return captcha;
            }
        }
        public bool VerifyAndDelete(string id, string secret)
        {
            Cleanup();
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                return false;
            lock (Save)
            {
                if(Save.TryGetValue(id, out Captcha captcha))
                {
                    if (secret.Equals(captcha.Secret)){
                        Cleanup(captcha);
                        return true;
                    }
                }
            }
            return false;
        }

        private void Cleanup(Captcha captcha)
        {
            File.Delete(captcha.ImgFSPath);
            Save.Remove(captcha.id);
        }

        private void Cleanup()
        {
            lock (Save)
            {
                IEnumerable<Captcha> toDelete = Save.Values.Where(c => c.Timestamp < DateTime.Now.Subtract(GÜLTIGKEIT));
                foreach(Captcha captcha in toDelete)
                {
                    Cleanup(captcha);
                }
            }
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
