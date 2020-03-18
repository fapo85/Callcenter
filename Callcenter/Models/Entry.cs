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
        public DateTime timestamp { get; set; }
        public DateTime? modifyts { get; set; }
        public string phone { get; set; }
        public string zip { get; set; }
        public EntryRequest request { get; set; }
        public bool marked { get; set; }
        public object __v { get; set; }
        public string CString => marked ? "other" : string.Empty;

        internal static int Compare(Entry x, Entry y)
        {
            return DateTime.Compare(x.timestamp, y.timestamp);
        }
        public void Validate()
        {
            phone = phone.Trim();
            phone = phone.Replace(" ", string.Empty);
            phone = phone.Replace("\t", string.Empty);
            if (phone.StartsWith('+')){
                phone = phone.Substring(0, 1);
                phone = "00" + phone;
            }
            ValidateTel(phone);
            zip = zip.Trim();
            ValidateZip(zip);
        }
        public static void ValidateTel(string phoneNumber)
        {
            if (phoneNumber.Length < 8)
                throw new Exception("Nummer zu klein");
            string firstFour = phoneNumber.Substring(0, 4);

            if (firstFour == "00" && firstFour != "0049")
            {
                throw new Exception("Not a german number!");
            }
            else if (firstFour == "0049")
            {
                firstFour = "0" + phoneNumber.Substring(4, 7);
            }

            switch (firstFour)
            {
                case "0137":
                case "0700":
                case "0900":
                case "0180":
                case "0190":
                case "1180":
                    throw new Exception("Number not allowed!");
            }
            foreach (char c in phoneNumber.ToCharArray())
            {
                if (!char.IsDigit(c))
                {
                    throw new Exception("Phone besteht nicht nur aus Buchstaben.");
                }
            }
            Regex rgx = new Regex(@"^(0049|+49|0)\d{7,}?$");
            if (!rgx.IsMatch(phoneNumber))
            {
                throw new Exception("Phone RegEX Fehler.");
            }
        }
        public static void ValidateZip(string zip)
        {
            if (zip.Length != 5){
                throw new Exception("Zip ist nicht länge 5");
            }
            foreach(char c in zip.ToCharArray()){
                if (!char.IsDigit(c)){
                    throw new Exception("Zip besteht nicht nur aus Buchstaben.");
                }
            }
        }
    }
}
