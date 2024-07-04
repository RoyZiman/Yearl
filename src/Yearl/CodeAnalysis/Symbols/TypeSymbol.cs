namespace Yearl.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new("?");

        public static readonly TypeSymbol Bool = new("Bool");
        public static readonly TypeSymbol Dynamic = new("Dynamic");
        public static readonly TypeSymbol Number = new("Number");
        public static readonly TypeSymbol String = new("String");
        public static readonly TypeSymbol Void = new("Void");

        private TypeSymbol(string name) : base(name) { }

        public override SymbolKind Kind => SymbolKind.Type;
    }
}