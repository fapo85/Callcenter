using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Models
{
    public class NotifikationFactory
    {
        private readonly DBConnection dBConnection;
        public NotifikationFactory(DBConnection dBConnection)
        {
            this.dBConnection = dBConnection;
        }

        internal void Send(Notifikation notifikation, Organization organisation, Entry entry)
        {
            Console.WriteLine($"Notifikation Gesendet: {organisation.name} Telefonnummer: {entry.phone}");
        }
    }
}
