namespace Yearl.CodeAnalysis.Symbols
{
    public sealed class ParameterSymbol(string name, TypeSymbol type) : LocalVariableSymbol(name, isReadOnly: true, type)
    {
        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}