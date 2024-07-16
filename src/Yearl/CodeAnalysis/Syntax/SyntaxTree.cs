using System.Collections.Immutable;
using Yearl.CodeAnalysis.Errors;
using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxTree
    {
        private delegate void ParseHandler(SyntaxTree syntaxTree,
                                           out SyntaxUnitCompilation root,
                                           out ImmutableArray<Error> diagnostics);
        private SyntaxTree(SourceText text, ParseHandler handler)
        {
            Text = text;

            handler(this, out var root, out var errors);

            Errors = errors;
            Root = root;
        }

        public SourceText Text { get; }
        public ImmutableArray<Error> Errors { get; }
        public SyntaxUnitCompilation Root { get; }

        public static SyntaxTree Load(string fileName)
        {
            string text = File.ReadAllText(fileName);
            var sourceText = SourceText.From(text, fileName);
            return Parse(sourceText);
        }

        private static void Parse(SyntaxTree syntaxTree, out SyntaxUnitCompilation root, out ImmutableArray<Error> errors)
        {
            Parser parser = new(syntaxTree);
            root = parser.ParseCompilationUnit();
            errors = [.. parser.Errors];
        }


        public static SyntaxTree Parse(string text)
        {
            var sourceText = SourceText.From(text);
            return Parse(sourceText);
        }

        public static SyntaxTree Parse(SourceText text) => new(text, Parse);

        public static ImmutableArray<SyntaxToken> ParseTokens(string text)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }

        public static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Error> errors)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText, out errors);
        }
        public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text) => ParseTokens(text, out _);
        public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text, out ImmutableArray<Error> errors)
        {
            List<SyntaxToken> tokens = [];

            void ParseTokens(SyntaxTree st, out SyntaxUnitCompilation root, out ImmutableArray<Error> errors)
            {
                Lexer l = new(st);
                while (true)
                {
                    var token = l.Lex();
                    if (token.Kind == SyntaxKind.EndOfFileToken)
                    {
                        root = new SyntaxUnitCompilation(st, [], token);
                        break;
                    }

                    tokens.Add(token);
                }

                errors = [.. l.Errors];
            }

            SyntaxTree syntaxTree = new(text, ParseTokens);
            errors = [.. syntaxTree.Errors];
            return [.. tokens];
        }
    }
}