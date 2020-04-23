using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class TimeKapsel
    {
        private static DateTime NullPunkt = new DateTime(1970, 1, 1);

        private DateTime dateobj { get; set; }
        public double date { get {
                return ((double)((dateobj.ToUniversalTime().Ticks) - (NullPunkt.Ticks))) / TimeSpan.TicksPerMillisecond;
            }
            set {

                TimeSpan time = TimeSpan.FromMilliseconds(value);
                dateobj = NullPunkt + time;
            }
        }
        public TimeKapsel()
        {
        }
        public TimeKapsel(DateTime value)
        {
            this.dateobj = value;
        }

        public TimeKapsel(string value)
        {
            if(DateTime.TryParse(value, out var newobj))
            {
                dateobj = newobj;
            }
            else
            {
                dateobj = JsonSerializer.Deserialize<TimeKapsel>(value).dateobj;
            }
        }

        public TimeKapsel(double v)
        {
            date = v;
        }

        public DateTime ToLocalTime() => dateobj.ToLocalTime();
        public string ToLocalTimeString() => dateobj.ToLocalTime().ToString("d.M.yyyy HH:mm:ss");

        public static implicit operator DateTime(TimeKapsel d) => d == null? DateTime.Now : d.dateobj;
        public static implicit operator TimeKapsel(DateTime d) => new TimeKapsel(d);
        public static implicit operator BsonDateTime(TimeKapsel d) => (BsonDateTime)d.dateobj;
        public static implicit operator TimeKapsel(BsonDateTime d) => new TimeKapsel(d.ToUniversalTime());
    }
}
