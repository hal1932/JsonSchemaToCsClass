using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSchemaToCsClass
{
    internal static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            return string.Join("",
                str.Split('_', ' ')
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Select(item => item.First().ToString().ToUpper() + item.Substring(1)));
        }
    }
}
