using Yearl.Language.Syntax;

namespace Yearl.Language.Syntax
{
    public sealed class SyntaxExpressionLiteral(SyntaxToken literalToken, object? value) : SyntaxExpression
    {
        public SyntaxExpressionLiteral(SyntaxToken literalToken)
           : this(literalToken, literalToken.Value) { }



        public SyntaxToken LiteralToken { get; } = literalToken;
        public object? Value { get; } = value;
        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    }
}
