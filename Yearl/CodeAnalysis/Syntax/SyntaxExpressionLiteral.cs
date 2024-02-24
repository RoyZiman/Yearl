using Yearl.CodeAnalysis.Binding;

namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionLiteral(SyntaxToken literalToken, object? value) : SyntaxExpression
    {
        public SyntaxExpressionLiteral(SyntaxToken literalToken)
           : this(literalToken, literalToken.Value) { }

        public SyntaxToken LiteralToken { get; } = literalToken;
        public object? Value { get; } = value;
        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
    }

    internal sealed class BoundWhileStatement : BoundStatement
    {
        public BoundWhileStatement(BoundExpression condition, BoundStatement body)
        {
            Condition = condition;
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }

}