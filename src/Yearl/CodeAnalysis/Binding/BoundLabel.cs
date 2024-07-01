namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundLabel(string name)
    {
        public string Name { get; } = name;

        public override string ToString() => Name;
    }
}