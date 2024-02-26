using Yearl.CodeAnalysis.Binding;

namespace Yearl.CodeAnalysis
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
                    EvaluateVariableDeclarationStatement((BoundVariableDeclarationStatement)node);
                    break;

                case BoundNodeKind.IfStatement:
                    EvaluateIfStatement((BoundIfStatement)node);
                    break;

                case BoundNodeKind.WhileStatement:
                    EvaluateWhileStatement((BoundWhileStatement)node);
                    break;

                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }
        }

        private void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement node)
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

        private void EvaluateIfStatement(BoundIfStatement node)
        {
            bool condition = (bool)EvaluateExpression(node.Condition);
            if (condition)
                EvaluateStatement(node.BodyStatement);
            else if (node.ElseStatement != null)
                EvaluateStatement(node.ElseStatement);
        }

        private void EvaluateForStatement(BoundForStatement node)
        {
            double firstBound = (double)EvaluateExpression(node.FirstBoundary);
            double secondBound = (double)EvaluateExpression(node.SecondBoundary);

            double step = (double)EvaluateExpression(node.Step);
            if (firstBound == secondBound)
                throw new Exception("add errors to evaluator - step not absolute value");


            if (step <= 0)
                throw new Exception("add errors to evaluator - step is not positive");

            step *= Math.Sign(secondBound - firstBound);
            int op = Math.Sign(step);

            for (double i = firstBound; i * op <= secondBound * op; i += step)
            {
                _variables[node.Variable] = i;
                EvaluateStatement(node.Body);
            }
        }

        private void EvaluateWhileStatement(BoundWhileStatement node)
        {
            while ((bool)EvaluateExpression(node.Condition))
                EvaluateStatement(node.Body);
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
                BoundBinaryOperatorKind.Addition => (double)left + (double)right,
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
        }
    }
}