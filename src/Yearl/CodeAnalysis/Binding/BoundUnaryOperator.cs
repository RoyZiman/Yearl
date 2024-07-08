using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol expressionType, TypeSymbol resultType)
    {
        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol operandType)
            : this(syntaxKind, kind, operandType, operandType) { }

        public SyntaxKind SyntaxKind { get; } = syntaxKind;
        public BoundUnaryOperatorKind Kind { get; } = kind;
        public TypeSymbol ExpressionType { get; } = expressionType;
        public TypeSymbol Type { get; } = resultType;

        private static readonly BoundUnaryOperator[] _operators =
        {
            new(SyntaxKind.NotToken, BoundUnaryOperatorKind.LogicalNegation, TypeSymbol.Bool),

            new(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, TypeSymbol.Number),
            new(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, TypeSymbol.Number),
        };

        public static BoundUnaryOperator Bind(SyntaxKind syntaxKind, TypeSymbol expressionType)
        {
            foreach (var op in _operators)
            {
                if (op.SyntaxKind == syntaxKind && op.ExpressionType == expressionType)
                    return op;
            }

            return null;
        }
    }
}