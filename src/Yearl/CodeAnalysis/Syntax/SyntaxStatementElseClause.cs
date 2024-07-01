namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementElseClause(SyntaxToken elseKeyword, SyntaxStatement elseStatement) : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.ElseClause;
        public SyntaxToken ElseKeyword { get; } = elseKeyword;
        public SyntaxStatement ElseStatement { get; } = elseStatement;
    }
}