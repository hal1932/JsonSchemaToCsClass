using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonSchemaToCsClass
{
    public class JsonSchema
    {
        internal JSchema RawSchema
        {
            get; private set;
        }

        public static JsonSchema Load(string path)
        {
            using (var stream = new StreamReader(path))
            using (var reader = new JsonTextReader(stream))
            {
                var schema = JSchema.Load(reader);
                return new JsonSchema(schema);
            }
        }

        private JsonSchema(JSchema rawSchema)
        {
            RawSchema = rawSchema;
        }
    }
}
