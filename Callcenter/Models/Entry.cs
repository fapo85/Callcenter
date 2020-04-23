using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class Entry
    {
        public Entry() { }

        [BsonId]
        public ObjectId id { get; set; }
        [BsonSerializer(typeof(TimeKapselConverter))]
        public TimeKapsel timestamp { get; set; }
        [BsonSerializer(typeof(TimeKapselConverter))]
        public TimeKapsel modifyts { get; set; }
        [BsonSerializer(typeof(TimeKapselConverter))]
        public TimeKapsel finishts { get; set; }
        public bool IsDeleted => finishts!=null;
        public string phone { get; set; }
        public string zip { get; set; }
        public EntryRequest request { get; set; }
        public bool marked { get; set; }
        public object __v { get; set; } = "1";
        public string CString => marked ? "other" : string.Empty;
        public EntryFill TrasportModel => new EntryFill()
        {
            id = id.ToString(),
            timestamp = timestamp,
            modifyts = modifyts,
            phone = phone,
            zip = zip,
            request = request.ToString(),
            requestid = (int)request,
            deleted = IsDeleted,
            marked = marked
        };
        internal static int Compare(Entry x, Entry y)
        {
            return DateTime.Compare(x.timestamp, y.timestamp);
        }
        public void Validate()
        {
            phone = phone.Trim();
            phone = phone.Replace(" ", string.Empty);
            phone = phone.Replace("-", string.Empty);
            phone = phone.Replace("\t", string.Empty);
            if (phone.StartsWith('+')){
                phone = phone.Substring(1, phone.Length -1);
                phone = "00" + phone;
            }
            ValidateTel(phone);
            zip = zip.Trim();
            ValidateZip(zip);
        }
        public static void ValidateTel(string phoneNumber)
        {
            if (phoneNumber.Length < 6)
                throw new Exception("Die Telefonnummer ist zu kurz.");
            string firstFour = phoneNumber.Substring(0, 4);

            if (firstFour.StartsWith("00") && firstFour != "0049")
            {
                throw new Exception("Nur Telefonnummern aus Deutschland zulässig!");
            }
            else if (firstFour == "0049")
            {
                firstFour = "0" + phoneNumber.Substring(4, 3);
            }

            switch (firstFour)
            {
                case "0137":
                case "0700":
                case "0900":
                case "0180":
                case "0190":
                case "1180":
                    throw new Exception("Rufnummer nicht erlaubt!");
            }
            foreach (char c in phoneNumber.ToCharArray())
            {
                if (!char.IsDigit(c))
                {
                    throw new Exception("Die Telefonnummer besteht nicht nur aus Zahlen.");
                }
            }
            if (!TelRgx.IsMatch(phoneNumber))
            {
                throw new Exception("Die Telefonnummer ist ungültig.");
            }
        }
        public static void ValidateZip(string zip)
        {
            if (zip.Length != 5 || !ZipRgx.IsMatch(zip)){
                throw new Exception("Die Postleitzahl ist ungültig.");
            }
        }

        private static readonly Regex TelRgx = new Regex(@"^(0049\d{5,}|0[1-9]\d{4,}|\+49\d{5,})$");
        private static readonly Regex ZipRgx = new Regex(@"^\d{5}$");
    }
}
