using Yearl.Language.Binding;

namespace Yearl.Language
{
    internal sealed class Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
    {
        private readonly BoundStatement _root = root;
        private readonly Dictionary<VariableSymbol, object> _variables = variables;
        private object? _lastValue;


        public object? Evaluate()
        {
            EvaluateStatement(_root);
            return _lastValue;
        }


        private void EvaluateStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    EvaluateBlockStatement((BoundBlockStatement)node);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)node);
                    break;
                case BoundNodeKind.VariableDeclarationStatement:
                    EvaluateVariableDeclaration((BoundVariableDeclarationStatement)node);
                    break;
                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclarationStatement node)
        {
            object value = EvaluateExpression(node.Initializer);
            _variables[node.Variable] = value;
            _lastValue = value;
        }

        private void EvaluateBlockStatement(BoundBlockStatement node)
        {
            foreach (BoundStatement statement in node.Statements)
                EvaluateStatement(statement);
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }


        private object EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
                return n.Value;

            if (node is BoundUnaryExpression u)
                return EvaluateUnaryExpression(u);

            if (node is BoundBinaryExpression b)
                return EvaluateBinaryExpression(b);

            if (node is BoundVariableExpression v)
                return _variables[v.Variable];

            if (node is BoundVariableAssignmentExpression a)
            {
                object value = EvaluateExpression(a.Expression);
                _variables[a.Variable] = value;
                return value;
            }

            throw new Exception($"Unexpected node {node.Kind}");
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            object expression = EvaluateExpression(u.Expression);

            switch (u.Operator.Kind)
            {
                case BoundUnaryOperatorKind.Identity:
                    return (double)expression;
                case BoundUnaryOperatorKind.Negation:
                    return -(double)expression;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool)expression;
                default:
                    throw new Exception($"Unexpected unary operator {u.Operator}");
            }
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            object left = EvaluateExpression(b.Left);
            object right = EvaluateExpression(b.Right);

            switch (b.Operator.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    return (double)left + (double)right;
                case BoundBinaryOperatorKind.Subtraction:
                    return (double)left - (double)right;
                case BoundBinaryOperatorKind.Multiplication:
                    return (double)left * (double)right;
                case BoundBinaryOperatorKind.Division:
                    return (double)left / (double)right;
                case BoundBinaryOperatorKind.Power:
                    return Math.Pow((double)left, (double)right);
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)left && (bool)right;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)left || (bool)right;
                case BoundBinaryOperatorKind.Equals:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.NotEquals:
                    return !Equals(left, right);
                case BoundBinaryOperatorKind.GreaterThan:
                    return (double)left > (double)right;
                case BoundBinaryOperatorKind.GreaterThanEquals:
                    return (double)left >= (double)right;
                case BoundBinaryOperatorKind.LessThan:
                    return (double)left < (double)right;
                case BoundBinaryOperatorKind.LessThanEquals:
                    return (double)left <= (double)right;
                default:
                    throw new Exception($"Unexpected binary operator {b.Operator}");
            }
        }
    }
}