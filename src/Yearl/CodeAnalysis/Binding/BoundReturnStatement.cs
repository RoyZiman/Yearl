namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundReturnStatement(BoundExpression expression) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;
        public BoundExpression Expression { get; } = expression;
    }
}