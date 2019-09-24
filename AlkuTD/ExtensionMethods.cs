using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlkuTD
{
    static class ExtensionMethods
    {
        public static string ZeroIfEmpty (this string s)
        {
            return string.IsNullOrEmpty(s) ? "0" : s;
        }
    }
}
