namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionParenthesized(SyntaxTree syntaxTree, SyntaxToken leftParenthesisToken, SyntaxExpression expression, SyntaxToken rightParenthesisToken)
        : SyntaxExpression(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
        public SyntaxToken OpenParenthesisToken { get; } = leftParenthesisToken;
        public SyntaxExpression Expression { get; } = expression;
        public SyntaxToken CloseParenthesisToken { get; } = rightParenthesisToken;
    }
}
