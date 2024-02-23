using System.Collections.Immutable;
using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxTree
    {
        private SyntaxTree(SourceText text)
        {
            Parser parser = new(text);
            SyntaxUnitCompilation root = parser.ParseCompilationUnit();
            ImmutableArray<Error> errors = parser.Errors.ToImmutableArray();

            Text = text;
            Errors = errors;
            Root = root;
        }

        public SourceText Text { get; }
        public ImmutableArray<Error> Errors { get; }
        public SyntaxUnitCompilation Root { get; }

        public static SyntaxTree Parse(string text)
        {
            SourceText sourceText = SourceText.From(text);
            return Parse(sourceText);
        }
        public static SyntaxTree Parse(SourceText text)
        {
            return new SyntaxTree(text);
        }

        public static IEnumerable<SyntaxToken> ParseTokens(string text)
        {
            SourceText sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }
        public static IEnumerable<SyntaxToken> ParseTokens(SourceText text)
        {
            Lexer lexer = new(text);
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
