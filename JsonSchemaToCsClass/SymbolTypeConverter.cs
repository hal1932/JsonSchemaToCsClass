using System;

namespace JsonSchemaToCsClass
{
    internal static class SymbolTypeConverter
    {
        public static string Convert(SymbolData symbol)
        {
            var nullable = (symbol.isNullable || !symbol.IsRequired && !symbol.isNullable);
            switch (symbol.TypeName)
            {
                case "integer": return (nullable) ? "int?" : "int";
                case "number": return (nullable) ? "double?" : "double";
                case "boolean": return (nullable) ? "bool?" : "bool";
                case "datetime": return (nullable) ? "System.DateTime?" : "System.DateTime";
                case "any": return "object";
                default: return symbol.TypeName;
            }
        }
    }
}
