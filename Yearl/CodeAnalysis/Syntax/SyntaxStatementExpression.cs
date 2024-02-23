namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementExpression : SyntaxStatement
    {
        public SyntaxStatementExpression(SyntaxExpression expression)
        {
            Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        public SyntaxExpression Expression { get; }
    }
}
