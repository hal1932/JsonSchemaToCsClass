# JsonSchemaToCsClass
- Json文字列をC#のクラス定義に変換します。

- 変換元の例（basic.json）
```
{
  "title": "Example Schema",
  "description": "A product from Acme's catalog",
  "type": "object",
  "properties": {
    "firstName": {
      "type": [ "string" ]
    },
    "lastName": {
      "type": [ "string", "null" ]
    },
    "age": {
      "description": "Age in years",
      "type": "integer",
      "minimum": 0
    },
    "birthday": {
      "type": "string",
      "format": "date-time"
    },
    "test": {
      "type": "object",
      "description": "test class",
      "properties": {
        "id": {
          "type": "integer",
          "description": "test id"
        },
        "data": {
          "type": "array",
          "description": "test array",
          "items": {
            "type": "string"
          }
        }
      },
      "required": [ "id" ]
    }
  },
  "required": [ "firstName" ]
}
```

- 変換先の例（Program.csを実行するとstdoutに出力）
```
using Newtonsoft.Json;
/// <Summary>A product from Acme's catalog</Summary>
[JsonObject]
public class ExampleSchemaClass
{
    [JsonProperty(Required = Required.Always)]
    public string firstName { get; set; }
    [JsonProperty(Required = Required.Default)]
    public string lastName { get; set; }
    /// <Summary>Age in years</Summary>
    [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public int? age { get; set; }
    [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public System.DateTime? birthday { get; set; }
    /// <Summary>test class</Summary>
    [JsonObject]
    public class TestClass
    {
        /// <Summary>test id</Summary>
        [JsonProperty(Required = Required.Always)]
        public int id { get; set; }
        /// <Summary>test array</Summary>
        [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public string[] data { get; set; }
    }
    /// <Summary>test class</Summary>
    [JsonProperty(Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public TestClass test { get; set; }
}
```
