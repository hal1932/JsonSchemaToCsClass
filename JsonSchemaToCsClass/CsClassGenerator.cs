using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace JsonSchemaToCsClass
{
    public class CsClassGenerator
    {
        public void ParseSchema(JsonSchema schema)
        {
            if (schema.RawSchema == null)
            {
                return;
            }

            if (schema.RawSchema.Type != JSchemaType.Object)
            {
                throw new ArgumentException("the type of the root element must be 'object'");
            }

            _rootSymbol = new SymbolData(null);

            var title = schema.RawSchema.Title ??
                "Class" + UniqueIndex.GetNext().ToString();
            ParseImpl(title, _rootSymbol, schema.RawSchema);
        }

        public void ConstructDeclaration(ClassConstructionOptions options)
        {
            _options = options;
            _rootNode = ConstructImpl(_rootSymbol) as ClassDeclarationSyntax;
        }

        public string ToFullString()
        {
            var unit = SF.CompilationUnit();
            if (_options.IsJsonSerializable)
            {
                unit = unit.AddUsings(SF.UsingDirective(SF.IdentifierName("Newtonsoft.Json")));
            }

            CSharpSyntaxNode rootNode;
            if (!string.IsNullOrEmpty(_options.Namespace))
            {
                rootNode = unit.AddMembers(
                    SF.NamespaceDeclaration(SF.IdentifierName(_options.Namespace))
                        .AddMembers(_rootNode));
            }
            else
            {
                rootNode = unit.AddMembers(_rootNode);
            }

            return Formatter.Format(rootNode, MSBuildWorkspace.Create()).ToFullString();
        }

        private void ParseImpl(string name, SymbolData node, JSchema rawSchema, bool isRequired = true)
        {
            var types = rawSchema.Type?.ToString()
                .Split(',')
                .Select(item => item.ToLower().Trim())
                .ToList();
            if (types.Contains("null"))
            {
                node.isNullable = true;
                types.Remove("null");
            }
            node.TypeName = types.First();

            if (node.TypeName == "string" && !string.IsNullOrEmpty(rawSchema.Format))
            {
                // http://json-schema.org/latest/json-schema-validation.html#anchor104
                switch (rawSchema.Format)
                {
                    case "date-time": node.TypeName = "datetime"; break;
                    default:
                        throw new ArgumentException("not-supported string format");
                }
            }

            if (node.TypeName == "array")
            {
                node.IsArray = true;
                node.TypeName = rawSchema.Items.First().Type?.ToString()
                    .Split(',')
                    .Select(item => item.ToLower().Trim())
                    .First();
            }

            node.Name = name;
            node.Summary = rawSchema.Description;
            node.Modifier = SymbolData.AccessModifier.Public;
            node.IsRequired = isRequired;

            if (node.TypeName == "object")
            {
                node.Members = new List<SymbolData>();
                foreach (var prop in rawSchema.Properties)
                {
                    var required = rawSchema.Required.Contains(prop.Key);

                    var member = new SymbolData(node);
                    ParseImpl(prop.Key, member, prop.Value, required);
                    node.Members.Add(member);
                }
            }
        }

        private CSharpSyntaxNode ConstructImpl(SymbolData symbol)
        {
            if (symbol.IsArray)
            {
                return CreateArray(symbol);
            }
            else
            {
                switch (symbol.TypeName)
                {
                    case "object": return CreateClass(symbol);
                    default: return CreateProperty(symbol);
                }
            }
        }

        private ClassDeclarationSyntax CreateClass(SymbolData symbol)
        {
            var className = symbol.Name.ToClassName();
            var node = SF.ClassDeclaration(className)
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword));

            if (!string.IsNullOrEmpty(symbol.Summary))
            {
                var comment = new DocumentComment() { Summary = symbol.Summary };
                node = node.WithLeadingTrivia(comment.ConstructTriviaList());
            }

            var props = new List<MemberDeclarationSyntax>();
            foreach (var member in symbol.Members)
            {
                props.Add(ConstructImpl(member) as MemberDeclarationSyntax);
                if (member.TypeName == "object")
                {
                    var childSymbol = member.CreateInstanceSymbol();
                    props.Add(CreateProperty(childSymbol));
                }
            }
            return node.AddMembers(props.ToArray());
        }

        private PropertyDeclarationSyntax CreateArray(SymbolData symbol)
        {
            var type = SF.ArrayType(
                SF.ParseTypeName(SymbolTypeConverter.Convert(symbol)),
                SF.List(new ArrayRankSpecifierSyntax[]
                {
                    SF.ArrayRankSpecifier(),
                }));
            return CreatePropertyImpl(type, symbol);
        }

        private PropertyDeclarationSyntax CreateProperty(SymbolData symbol)
        {
            var type = SF.ParseTypeName(SymbolTypeConverter.Convert(symbol));
            return CreatePropertyImpl(type, symbol);
        }

        private PropertyDeclarationSyntax CreatePropertyImpl(TypeSyntax type, SymbolData symbol)
        {
            // public [type] [symbol.Name] { get; set; }
            var node = SF.PropertyDeclaration(type, symbol.Name)
                .AddModifiers(SF.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)),
                    SF.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SF.Token(SyntaxKind.SemicolonToken)));

            if (_options.IsJsonSerializable)
            {
                node = node.WithAttributeLists(CreateSerializationAttribute(symbol));
            }

            if (!string.IsNullOrEmpty(symbol.Summary))
            {
                var comment = new DocumentComment() { Summary = symbol.Summary };
                node = node.WithLeadingTrivia(comment.ConstructTriviaList());
            }

            return node;
        }

        private SyntaxList<AttributeListSyntax> CreateSerializationAttribute(SymbolData symbol)
        {
            string requiredValueName;
            if (symbol.IsRequired)
            {
                requiredValueName = (symbol.isNullable) ? "AllowNull" : "Always";
            }
            else
            {
                requiredValueName = "Default";
            }

            Func<string, string, string, AttributeArgumentSyntax> createAttr =
                (lhs, expr, name) =>
                {
                    // lhs = expr.name
                    return SF.AttributeArgument(
                        SF.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SF.IdentifierName(expr),
                            SF.Token(SyntaxKind.DotToken),
                            SF.IdentifierName(name)))
                        .WithNameEquals(
                            SF.NameEquals(SF.IdentifierName(lhs)));
                };

            var attributes = SF.SeparatedList(new[]
            {
                // Required = Required.[requiredValueName]
                createAttr("Required", "Required", requiredValueName),
            });
            if (requiredValueName == "Default" && !symbol.isNullable)
            {
                // NullValueHandling = NullValueHandling.Ignore
                attributes = attributes.Add(
                    createAttr("NullValueHandling", "NullValueHandling", "Ignore"));
            }

            return SF.SingletonList(
                SF.AttributeList(
                    SF.SingletonSeparatedList(
                        SF.Attribute(SF.IdentifierName("JsonProperty"))
                            .WithArgumentList(
                                SF.AttributeArgumentList(attributes)))));
        }

        private ClassConstructionOptions _options;
        private SymbolData _rootSymbol;
        private ClassDeclarationSyntax _rootNode;
    }
}
