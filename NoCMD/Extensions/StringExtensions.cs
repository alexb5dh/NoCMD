using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoCMD.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string s, int length, string terminator)
        {
            if(terminator.Length > length) throw new ArgumentException("Terminator string length is larger than truncate length.", terminator);

            if (s.Length <= length) return s;
            return s.Substring(0, length - terminator.Length) + terminator;
        }
    }
}
