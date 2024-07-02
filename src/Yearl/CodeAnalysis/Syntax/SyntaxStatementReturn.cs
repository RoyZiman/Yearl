namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementReturn(SyntaxTree syntaxTree, SyntaxToken returnKeyword, SyntaxToken openParenthesisToken, SyntaxExpression expression, SyntaxToken closeParenthesisToken)
        : SyntaxStatement(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
        public SyntaxToken ReturnKeyword { get; } = returnKeyword;
        public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
        public SyntaxExpression Expression { get; } = expression;
        public SyntaxToken CloseParenthesisToken { get; } = closeParenthesisToken;
    }
}