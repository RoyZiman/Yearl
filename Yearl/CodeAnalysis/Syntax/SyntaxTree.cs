using System.Collections.Immutable;
using Yearl.CodeAnalysis.Text;
using Yearl.Language.Syntax;
using static System.Net.Mime.MediaTypeNames;

namespace Yearl.Language.Syntax
{
    public sealed class SyntaxTree(SourceText code,ImmutableArray<Error> errors, SyntaxNode root, SyntaxToken endOfFileToken) : SyntaxNode
    {
        public SourceText Code { get; } = code;
        public ImmutableArray<Error> Errors { get; } = errors;
        public SyntaxNode Root { get; } = root;
        public SyntaxToken EndOfFileToken { get; } = endOfFileToken;

        public override SyntaxKind Kind => SyntaxKind.TreeStatement;

        public static SyntaxTree Parse(string code)
        {
            var sourceText = SourceText.From(code);
            return Parse(sourceText);
        }
        public static SyntaxTree Parse(SourceText code)
        {
            var parser = new Parser(code);
            return parser.Parse();
        }

        public static IEnumerable<SyntaxToken> ParseTokens(string text)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }

        public static IEnumerable<SyntaxToken> ParseTokens(SourceText text)
        {
            Lexer lexer = new Lexer(text);
            while (true)
            {
                SyntaxToken token = lexer.Lex();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;

                yield return token;
            }
        }


        public override string ToString()
        {
            return "\nSyntaxTree\n" + PrettyPrint(Root);
        }
        private static string PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            string output = "";

            output += indent + (isLast ? "└──" : "├──") + node.Kind;

            if (node is SyntaxToken t && t.Value != null)
                output += " " + t.Value;


            output += "\n";

            indent += isLast ? "    " : "│   ";

            SyntaxNode? lastChild = node.GetChildren().LastOrDefault();

            foreach (SyntaxNode child in node.GetChildren())
                output += PrettyPrint(child, indent, child == lastChild);

            return output;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            throw new Exception("Inaccessible due to invalid accessibility call");
        }
    }
}
