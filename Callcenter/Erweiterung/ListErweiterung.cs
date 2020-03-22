using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Callcenter.Erweiterung
{
    public static class ListErweiterung
    {
        public static bool OneStartWith(this List<string> liste, string tosearch)
        {
            foreach(string item in liste)
            {
                if (tosearch.ToLower().StartsWith(item.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
