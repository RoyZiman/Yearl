using System.Collections.Immutable;
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
            var text = File.ReadAllText(fileName);
            var sourceText = SourceText.From(text, fileName);
            return Parse(sourceText);
        }

        private static void Parse(SyntaxTree syntaxTree, out SyntaxUnitCompilation root, out ImmutableArray<Error> diagnostics)
        {
            var parser = new Parser(syntaxTree);
            root = parser.ParseCompilationUnit();
            diagnostics = parser.Errors.ToImmutableArray();
        }

        public static SyntaxTree Parse(string text)
        {
            SourceText sourceText = SourceText.From(text);
            return Parse(sourceText);
        }

        public static SyntaxTree Parse(SourceText text)
        {
            return new SyntaxTree(text, Parse);
        }

        public static ImmutableArray<SyntaxToken> ParseTokens(string text)
        {
            SourceText sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }

        public static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Error> errors)
        {
            SourceText sourceText = SourceText.From(text);
            return ParseTokens(sourceText, out errors);
        }
        public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text)
        {
            return ParseTokens(text, out _);
        }
        public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text, out ImmutableArray<Error> errors)
        {
            var tokens = new List<SyntaxToken>();

            void ParseTokens(SyntaxTree st, out SyntaxUnitCompilation root, out ImmutableArray<Error> d)
            {
                root = null;

                var l = new Lexer(st);
                while (true)
                {
                    var token = l.Lex();
                    if (token.Kind == SyntaxKind.EndOfFileToken)
                    {
                        root = new SyntaxUnitCompilation(st, ImmutableArray<SyntaxMember>.Empty, token);
                        break;
                    }

                    tokens.Add(token);
                }

                d = l.Errors.ToImmutableArray();
            }

            var syntaxTree = new SyntaxTree(text, ParseTokens);
            errors = syntaxTree.Errors.ToImmutableArray();
            return tokens.ToImmutableArray();
        }
    }
}