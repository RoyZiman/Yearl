namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionName(SyntaxTree syntaxTree, SyntaxToken identifierToken)
        : SyntaxExpression(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.NameExpression;
        public SyntaxToken IdentifierToken { get; } = identifierToken;
    }
}