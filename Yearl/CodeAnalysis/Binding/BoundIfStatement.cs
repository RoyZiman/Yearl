namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundIfStatement(BoundExpression condition, BoundStatement thenStatement, BoundStatement? elseStatement) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.IfStatement;
        public BoundExpression Condition { get; } = condition;
        public BoundStatement ThenStatement { get; } = thenStatement;
        public BoundStatement? ElseStatement { get; } = elseStatement;
    }
}