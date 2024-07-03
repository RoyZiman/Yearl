namespace Yearl.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
        public override string ToString()
        {
            using StringWriter writer = new();
            this.WriteTo(writer);
            return writer.ToString();
        }
    }
}