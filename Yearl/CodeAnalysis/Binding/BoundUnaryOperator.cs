using Yearl.Language.Syntax;

namespace Yearl.Language.Binding
{
    internal sealed class BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type expressionType, Type resultType)
    {
        private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type operandType)
            : this(syntaxKind, kind, operandType, operandType) { }


        public SyntaxKind SyntaxKind { get; } = syntaxKind;
        public BoundUnaryOperatorKind Kind { get; } = kind;
        public Type ExpressionType { get; } = expressionType;
        public Type Type { get; } = resultType;

        private static readonly BoundUnaryOperator[] _operators =
        {
            new(SyntaxKind.NotToken, BoundUnaryOperatorKind.LogicalNegation, typeof(bool)),

            new(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, typeof(double)),
            new(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, typeof(double)),
        };

        public static BoundUnaryOperator Bind(SyntaxKind syntaxKind, Type expressionType)
        {
            foreach (BoundUnaryOperator op in _operators)
            {
                if (op.SyntaxKind == syntaxKind && op.ExpressionType == expressionType)
                    return op;
            }

            return null;
        }
    }
}