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
                    return 6;

                default:
                    return 0;
            }
        }

        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 5;

                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 4;

                case SyntaxKind.DoubleEqualsToken:
                case SyntaxKind.NotEqualsToken:
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
    }
}
