using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace JsonSchemaToCsClass
{
    class Program
    {
        static void Main(string[] args)
        {
            var schema = JsonSchema.Load("basic.json");

            ///////////////////////////////////////////
            // JSON を C# 定義に変換
            ///////////////////////////////////////////
            var generator = new CsClassGenerator();
            generator.ParseSchema(schema);

            var option = new ClassConstructionOptions()
            {
                //Namespace = "Hoge.Foo",
                IsJsonSerializable = true,
            };
            generator.ConstructDeclaration(option);
            Console.WriteLine(generator.ToFullString());


            ///////////////////////////////////////////
            // 変換した C# 定義を文字列化してビルドしてみる
            ///////////////////////////////////////////
            var options = ScriptOptions.Default
                .WithMetadataResolver(
                    ScriptMetadataResolver.Default
                        .WithBaseDirectory(Environment.CurrentDirectory))
                .WithReferences("Newtonsoft.Json.dll");
            var script = CSharpScript.Create(generator.ToFullString(), options);
            foreach (var diag in script.Compile())
            {
                Console.WriteLine(diag);
            }

            IEnumerable<Type> rootSerializables = null;
            using (var stream = new MemoryStream())
            {
                if (script.GetCompilation().Emit(stream).Success)
                {
                    var assembly = Assembly.Load(stream.GetBuffer());
                    // ルート直下にあるJsonObjectAttributeを持ったクラス定義だけ抜き出す
                    var serializables = assembly.GetTypes()
                        .Where(type => type.CustomAttributes
                            .Any(attr => attr.AttributeType.Name == "JsonObjectAttribute"))
                        .ToArray();// ↓のWhereでserializables自身を参照するから、循環しないようにここで実体化しておく
                    rootSerializables = serializables
                        .Where(type => !serializables.Contains(type.ReflectedType));
                }
            }

            foreach (var type in rootSerializables)
            {
                Console.WriteLine(type.Name);
                foreach (var prop in type.GetProperties())
                {
                    Console.WriteLine("{0} {1}", prop.PropertyType.FullName, prop.Name);
                }

                // ここでリフレクションしながら適当な値を詰める

                // スキーマを満たすように値を詰めておけば、ここでシリアライズできる
                //var instance = Activator.CreateInstance(type);
                //var jsonStr = JsonConvert.SerializeObject(instance);
                //Console.WriteLine(jsonStr);
            }

            Console.Read();
        }
    }
}
