namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundConditionalGotoStatement(BoundLabel label, BoundExpression condition, bool jumpIfTrue = true) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
        public BoundLabel Label { get; } = label;
        public BoundExpression Condition { get; } = condition;
        public bool JumpIfTrue { get; } = jumpIfTrue;
    }
}