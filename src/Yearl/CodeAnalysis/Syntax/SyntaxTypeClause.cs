namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxTypeClause(SyntaxToken colonToken, SyntaxToken identifier) : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.TypeClause;
        public SyntaxToken ColonToken { get; } = colonToken;
        public SyntaxToken Identifier { get; } = identifier;
    }
}