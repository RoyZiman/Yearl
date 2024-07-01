using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpression(object value) : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;

        public override TypeSymbol Type { get; } = value switch
        {
            bool => TypeSymbol.Bool,
            double => TypeSymbol.Number,
            string => TypeSymbol.String,
            _ => throw new Exception($"Unexpected literal '{value}' of type {value.GetType()}"),
        };

        public object Value { get; } = value;
    }
}