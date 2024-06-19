namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxParameter(SyntaxToken identifier, SyntaxTypeClause type) : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.Parameter;
        public SyntaxToken Identifier { get; } = identifier;
        public SyntaxTypeClause Type { get; } = type;
    }
}