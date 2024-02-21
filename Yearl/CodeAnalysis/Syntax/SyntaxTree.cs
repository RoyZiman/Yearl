using System.Collections.Immutable;
using Yearl.CodeAnalysis;
using Yearl.CodeAnalysis.Text;
using Yearl.Language.Syntax;
using static System.Net.Mime.MediaTypeNames;

namespace Yearl.Language.Syntax
{
    public sealed class SyntaxTree(SourceText text,ImmutableArray<Error> errors, SyntaxNode root, SyntaxToken endOfFileToken) : SyntaxNode
    {
        public SourceText Text { get; } = text;
        public ImmutableArray<Error> Errors { get; } = errors;
        public SyntaxNode Root { get; } = root;
        public SyntaxToken EndOfFileToken { get; } = endOfFileToken;

        public override SyntaxKind Kind => SyntaxKind.TreeStatement;

        public static SyntaxTree Parse(string text)
        {
            SourceText sourceText = SourceText.From(text);
            return Parse(sourceText);
        }
        public static SyntaxTree Parse(SourceText text)
        {
            Parser parser = new Parser(text);
            return parser.Parse();
        }

        public static IEnumerable<SyntaxToken> ParseTokens(string text)
        {
            SourceText sourceText = SourceText.From(text);
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
    }
}
