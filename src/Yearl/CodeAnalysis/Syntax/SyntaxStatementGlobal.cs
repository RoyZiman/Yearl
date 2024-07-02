namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementGlobal(SyntaxTree syntaxTree, SyntaxStatement statement)
        : SyntaxMember(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
        public SyntaxStatement Statement { get; } = statement;
    }
}