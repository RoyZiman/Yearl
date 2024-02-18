namespace Yearl.Language
{
    public sealed class VariableSymbol(string name, Type type)
    {
        public string Name { get; } = name;
        public Type Type { get; } = type;
    }
}