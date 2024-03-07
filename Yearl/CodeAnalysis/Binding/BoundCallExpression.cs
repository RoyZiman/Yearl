using System.Collections.Immutable;
using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundCallExpression(FunctionSymbol function, ImmutableArray<BoundExpression> arguments) : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.CallExpression;
        public override TypeSymbol Type => Function.Type;
        public FunctionSymbol Function { get; } = function;
        public ImmutableArray<BoundExpression> Arguments { get; } = arguments;
    }
}