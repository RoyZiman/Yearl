using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundVariableAssignmentExpression(VariableSymbol variable, BoundExpression expression) : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.VariableAssignmentExpression;
        public override TypeSymbol Type => Expression.Type;
        public VariableSymbol Variable { get; } = variable;
        public BoundExpression Expression { get; } = expression;
    }
}