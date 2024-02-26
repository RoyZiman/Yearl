namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundGotoStatement(LabelSymbol label) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
        public LabelSymbol Label { get; } = label;
    }
}