using Yearl.Language.Syntax;

namespace Yearl.Language.Syntax
{
    public sealed class SyntaxExpressionBinary(SyntaxExpression left, SyntaxToken operatorToken, SyntaxExpression right) : SyntaxExpression
    {
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
        public SyntaxExpression Left { get; } = left;
        public SyntaxToken OperatorToken { get; } = operatorToken;
        public SyntaxExpression Right { get; } = right;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Right;
        }
    }
}
