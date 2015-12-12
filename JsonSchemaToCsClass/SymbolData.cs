using System.Collections.Generic;

namespace JsonSchemaToCsClass
{
    internal class SymbolData
    {
        public enum AccessModifier
        {
            Public,
            Protected,
            Internal,
            Private,
        }

        public string TypeName
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public string Summary
        {
            get; set;
        }

        public bool IsArray
        {
            get; set;
        }

        public AccessModifier Modifier
        {
            get; set;
        }

        public List<SymbolData> Members
        {
            get; set;
        }

        public bool IsRequired
        {
            get; set;
        }

        public bool isNullable
        {
            get; set;
        }
    }
}
