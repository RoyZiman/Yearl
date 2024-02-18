namespace Yearl.Language.Binding
{
    internal sealed class BoundVariableExpression(VariableSymbol variable) : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public override Type Type => Variable.Type;
        public VariableSymbol Variable { get; } = variable;
    }
}