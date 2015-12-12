using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSchemaToCsClass
{
    public class ClassConstructionOptions
    {
        public string Namespace
        {
            get; set;
        }

        public bool IsJsonSerializable
        {
            get; set;
        }
    }
}
