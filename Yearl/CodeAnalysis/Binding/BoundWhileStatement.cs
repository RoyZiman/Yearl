namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundWhileStatement(BoundExpression condition, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel) : BoundLoopStatement(breakLabel, continueLabel)
    {
        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
        public BoundExpression Condition { get; } = condition;
        public BoundStatement Body { get; } = body;
    }
}