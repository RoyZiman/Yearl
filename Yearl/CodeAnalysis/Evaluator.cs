using Yearl.CodeAnalysis.Binding;
using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis
{
    internal sealed class Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
    {
        private readonly BoundBlockStatement _root = root;
        private readonly Dictionary<VariableSymbol, object> _variables = variables;
        private object? _lastValue;

        public object? Evaluate()
        {
            Dictionary<BoundLabel, int> labelToIndex = new();

            for (int i = 0; i < _root.Statements.Length; i++)
                if (_root.Statements[i] is BoundLabelStatement l)
                    labelToIndex.Add(l.Label, i + 1);

            int index = 0;

            while (index < _root.Statements.Length)
            {
                BoundStatement s = _root.Statements[index];
                switch (s.Kind)
                {
                    case BoundNodeKind.VariableDeclarationStatement:
                        EvaluateVariableDeclarationStatement((BoundVariableDeclarationStatement)s);
                        index++;
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement)s);
                        index++;
                        break;
                    case BoundNodeKind.GotoStatement:
                        BoundGotoStatement gs = (BoundGotoStatement)s;
                        index = labelToIndex[gs.Label];
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        BoundConditionalGotoStatement cgs = (BoundConditionalGotoStatement)s;
                        bool condition = (bool)EvaluateExpression(cgs.Condition);
                        if (condition == cgs.JumpIfTrue)
                            index = labelToIndex[cgs.Label];
                        else
                            index++;
                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;
                    default:
                        throw new Exception($"Unexpected node {s.Kind}");
                }
            }
            return _lastValue;
        }

        private void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement node)
        {
            object value = EvaluateExpression(node.Initializer);
            _variables[node.Variable] = value;
            _lastValue = value;
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

            return u.Operator.Kind switch
            {
                BoundUnaryOperatorKind.Identity => (double)expression,
                BoundUnaryOperatorKind.Negation => -(double)expression,
                BoundUnaryOperatorKind.LogicalNegation => !(bool)expression,
                _ => throw new Exception($"Unexpected unary operator {u.Operator}"),
            };
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            object left = EvaluateExpression(b.Left);
            object right = EvaluateExpression(b.Right);

            if (b.Operator.Kind == BoundBinaryOperatorKind.Division && (double)right == 0)
                throw new Exception("add errors to evaluator - division by 0");

            return b.Operator.Kind switch
            {
                BoundBinaryOperatorKind.Addition => b.Left.Type == TypeSymbol.Number ? (double)left + (double)right : (string)left + (string)right,
                BoundBinaryOperatorKind.Subtraction => (double)left - (double)right,
                BoundBinaryOperatorKind.Multiplication => (double)left * (double)right,
                BoundBinaryOperatorKind.Division => (double)left / (double)right,
                BoundBinaryOperatorKind.Power => Math.Pow((double)left, (double)right),
                BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
                BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
                BoundBinaryOperatorKind.Equals => Equals(left, right),
                BoundBinaryOperatorKind.NotEquals => !Equals(left, right),
                BoundBinaryOperatorKind.GreaterThan => (double)left > (double)right,
                BoundBinaryOperatorKind.GreaterThanEquals => (double)left >= (double)right,
                BoundBinaryOperatorKind.LessThan => (double)left < (double)right,
                BoundBinaryOperatorKind.LessThanEquals => (double)left <= (double)right,
                _ => throw new Exception($"Unexpected binary operator {b.Operator}"),
            };
            ;
        }
    }
}
