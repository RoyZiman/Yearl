namespace Yearl.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        NumberToken,
        //StringToken,

        IdentifierToken,
        TrueKeyword,
        FalseKeyword,
        VarKeyword,
        ConstKeyword,
        IfKeyword,
        FromKeyword,
        ToKeyword,
        StepKeyword,
        ElseKeyword,
        ForKeyword,
        //ForeachKeyword
        WhileKeyword,

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
        LeftCurlyBraceToken,

        RightCurlyBraceToken,

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


        BlockStatement,
        VariableDeclarationStatement,
        IfStatement,
        ElseClause,
        ForStatement,
        WhileStatement,


        ExpressionStatement,

        CompilationUnit,
    }
}