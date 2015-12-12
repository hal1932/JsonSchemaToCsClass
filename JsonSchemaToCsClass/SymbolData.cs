using System;
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

        public SymbolData Parent
        {
            get; private set;
        }

        public SymbolData(SymbolData parent)
        {
            Parent = parent;
        }

        public SymbolData CreateInstanceSymbol()
        {
            return new SymbolData(Parent)
            {
                TypeName = Name.ToClassName(),
                Name = Name,
                Summary = Summary,
                IsArray = IsArray,
                Modifier = Modifier,
                IsRequired = IsRequired,
                isNullable = isNullable,
            };
        }
    }
}
