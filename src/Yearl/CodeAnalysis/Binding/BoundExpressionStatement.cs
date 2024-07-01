namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundExpressionStatement(BoundExpression expression) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
        public BoundExpression Expression { get; } = expression;
    }
}