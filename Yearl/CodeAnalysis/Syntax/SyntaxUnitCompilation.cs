namespace Yearl.Language.Syntax
{
    public sealed class SyntaxUnitCompilation(SyntaxExpression expression, SyntaxToken endOfFileToken) : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public SyntaxExpression Expression { get; } = expression;
        public SyntaxToken EndOfFileToken { get; } = endOfFileToken;
    }
}
