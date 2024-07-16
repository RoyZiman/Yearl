namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundIfStatement(BoundExpression condition, BoundStatement bodyStatement, BoundStatement? elseStatement) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.IfStatement;
        public BoundExpression Condition { get; } = condition;
        public BoundStatement BodyStatement { get; } = bodyStatement;
        public BoundStatement? ElseStatement { get; } = elseStatement;
    }
}