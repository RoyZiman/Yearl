namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementExpression(SyntaxExpression expression) : SyntaxStatement
    {
        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        public SyntaxExpression Expression { get; } = expression;
    }
}