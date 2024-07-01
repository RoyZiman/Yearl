namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementGlobal(SyntaxStatement statement) : SyntaxMember
    {
        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
        public SyntaxStatement Statement { get; } = statement;
    }
}