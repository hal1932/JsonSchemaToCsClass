using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace JsonSchemaToCsClass
{
    internal class DocumentComment
    {
        public string Summary
        {
            get; set;
        }

        public SyntaxTriviaList ConstructTriviaList()
        {
            return SF.TriviaList(
                        SF.Trivia(
                            SF.DocumentationCommentTrivia(
                                SyntaxKind.MultiLineDocumentationCommentTrivia,
                                CreateDocumentElement("Summary", Summary))));
        }

        private SyntaxList<XmlNodeSyntax> CreateDocumentElement(string tag, string value)
        {
            return SF.List(
                new XmlNodeSyntax[]
                {
                    CreateCommentLeading(),
                    CreateXmlElement("Summary", Summary),
                });
        }

        private XmlTextSyntax CreateCommentLeading()
        {
            return SF.XmlText().WithTextTokens(
                SF.TokenList(
                    SF.XmlTextLiteral(
                        SF.TriviaList(
                            SF.DocumentationCommentExterior("///")),
                        " ",
                        " ",
                        SF.TriviaList())));
        }

        private XmlElementSyntax CreateXmlElement(string tag, string value)
        {
            return SF.XmlElement(
                SF.XmlElementStartTag(SF.XmlName(tag)),
                SF.SingletonList<XmlNodeSyntax>(
                    SF.XmlText(
                        SF.TokenList(
                            new[] {
                                SF.XmlTextLiteral(
                                    SF.TriviaList(),
                                    value,
                                    value,
                                    SF.TriviaList())
                            }))),
                SF.XmlElementEndTag(SF.XmlName(tag)));
        }
    }
}
