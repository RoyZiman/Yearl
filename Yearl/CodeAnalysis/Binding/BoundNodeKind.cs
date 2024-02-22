namespace Yearl.Language.Binding
{
    internal enum BoundNodeKind
    {
        LiteralExpression,
        UnaryExpression,
        BinaryExpression,
        VariableExpression,
        VariableAssignmentExpression,

        BlockStatement,
        ExpressionStatement,
        VariableDeclarationStatement,
    }
}