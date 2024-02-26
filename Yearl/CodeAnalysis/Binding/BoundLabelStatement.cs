namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundLabelStatement(LabelSymbol label) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
        public LabelSymbol Label { get; } = label;
    }
}