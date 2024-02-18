namespace Yearl.Language.Syntax
{
    public sealed class SyntaxExpressionName(SyntaxToken identifierToken) : SyntaxExpression
    {
        public override SyntaxKind Kind => SyntaxKind.NameExpression;
        public SyntaxToken IdentifierToken { get; } = identifierToken;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
        }
    }
}
