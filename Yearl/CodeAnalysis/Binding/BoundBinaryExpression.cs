namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right) : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
        public override Type Type => Operator.Type;
        public BoundExpression Left { get; } = left;
        public BoundBinaryOperator Operator { get; } = op;
        public BoundExpression Right { get; } = right;
    }
}