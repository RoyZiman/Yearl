namespace Yearl.Language.Syntax
{
    public static class Syntaxing
    {
        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.NotToken:
                    return 8;

                default:
                    return 0;
            }
        }

        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.HatToken:
                    return 7;

                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 6;

                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 5;

                case SyntaxKind.DoubleEqualsToken:
                case SyntaxKind.NotEqualsToken:
                    return 4;

                case SyntaxKind.GreaterThanToken:
                case SyntaxKind.LessThanToken:
                case SyntaxKind.GreaterThanEqualsToken:
                case SyntaxKind.LessThanEqualsToken:
                    return 3;

                case SyntaxKind.AndToken:
                    return 2;

                case SyntaxKind.OrToken:
                    return 1;

                default:
                    return 0;
            }
        }
        public static SyntaxKind GetKeywordKind(this string text)
        {
            switch (text)
            {
                case "True":
                    return SyntaxKind.TrueKeyword;
                case "False":
                    return SyntaxKind.FalseKeyword;
                default:
                    return SyntaxKind.IdentifierToken;
            }
        }
        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            SyntaxKind[] kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (SyntaxKind kind in kinds)
            {
                if (GetUnaryOperatorPrecedence(kind) > 0)
                    yield return kind;
            }
        }

        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            SyntaxKind[] kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
            foreach (SyntaxKind kind in kinds)
            {
                if (GetBinaryOperatorPrecedence(kind) > 0)
                    yield return kind;
            }
        }

        public static string GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.StarToken:
                    return "*";
                case SyntaxKind.SlashToken:
                    return "/";
                case SyntaxKind.HatToken:
                    return "^";
                case SyntaxKind.NotToken:
                    return "!";
                case SyntaxKind.EqualsToken:
                    return "=";
                case SyntaxKind.AndToken:
                    return "&&";
                case SyntaxKind.OrToken:
                    return "||";
                case SyntaxKind.DoubleEqualsToken:
                    return "==";
                case SyntaxKind.NotEqualsToken:
                    return "!=";
                case SyntaxKind.GreaterThanToken:
                    return ">";
                case SyntaxKind.GreaterThanEqualsToken:
                    return ">=";
                case SyntaxKind.LessThanToken:
                    return "<";
                case SyntaxKind.LessThanEqualsToken:
                    return "<=";
                case SyntaxKind.LeftParenthesisToken:
                    return "(";
                case SyntaxKind.RightParenthesisToken:
                    return ")";
                case SyntaxKind.FalseKeyword:
                    return "False";
                case SyntaxKind.TrueKeyword:
                    return "True";
                default:
                    return null;
            }
        }
    }
}
