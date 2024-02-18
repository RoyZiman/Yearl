namespace Yearl.Language.Binding
{
    internal sealed class BoundUnaryExpression(BoundUnaryOperator op, BoundExpression expression) : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override Type Type => Operator.Type;
        public BoundUnaryOperator Operator { get; } = op;
        public BoundExpression Expression { get; } = expression;
    }
}