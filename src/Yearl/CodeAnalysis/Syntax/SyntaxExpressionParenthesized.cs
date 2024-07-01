namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionParenthesized(SyntaxToken leftParenthesisToken, SyntaxExpression expression, SyntaxToken rightParenthesisToken) : SyntaxExpression
    {
        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
        public SyntaxToken OpenParenthesisToken { get; } = leftParenthesisToken;
        public SyntaxExpression Expression { get; } = expression;
        public SyntaxToken CloseParenthesisToken { get; } = rightParenthesisToken;
    }
}