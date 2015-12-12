using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSchemaToCsClass
{
    internal static class SymbolTypeConverter
    {
        public static string Convert(string symbolType)
        {
            switch (symbolType)
            {
                case "integer": return "int";
                case "number": return "double";
                case "boolean": return "bool";
                case "any": return "object";
                default: return symbolType;
            }
        }

        public static string ConvertBack(string csType)
        {
            throw new NotImplementedException();
        }
    }
}
