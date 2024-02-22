namespace Yearl.Language.Syntax
{
    public sealed class SyntaxUnitCompilation(SyntaxStatement statement, SyntaxToken endOfFileToken) : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public SyntaxStatement Statement { get; } = statement;
        public SyntaxToken EndOfFileToken { get; } = endOfFileToken;
    }
}
