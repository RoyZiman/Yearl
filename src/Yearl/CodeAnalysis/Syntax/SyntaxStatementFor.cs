namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementFor(SyntaxTree syntaxTree, SyntaxToken forKeyword, SyntaxToken identifier, SyntaxToken fromKeyword,
        SyntaxExpression bound1, SyntaxToken toKeyword, SyntaxExpression bound2, SyntaxToken stepKeyword,
        SyntaxExpression stepExpression, SyntaxStatement bodyStatement)
        : SyntaxStatement(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.ForStatement;
        public SyntaxToken ForKeyword { get; } = forKeyword;
        public SyntaxToken Identifier { get; } = identifier;
        public SyntaxToken FromKeyword { get; } = fromKeyword;
        public SyntaxExpression Bound1 { get; } = bound1;
        public SyntaxToken ToKeyword { get; } = toKeyword;
        public SyntaxExpression Bound2 { get; } = bound2;
        public SyntaxToken StepKeyword { get; } = stepKeyword;
        public SyntaxExpression StepExpression { get; } = stepExpression;
        public SyntaxStatement BodyStatement { get; } = bodyStatement;
    }
}