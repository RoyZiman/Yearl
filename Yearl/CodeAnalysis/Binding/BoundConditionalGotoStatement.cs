namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundConditionalGotoStatement(LabelSymbol label, BoundExpression condition, bool jumpIfTrue = true) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
        public LabelSymbol Label { get; } = label;
        public BoundExpression Condition { get; } = condition;
        public bool JumpIfTrue { get; } = jumpIfTrue;
    }
}