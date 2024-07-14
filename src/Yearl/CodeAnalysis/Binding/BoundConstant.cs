namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundConstant(object value)
    {
        public object Value { get; } = value;
    }
}