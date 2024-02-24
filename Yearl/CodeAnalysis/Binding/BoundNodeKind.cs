namespace Yearl.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        VariableExpression,
        VariableAssignmentExpression,



        BlockStatement,
        VariableDeclarationStatement,
        IfStatement,
        WhileStatement,

        ExpressionStatement,
    }
}