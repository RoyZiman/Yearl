namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementIf(SyntaxTree syntaxTree, SyntaxToken ifKeyword, SyntaxExpression condition, SyntaxStatement bodyStatement, SyntaxStatementElseClause? elseClause)
        : SyntaxStatement(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.IfStatement;
        public SyntaxToken IfKeyword { get; } = ifKeyword;
        public SyntaxExpression Condition { get; } = condition;
        public SyntaxStatement BodyStatement { get; } = bodyStatement;
        public SyntaxStatementElseClause? ElseClause { get; } = elseClause;
    }
}