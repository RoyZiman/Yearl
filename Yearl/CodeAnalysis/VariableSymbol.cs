namespace Yearl.CodeAnalysis
{
    public sealed class VariableSymbol(string name, bool isReadOnly, Type type)
    {
        public string Name { get; } = name;
        public bool IsReadOnly { get; } = isReadOnly;
        public Type Type { get; } = type;
    }
}