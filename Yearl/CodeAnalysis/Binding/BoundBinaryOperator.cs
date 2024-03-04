using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
    {
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol type)
         : this(syntaxKind, kind, type, type, type) { }

        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType)
         : this(syntaxKind, kind, operandType, operandType, resultType) { }

        public SyntaxKind SyntaxKind { get; } = syntaxKind;
        public BoundBinaryOperatorKind Kind { get; } = kind;
        public TypeSymbol LeftType { get; } = leftType;
        public TypeSymbol RightType { get; } = rightType;
        public TypeSymbol Type { get; } = resultType;

        private static BoundBinaryOperator[] _operators =
        [

            new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Number),
            new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Number),
            new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Number),
            new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Number),
            new(SyntaxKind.HatToken, BoundBinaryOperatorKind.Power, TypeSymbol.Number),

            new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.String),

            new(SyntaxKind.DoubleEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Number, TypeSymbol.Bool),
            new(SyntaxKind.NotEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Number, TypeSymbol.Bool),
            new(SyntaxKind.GreaterThanToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Number, TypeSymbol.Bool),
            new(SyntaxKind.GreaterThanEqualsToken, BoundBinaryOperatorKind.GreaterThanEquals, TypeSymbol.Number, TypeSymbol.Bool),
            new(SyntaxKind.LessThanToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Number, TypeSymbol.Bool),
            new(SyntaxKind.LessThanEqualsToken, BoundBinaryOperatorKind.LessThanEquals, TypeSymbol.Number, TypeSymbol.Bool),


            new(SyntaxKind.AndToken, BoundBinaryOperatorKind.LogicalAnd, TypeSymbol.Bool),
            new(SyntaxKind.OrToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.Bool),
            new(SyntaxKind.DoubleEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Bool),
            new(SyntaxKind.NotEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Bool)
        ];

        public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
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