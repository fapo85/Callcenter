using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class TimeKapsel
    {
        private double v;

        private DateTime dateobj { get; set; }
        public double date { get {
                return dateobj.Millisecond;
            }
            set {

                TimeSpan time = TimeSpan.FromMilliseconds(value);
                dateobj = new DateTime(1970, 1, 1) + time;
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
            dateobj = DateTime.Parse(value);
        }

        public TimeKapsel(double v)
        {
            date = v;
        }

        public DateTime ToLocalTime() => dateobj.ToLocalTime();
        public string ToLocalTimeString() => dateobj.ToLocalTime().ToString("d.M.yyyy HH:mm:ss");

        public static implicit operator DateTime(TimeKapsel d) => d.dateobj;
        public static implicit operator TimeKapsel(DateTime d) => new TimeKapsel(d);
        public static implicit operator BsonDateTime(TimeKapsel d) => (BsonDateTime)d.dateobj;
        public static implicit operator TimeKapsel(BsonDateTime d) => new TimeKapsel(d.ToUniversalTime());
    }
}
