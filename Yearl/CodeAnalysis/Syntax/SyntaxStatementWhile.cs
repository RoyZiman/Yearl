namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementWhile(SyntaxToken whileKeyword, SyntaxExpression condition, SyntaxStatement bodyStatement) : SyntaxStatement
    {
        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        public SyntaxToken WhileKeyword { get; } = whileKeyword;
        public SyntaxExpression Condition { get; } = condition;
        public SyntaxStatement BodyStatement { get; } = bodyStatement;
    }
}