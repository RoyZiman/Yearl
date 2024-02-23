namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundVariableAssignmentExpression(VariableSymbol variable, BoundExpression expression) : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.VariableAssignmentExpression;
        public override Type Type => Expression.Type;
        public VariableSymbol Variable { get; } = variable;
        public BoundExpression Expression { get; } = expression;
    }
}