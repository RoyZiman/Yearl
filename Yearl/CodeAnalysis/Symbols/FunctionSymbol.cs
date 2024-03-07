using System.Collections.Immutable;

namespace Yearl.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameter, TypeSymbol type) : Symbol(name)
    {
        public override SymbolKind Kind => SymbolKind.Function;
        public ImmutableArray<ParameterSymbol> Parameter { get; } = parameter;
        public TypeSymbol Type { get; } = type;
    }
}