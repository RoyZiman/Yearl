namespace Yearl.CodeAnalysis.Symbols
{
    public sealed class ParameterSymbol(string name, TypeSymbol type, int ordinal) : LocalVariableSymbol(name, isReadOnly: true, type)
    {
        public override SymbolKind Kind => SymbolKind.Parameter;
        public int Ordinal { get; } = ordinal;
    }
}