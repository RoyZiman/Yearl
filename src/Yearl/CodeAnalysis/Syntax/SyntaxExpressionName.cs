namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionName(SyntaxToken identifierToken) : SyntaxExpression
    {
        public override SyntaxKind Kind => SyntaxKind.NameExpression;
        public SyntaxToken IdentifierToken { get; } = identifierToken;
    }
}