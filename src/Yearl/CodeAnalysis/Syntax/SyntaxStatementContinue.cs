namespace Yearl.CodeAnalysis.Syntax
{
    internal class SyntaxStatementContinue(SyntaxToken keyword) : SyntaxStatement
    {
        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
        public SyntaxToken Keyword { get; } = keyword;
    }

}