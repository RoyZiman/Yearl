namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementIf(SyntaxToken ifKeyword, SyntaxExpression condition, SyntaxStatement thenStatement, SyntaxStatementElseClause? elseClause) : SyntaxStatement
    {
        public override SyntaxKind Kind => SyntaxKind.IfStatement;
        public SyntaxToken IfKeyword { get; } = ifKeyword;
        public SyntaxExpression Condition { get; } = condition;
        public SyntaxStatement ThenStatement { get; } = thenStatement;
        public SyntaxStatementElseClause? ElseClause { get; } = elseClause;
    }
    public sealed class SyntaxStatementElseClause(SyntaxToken elseKeyword, SyntaxStatement elseStatement) : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.ElseClause;
        public SyntaxToken ElseKeyword { get; } = elseKeyword;
        public SyntaxStatement ElseStatement { get; } = elseStatement;
    }
}