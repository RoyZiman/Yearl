using Yearl.Language.Syntax;

namespace Yearl.Language.Syntax
{
    public sealed class SyntaxExpressionParenthesized(SyntaxToken leftParenthesisToken, SyntaxExpression expression, SyntaxToken rightParenthesisToken) : SyntaxExpression
    {
        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
        public SyntaxToken OpenParenthesisToken { get; } = leftParenthesisToken;
        public SyntaxExpression Expression { get; } = expression;
        public SyntaxToken CloseParenthesisToken { get; } = rightParenthesisToken;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return CloseParenthesisToken;
        }
    }
}
