using Yearl.CodeAnalysis.Syntax;
using Yearl.CodeAnalysis.Text;

namespace Yearl.Tests.CodeAnalysis.Syntax
{
    public class LexerTests
    {
        [Fact]
        public void Lexer_Lexes_UnterminatedString()
        {
            string text = "\"text";
            var tokens = SyntaxTree.ParseTokens(text, out var diagnostics);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.StringToken, token.Kind);
            Assert.Equal(text, token.Text);

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal(new TextSpan(0, 1), diagnostic.Location.Span);
            Assert.Equal("Unterminated string literal.", diagnostic.Message);
        }

        [Fact]
        public void Lexer_Covers_AllTokens()
        {
            var tokenKinds = Enum.GetValues(typeof(SyntaxKind))
                                 .Cast<SyntaxKind>()
                                 .Where(k => k.IsToken());

            var testedTokenKinds = GetTokens().Concat(GetSeparators()).Select(t => t.kind);

            SortedSet<SyntaxKind> untestedTokenKinds = new(tokenKinds);
            untestedTokenKinds.Remove(SyntaxKind.InvalidTokenTrivia);
            untestedTokenKinds.Remove(SyntaxKind.EndOfFileToken);
            untestedTokenKinds.ExceptWith(testedTokenKinds);

            Assert.Empty(untestedTokenKinds);
        }

        [Theory]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Token(SyntaxKind kind, string text)
        {
            IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("foo42")]
        [InlineData("foo_42")]
        [InlineData("_foo")]
        public void Lexer_Lexes_Identifiers(string name)
        {
            var tokens = SyntaxTree.ParseTokens(name).ToArray();

            Assert.Single(tokens);

            var token = tokens[0];
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind);
            Assert.Equal(name, token.Text);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairsData))]
        public void Lexer_Lexes_TokenPairs(SyntaxKind t1Kind, string t1Text,
                                           SyntaxKind t2Kind, string t2Text)
        {
            string text = t1Text + t2Text;
            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            Assert.Equal(2, tokens.Length);
            Assert.Equal(tokens[0].Kind, t1Kind);
            Assert.Equal(tokens[0].Text, t1Text);
            Assert.Equal(tokens[1].Kind, t2Kind);
            Assert.Equal(tokens[1].Text, t2Text);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairsWithSeparatorData))]
        public void Lexer_Lexes_TokenPairs_WithSeparators(SyntaxKind t1Kind, string t1Text,
                                                          SyntaxKind separatorKind, string separatorText,
                                                          SyntaxKind t2Kind, string t2Text)
        {
            string text = t1Text + separatorText + t2Text;
            var tokens = SyntaxTree.ParseTokens(text).ToArray();

            Assert.Equal(3, tokens.Length);
            Assert.Equal(tokens[0].Kind, t1Kind);
            Assert.Equal(tokens[0].Text, t1Text);
            Assert.Equal(tokens[1].Kind, separatorKind);
            Assert.Equal(tokens[1].Text, separatorText);
            Assert.Equal(tokens[2].Kind, t2Kind);
            Assert.Equal(tokens[2].Text, t2Text);
        }

        public static IEnumerable<object[]> GetTokensData()
        {
            foreach ((var kind, string text) in GetTokens().Concat(GetSeparators()))
                yield return new object[] { kind, text };
        }

        public static IEnumerable<object[]> GetTokenPairsData()
        {
            foreach (var t in GetTokenPairs())
                yield return new object[] { t.t1Kind, t.t1Text, t.t2Kind, t.t2Text };
        }

        public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
        {
            foreach (var t in GetTokenPairsWithSeparator())
                yield return new object[] { t.t1Kind, t.t1Text, t.separatorKind, t.separatorText, t.t2Kind, t.t2Text };
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
        {
            var fixedTokens = Enum.GetValues(typeof(SyntaxKind))
                                  .Cast<SyntaxKind>()
                                  .Select(k => (kind: k, text: SyntaxFacts.GetText(k)))
                                  .Where(t => t.text != null);

            (SyntaxKind, string)[] dynamicTokens =
            [
                (SyntaxKind.NumberToken, "1"),
                (SyntaxKind.NumberToken, "1.0"),
                (SyntaxKind.NumberToken, "1."),
                (SyntaxKind.NumberToken, "123"),
                (SyntaxKind.IdentifierToken, "a"),
                (SyntaxKind.IdentifierToken, "abc"),
                (SyntaxKind.StringToken, "\"Test\""),
                (SyntaxKind.StringToken, "\"Te\\\"st\""),
            ];

            return fixedTokens.Concat(dynamicTokens);
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
        {
            return new[]
            {
                (SyntaxKind.WhitespaceTrivia, " "),
                (SyntaxKind.WhitespaceTrivia, "  "),
                (SyntaxKind.WhitespaceTrivia, "\r"),
                (SyntaxKind.WhitespaceTrivia, "\n"),
                (SyntaxKind.WhitespaceTrivia, "\r\n"),
                (SyntaxKind.MultiLineCommentTrivia, "/**/"),
            };
        }

        private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
        {
            bool t1IsKeyword = t1Kind.IsKeyword();
            bool t2IsKeyword = t2Kind.IsKeyword();

            if (t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.IdentifierToken)
                return true;

            if (t1IsKeyword && t2IsKeyword)
                return true;

            if (t1IsKeyword && t2Kind == SyntaxKind.IdentifierToken)
                return true;

            if (t1Kind == SyntaxKind.IdentifierToken && t2IsKeyword)
                return true;

            if (t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.NumberToken)
                return true;

            if (t1IsKeyword && t2Kind == SyntaxKind.NumberToken)
                return true;

            if (t1Kind == SyntaxKind.NumberToken && t2Kind == SyntaxKind.NumberToken)
                return true;

            if (t1Kind == SyntaxKind.NotToken && t2Kind == SyntaxKind.EqualsToken)
                return true;

            if (t1Kind == SyntaxKind.NotToken && t2Kind == SyntaxKind.DoubleEqualsToken)
                return true;

            if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsToken)
                return true;

            if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.DoubleEqualsToken)
                return true;

            if (t1Kind == SyntaxKind.LessThanToken && t2Kind == SyntaxKind.EqualsToken)
                return true;

            if (t1Kind == SyntaxKind.LessThanToken && t2Kind == SyntaxKind.DoubleEqualsToken)
                return true;

            if (t1Kind == SyntaxKind.GreaterThanToken && t2Kind == SyntaxKind.EqualsToken)
                return true;

            if (t1Kind == SyntaxKind.GreaterThanToken && t2Kind == SyntaxKind.DoubleEqualsToken)
                return true;

            if (t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.SlashToken)
                return true;

            if (t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.StarToken)
                return true;

            if (t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.SingleLineCommentTrivia)
                return true;

            if (t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.MultiLineCommentTrivia)
                return true;

            return false;
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (!RequiresSeparator(t1.kind, t2.kind))
                        yield return (t1.kind, t1.text, t2.kind, t2.text);
                }
            }
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text,
                                    SyntaxKind separatorKind, string separatorText,
                                    SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparator()
        {
            foreach (var t1 in GetTokens())
            {
                foreach (var t2 in GetTokens())
                {
                    if (RequiresSeparator(t1.kind, t2.kind))
                    {
                        foreach (var (kind, text) in GetSeparators())
                        {
                            if (!RequiresSeparator(t1.kind, kind) && !RequiresSeparator(kind, t2.kind))
                                yield return (t1.kind, t1.text, kind, text, t2.kind, t2.text);
                        }
                    }
                }
            }
        }
    }
}