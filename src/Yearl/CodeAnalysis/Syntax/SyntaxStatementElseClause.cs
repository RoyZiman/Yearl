namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementElseClause(SyntaxTree syntaxTree, SyntaxToken elseKeyword, SyntaxStatement elseStatement)
        : SyntaxNode(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.ElseClause;
        public SyntaxToken ElseKeyword { get; } = elseKeyword;
        public SyntaxStatement ElseStatement { get; } = elseStatement;
    }
}