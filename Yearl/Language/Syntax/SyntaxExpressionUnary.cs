using Yearl.Language.Syntax;

namespace Yearl.Language.Syntax
{
    public sealed class SyntaxExpressionUnary(SyntaxToken operatorToken, SyntaxExpression expression) : SyntaxExpression
    {
        public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
        public SyntaxToken OperatorToken { get; } = operatorToken;
        public SyntaxExpression Expression { get; } = expression;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OperatorToken;
            yield return Expression;
        }
    }
}
