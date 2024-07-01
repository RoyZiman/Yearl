using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryExpression(BoundUnaryOperator op, BoundExpression expression) : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override TypeSymbol Type => Operator.Type;
        public BoundUnaryOperator Operator { get; } = op;
        public BoundExpression Expression { get; } = expression;
    }
}