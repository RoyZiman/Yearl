namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundWhileStatement(BoundExpression condition, BoundStatement body) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
        public BoundExpression Condition { get; } = condition;
        public BoundStatement Body { get; } = body;
    }

}