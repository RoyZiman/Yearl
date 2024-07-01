namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundLabelStatement(BoundLabel label) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
        public BoundLabel Label { get; } = label;
    }
}