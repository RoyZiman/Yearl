namespace Yearl.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusToken or SyntaxKind.MinusToken or SyntaxKind.NotToken => 8,
                _ => 0,
            };
        }

        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.HatToken => 7,
                SyntaxKind.StarToken or SyntaxKind.SlashToken => 6,
                SyntaxKind.PlusToken or SyntaxKind.MinusToken => 5,
                SyntaxKind.DoubleEqualsToken or SyntaxKind.NotEqualsToken => 4,
                SyntaxKind.GreaterThanToken or SyntaxKind.LessThanToken or SyntaxKind.GreaterThanEqualsToken or SyntaxKind.LessThanEqualsToken => 3,
                SyntaxKind.AndToken => 2,
                SyntaxKind.OrToken => 1,
                _ => 0,
            };
        }

        public static SyntaxKind GetKeywordKind(this string text)
        {
            return text switch
            {
                "True" => SyntaxKind.TrueKeyword,
                "False" => SyntaxKind.FalseKeyword,
                "var" => SyntaxKind.VarKeyword,
                "const" => SyntaxKind.ConstKeyword,
                "if" => SyntaxKind.IfKeyword,
                "else" => SyntaxKind.ElseKeyword,
                _ => SyntaxKind.IdentifierToken,
            };
        }

        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            SyntaxKind[] kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (SyntaxKind kind in kinds)
            {
                if (kind.GetUnaryOperatorPrecedence() > 0)
                    yield return kind;
            }
        }

        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            SyntaxKind[] kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (SyntaxKind kind in kinds)
            {
                if (kind.GetBinaryOperatorPrecedence() > 0)
                    yield return kind;
            }
        }

        public static string? GetText(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusToken => "+",
                SyntaxKind.MinusToken => "-",
                SyntaxKind.StarToken => "*",
                SyntaxKind.SlashToken => "/",
                SyntaxKind.HatToken => "^",
                SyntaxKind.NotToken => "!",
                SyntaxKind.EqualsToken => "=",
                SyntaxKind.AndToken => "&&",
                SyntaxKind.OrToken => "||",
                SyntaxKind.DoubleEqualsToken => "==",
                SyntaxKind.NotEqualsToken => "!=",
                SyntaxKind.GreaterThanToken => ">",
                SyntaxKind.GreaterThanEqualsToken => ">=",
                SyntaxKind.LessThanToken => "<",
                SyntaxKind.LessThanEqualsToken => "<=",
                SyntaxKind.LeftParenthesisToken => "(",
                SyntaxKind.RightParenthesisToken => ")",
                SyntaxKind.LeftCurlyBraceToken => "{",
                SyntaxKind.RightCurlyBraceToken => "}",
                SyntaxKind.TrueKeyword => "True",
                SyntaxKind.FalseKeyword => "False",
                SyntaxKind.VarKeyword => "var",
                SyntaxKind.ConstKeyword => "const",
                SyntaxKind.IfKeyword => "if",
                SyntaxKind.ElseKeyword => "else",
                _ => null,
            };
        }
    }
}