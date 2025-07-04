﻿namespace Yearl.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {

        NumberToken,
        StringToken,

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
        BreakKeyword,
        ContinueKeyword,
        ReturnKeyword,
        FuncKeyword,

        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        HatToken,

        EqualsToken,
        ColonToken,
        CommaToken,

        PlusEqualsToken,
        MinusEqualsToken,
        StarEqualsToken,
        SlashEqualsToken,
        AmpersandEqualsToken,
        PipeEqualsToken,

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


        InvalidTokenTrivia,
        WhitespaceTrivia,
        SingleLineCommentTrivia,
        MultiLineCommentTrivia,

        EndOfFileToken,




        LiteralExpression,
        NameExpression,
        BinaryExpression,
        UnaryExpression,
        ParenthesizedExpression,
        VariableAssignmentExpression,
        CallExpression,


        GlobalStatement,
        TypeClause,
        Parameter,
        FunctionDeclaration,
        BlockStatement,
        VariableDeclarationStatement,
        IfStatement,
        ElseClause,
        ForStatement,
        WhileStatement,
        BreakStatement,
        ContinueStatement,
        ReturnStatement,


        ExpressionStatement,

        CompilationUnit,
    }
}