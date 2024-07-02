namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementExpression(SyntaxTree syntaxTree, SyntaxExpression expression)
        : SyntaxStatement(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        public SyntaxExpression Expression { get; } = expression;
    }
}