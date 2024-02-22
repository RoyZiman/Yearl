namespace Yearl.Language.Syntax
{
    public enum SyntaxKind
    {
        NumberToken,
        //StringToken,

        IdentifierToken,
        TrueKeyword,
        FalseKeyword,

        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        HatToken,

        EqualsToken,
        //CommaToken,

        LeftParenthesisToken,
        RightParenthesisToken,
        //LeftSquareBracketToken,
        //RightSquareBracketToken,
        //LeftCurlyBracketsToken,
        //RightCurlyBracketsToken,

        NotToken,
        DoubleEqualsToken,
        NotEqualsToken,
        LessThanToken,
        GreaterThanToken,
        LessThanEqualsToken,
        GreaterThanEqualsToken,

        AndToken,
        OrToken,

        WhitespaceToken,
        //NewLineToken,
        EndOfFileToken,
        InvalidToken,



        LiteralExpression,
        NameExpression,
        BinaryExpression,
        UnaryExpression,
        ParenthesizedExpression,
        VariableAssignmentExpression,



        TreeStatement,


        CompilationUnit,
    }
}
