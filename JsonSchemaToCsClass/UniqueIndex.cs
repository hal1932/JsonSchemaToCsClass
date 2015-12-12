using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSchemaToCsClass
{
    internal static class UniqueIndex
    {
        public static int GetNext()
        {
            return _currentId++;
        }

        private static int _currentId;
    }
}
