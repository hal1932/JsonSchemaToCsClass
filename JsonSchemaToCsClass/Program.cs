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

            var generator = new CsClassGenerator();
            generator.ParseSchema(schema);

            var option = new ClassConstructionOptions()
            {
                //Namespace = "Hoge.Foo",
                IsJsonSerializable = true,
            };
            generator.ConstructDeclaration(option);
            Console.WriteLine(generator.ToFullString());

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
#if false
            Script<object> script = null;
            DateTime begin;

            var sourceCode = @"
                using System;
                using System.Collections.Generic;
                public class TestData
                {
                    public int Item { get; set; }
                }
                var typeDic = new Dictionary<string, Type>()
                {
                    { ""TestData"", typeof(TestData) },
                };
                var types = System.Reflection.Assembly.GetExecutingAssembly().ExportedTypes;
                return typeof(TestData);
            ";

            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Task.Factory.StartNew(() =>
            {
                begin = DateTime.Now;
                script = CSharpScript.Create(sourceCode);
                Console.WriteLine("create {0}", (DateTime.Now - begin).TotalMilliseconds);

                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                begin = DateTime.Now;
                foreach (var diag in script.Compile())
                {
                    Console.WriteLine(diag);
                }
                Console.WriteLine("compile {0}", (DateTime.Now - begin).TotalMilliseconds);
            }).Wait();

            begin = DateTime.Now;
            var state = script.RunAsync().Result;
            Console.WriteLine("run {0}", (DateTime.Now - begin).TotalMilliseconds);

            // 1. 返り値で受け取る
            foreach (var prop in (state.ReturnValue as Type).GetProperties())
            {
                Console.WriteLine(prop.Name);
            }

            // 2. 変数にいれとく
            var typeDic = state.Variables.First(v => v.Name == "typeDic").Value as Dictionary<string, Type>;
            foreach (var item in typeDic)
            {
                foreach (var prop in item.Value.GetProperties())
                {
                    Console.WriteLine(prop.Name);
                }
            }

            // 3. コンパイル結果をメモリ上に展開してアセンブリ生成
            using (var stream = new MemoryStream())
            {
                begin = DateTime.Now;
                if (script.GetCompilation().Emit(stream).Success)
                {
                    Console.WriteLine("emit {0}", (DateTime.Now - begin).TotalMilliseconds);
                    begin = DateTime.Now;
                    var asm = Assembly.Load(stream.GetBuffer());
                    Console.WriteLine("load {0}", (DateTime.Now - begin).TotalMilliseconds);
                    var type = asm.GetTypes().First(t => t.Name == "TestData");
                    foreach (var prop in type.GetProperties())
                    {
                        Console.WriteLine(prop.Name);
                    }
                }
            }
#endif

            Console.Read();
        }
    }
}
