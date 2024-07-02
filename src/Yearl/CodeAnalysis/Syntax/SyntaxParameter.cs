namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxParameter(SyntaxTree syntaxTree, SyntaxToken identifier, SyntaxTypeClause type)
        : SyntaxNode(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.Parameter;
        public SyntaxToken Identifier { get; } = identifier;
        public SyntaxTypeClause Type { get; } = type;
    }
}