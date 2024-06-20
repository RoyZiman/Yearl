namespace Yearl.CodeAnalysis.Syntax
{
    internal class SyntaxStatementBreak(SyntaxToken keyword) : SyntaxStatement
    {
        public override SyntaxKind Kind => SyntaxKind.BreakStatement;
        public SyntaxToken Keyword { get; } = keyword;
    }
}