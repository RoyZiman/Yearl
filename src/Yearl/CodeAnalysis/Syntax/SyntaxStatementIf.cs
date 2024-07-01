namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementIf(SyntaxToken ifKeyword, SyntaxExpression condition, SyntaxStatement bodyStatement, SyntaxStatementElseClause? elseClause) : SyntaxStatement
    {
        public override SyntaxKind Kind => SyntaxKind.IfStatement;
        public SyntaxToken IfKeyword { get; } = ifKeyword;
        public SyntaxExpression Condition { get; } = condition;
        public SyntaxStatement BodyStatement { get; } = bodyStatement;
        public SyntaxStatementElseClause? ElseClause { get; } = elseClause;
    }
}