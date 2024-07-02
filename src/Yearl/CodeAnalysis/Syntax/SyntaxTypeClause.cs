namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxTypeClause(SyntaxTree syntaxTree, SyntaxToken colonToken, SyntaxToken identifier)
        : SyntaxNode(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.TypeClause;
        public SyntaxToken ColonToken { get; } = colonToken;
        public SyntaxToken Identifier { get; } = identifier;
    }
}