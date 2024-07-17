using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal static class ConstantFolding
    {
        public static BoundConstant? ComputeConstant(BoundUnaryOperator op, BoundExpression operand)
        {
            if (operand.ConstantValue != null)
            {
                switch (op.Kind)
                {
                    case BoundUnaryOperatorKind.Identity:
                        return new BoundConstant((double)operand.ConstantValue.Value);
                    case BoundUnaryOperatorKind.Negation:
                        return new BoundConstant(-(double)operand.ConstantValue.Value);
                    case BoundUnaryOperatorKind.LogicalNegation:
                        return new BoundConstant(!(bool)operand.ConstantValue.Value);
                    default:
                        throw new Exception($"Unexpected unary operator {op.Kind}");
                }
            }

            return null;
        }

        public static BoundConstant? ComputeConstant(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
        {
            var leftConstant = left.ConstantValue;
            var rightConstant = right.ConstantValue;

            // Special case && and || because there are cases where only one
            // side needs to be known.

            if (op.Kind == BoundBinaryOperatorKind.LogicalAnd)
            {
                if (leftConstant != null && !(bool)leftConstant.Value ||
                    rightConstant != null && !(bool)rightConstant.Value)
                {
                    return new BoundConstant(false);
                }
            }

            if (op.Kind == BoundBinaryOperatorKind.LogicalOr)
            {
                if (leftConstant != null && (bool)leftConstant.Value ||
                    rightConstant != null && (bool)rightConstant.Value)
                {
                    return new BoundConstant(true);
                }
            }

            if (leftConstant == null || rightConstant == null)
                return null;

            object l = leftConstant.Value;
            object r = rightConstant.Value;

            switch (op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (left.Type == TypeSymbol.Number)
                        return new BoundConstant((double)l + (double)r);
                    else
                        return new BoundConstant((string)l + (string)r);
                case BoundBinaryOperatorKind.Subtraction:
                    return new BoundConstant((double)l - (double)r);
                case BoundBinaryOperatorKind.Multiplication:
                    return new BoundConstant((double)l * (double)r);
                case BoundBinaryOperatorKind.Division:
                    return new BoundConstant((double)l / (double)r);
                case BoundBinaryOperatorKind.Power:
                    return new BoundConstant(Math.Pow((double)l, (double)r));
                case BoundBinaryOperatorKind.LogicalAnd:
                    return new BoundConstant((bool)l && (bool)r);
                case BoundBinaryOperatorKind.LogicalOr:
                    return new BoundConstant((bool)l || (bool)r);
                case BoundBinaryOperatorKind.Equals:
                    return new BoundConstant(Equals(l, r));
                case BoundBinaryOperatorKind.NotEquals:
                    return new BoundConstant(!Equals(l, r));
                case BoundBinaryOperatorKind.LessThan:
                    return new BoundConstant((double)l < (double)r);
                case BoundBinaryOperatorKind.LessThanEquals:
                    return new BoundConstant((double)l <= (double)r);
                case BoundBinaryOperatorKind.GreaterThan:
                    return new BoundConstant((double)l > (double)r);
                case BoundBinaryOperatorKind.GreaterThanEquals:
                    return new BoundConstant((double)l >= (double)r);
                default:
                    throw new Exception($"Unexpected binary operator {op.Kind}");
            }
        }
    }
}