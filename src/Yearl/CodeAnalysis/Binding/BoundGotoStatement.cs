namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundGotoStatement(BoundLabel label) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
        public BoundLabel Label { get; } = label;
    }
}