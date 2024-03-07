namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionCall(SyntaxToken identifier, SyntaxToken openParenthesisToken, SeparatedSyntaxList<SyntaxExpression> arguments, SyntaxToken closeParenthesisToken) : SyntaxExpression
    {
        public override SyntaxKind Kind => SyntaxKind.CallExpression;
        public SyntaxToken Identifier { get; } = identifier;
        public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
        public SeparatedSyntaxList<SyntaxExpression> Arguments { get; } = arguments;
        public SyntaxToken CloseParenthesisToken { get; } = closeParenthesisToken;
    }
}