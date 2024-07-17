namespace Yearl.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        ErrorExpression,
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        VariableExpression,
        VariableAssignmentExpression,
        VariableCompoundAssignmentExpression,
        CallExpression,
        ConversionExpression,


        BlockStatement,
        NopStatement,
        VariableDeclarationStatement,
        IfStatement,
        ForStatement,
        WhileStatement,

        LabelStatement,
        GotoStatement,
        ConditionalGotoStatement,

        ReturnStatement,

        ExpressionStatement,
    }

}