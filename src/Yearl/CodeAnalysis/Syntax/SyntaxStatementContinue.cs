namespace Yearl.CodeAnalysis.Syntax
{
    internal class SyntaxStatementContinue(SyntaxTree syntaxTree, SyntaxToken keyword)
        : SyntaxStatement(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
        public SyntaxToken Keyword { get; } = keyword;
    }

}