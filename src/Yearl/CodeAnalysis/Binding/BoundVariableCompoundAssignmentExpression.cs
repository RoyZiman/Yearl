using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundVariableCompoundAssignmentExpression(VariableSymbol variable, BoundBinaryOperator op, BoundExpression expression) : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.VariableCompoundAssignmentExpression;
        public override TypeSymbol Type => Expression.Type;
        public VariableSymbol Variable { get; } = variable;
        public BoundBinaryOperator Operator { get; } = op;
        public BoundExpression Expression { get; } = expression;
    }

}