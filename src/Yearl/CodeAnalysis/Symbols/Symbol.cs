namespace Yearl.CodeAnalysis.Symbols
{
    public abstract class Symbol
    {
        private protected Symbol(string name) => Name = name;

        public abstract SymbolKind Kind { get; }
        public string Name { get; }

        public void WriteTo(TextWriter writer)
        {
            SymbolPrinter.WriteTo(this, writer);
        }

        public override string ToString()
        {
            using (StringWriter writer = new())
            {
                WriteTo(writer);
                return writer.ToString();
            }
        }
    }
}