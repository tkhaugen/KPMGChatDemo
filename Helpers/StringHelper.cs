using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Helpers
{
    public static class StringHelper
    {
        public static string Capitalize(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s.Substring(0, 1).ToUpper() + s.Substring(1);
        }

        public static string AppendNewline(this string s)
        {
            return (s ?? "") + "  \n";
        }

        public static string MakeEmailAddress(string name, string address)
        {
            return $"{name} <{address}>";
        }
    }
}