namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionUnary(SyntaxTree syntaxTree, SyntaxToken operatorToken, SyntaxExpression expression)
        : SyntaxExpression(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
        public SyntaxToken OperatorToken { get; } = operatorToken;
        public SyntaxExpression Expression { get; } = expression;
    }
}