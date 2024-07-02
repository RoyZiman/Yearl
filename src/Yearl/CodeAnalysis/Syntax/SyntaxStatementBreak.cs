namespace Yearl.CodeAnalysis.Syntax
{
    internal class SyntaxStatementBreak(SyntaxTree syntaxTree, SyntaxToken keyword)
        : SyntaxStatement(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.BreakStatement;
        public SyntaxToken Keyword { get; } = keyword;
    }
}