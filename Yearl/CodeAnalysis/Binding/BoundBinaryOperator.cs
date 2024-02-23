using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type leftType, Type rightType, Type resultType)
    {
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type type)
         : this(syntaxKind, kind, type, type, type) { }

        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type operandType, Type resultType)
         : this(syntaxKind, kind, operandType, operandType, resultType) { }

        public SyntaxKind SyntaxKind { get; } = syntaxKind;
        public BoundBinaryOperatorKind Kind { get; } = kind;
        public Type LeftType { get; } = leftType;
        public Type RightType { get; } = rightType;
        public Type Type { get; } = resultType;

        private static BoundBinaryOperator[] _operators =
        [
            new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, typeof(double)),
            new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, typeof(double)),
            new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, typeof(double)),
            new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, typeof(double)),
            new(SyntaxKind.HatToken, BoundBinaryOperatorKind.Power, typeof(double)),

            new(SyntaxKind.DoubleEqualsToken, BoundBinaryOperatorKind.Equals, typeof(double), typeof(bool)),
            new(SyntaxKind.NotEqualsToken, BoundBinaryOperatorKind.NotEquals, typeof(double), typeof(bool)),
            new(SyntaxKind.GreaterThanToken, BoundBinaryOperatorKind.GreaterThan, typeof(double), typeof(bool)),
            new(SyntaxKind.GreaterThanEqualsToken, BoundBinaryOperatorKind.GreaterThanEquals, typeof(double), typeof(bool)),
            new(SyntaxKind.LessThanToken, BoundBinaryOperatorKind.LessThan, typeof(double), typeof(bool)),
            new(SyntaxKind.LessThanEqualsToken, BoundBinaryOperatorKind.LessThanEquals, typeof(double), typeof(bool)),

            new(SyntaxKind.AndToken, BoundBinaryOperatorKind.LogicalAnd, typeof(bool)),
            new(SyntaxKind.OrToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool)),
            new(SyntaxKind.DoubleEqualsToken, BoundBinaryOperatorKind.Equals, typeof(bool)),
            new(SyntaxKind.NotEqualsToken, BoundBinaryOperatorKind.NotEquals, typeof(bool)),
        ];

        public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, Type leftType, Type rightType)
        {
            foreach (BoundBinaryOperator op in _operators)
            {
                if (op.SyntaxKind == syntaxKind && op.LeftType == leftType && op.RightType == rightType)
                    return op;
            }

            return null;
        }
    }
}