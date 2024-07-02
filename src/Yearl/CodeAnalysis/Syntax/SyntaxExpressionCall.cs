namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionCall(SyntaxTree syntaxTree, SyntaxToken identifier, SyntaxToken openParenthesisToken, SeparatedSyntaxList<SyntaxExpression> arguments, SyntaxToken closeParenthesisToken)
        : SyntaxExpression(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.CallExpression;
        public SyntaxToken Identifier { get; } = identifier;
        public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
        public SeparatedSyntaxList<SyntaxExpression> Arguments { get; } = arguments;
        public SyntaxToken CloseParenthesisToken { get; } = closeParenthesisToken;
    }
}