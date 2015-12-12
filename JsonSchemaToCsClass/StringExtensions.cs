using System.Linq;

namespace JsonSchemaToCsClass
{
    internal static class StringExtensions
    {
        public static string ToClassName(this string str)
        {
            return str.ToCamelCase() + "Class";
        }

        public static string ToCamelCase(this string str)
        {
            return string.Join("",
                str.Split('_', ' ')
                    .Where(item => !string.IsNullOrEmpty(item))
                    .Select(item => item.First().ToString().ToUpper() + item.Substring(1)));
        }
    }
}
