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



        BlockStatement,
        VariableDeclarationStatement,
        IfStatement,
        ForStatement,
        WhileStatement,

        LabelStatement,
        GotoStatement,
        ConditionalGotoStatement,


        ExpressionStatement,
    }
}