namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementWhile(SyntaxTree syntaxTree, SyntaxToken whileKeyword, SyntaxExpression condition, SyntaxStatement bodyStatement)
        : SyntaxStatement(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        public SyntaxToken WhileKeyword { get; } = whileKeyword;
        public SyntaxExpression Condition { get; } = condition;
        public SyntaxStatement BodyStatement { get; } = bodyStatement;
    }
}