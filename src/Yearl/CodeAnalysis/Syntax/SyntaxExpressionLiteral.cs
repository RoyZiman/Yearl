namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionLiteral(SyntaxTree syntaxTree, SyntaxToken literalToken, object value)
        : SyntaxExpression(syntaxTree)
    {
        public SyntaxExpressionLiteral(SyntaxTree syntaxTree, SyntaxToken literalToken)
            : this(syntaxTree, literalToken, literalToken.Value) { }

        public SyntaxToken LiteralToken { get; } = literalToken;
        public object Value { get; } = value;
        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    }

}