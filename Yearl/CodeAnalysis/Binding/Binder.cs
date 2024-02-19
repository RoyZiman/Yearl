using Yearl.Language.Syntax;

namespace Yearl.Language.Binding
{
    internal sealed class Binder()
    {
        private readonly Dictionary<VariableSymbol, object?> _variables = new Dictionary<VariableSymbol, object?>();
        private ErrorHandler _errors = new ErrorHandler();

        public ErrorHandler Errors => _errors;


        public BoundNode BindNode(SyntaxNode syntaxNode)
        {
            return BindExpression((SyntaxExpression)syntaxNode);
        }


        public BoundExpression BindExpression(SyntaxExpression syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((SyntaxExpressionLiteral)syntax);

                case SyntaxKind.ParenthesizedExpression:
                    return BindExpression(((SyntaxExpressionParenthesized)syntax).Expression);

                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((SyntaxExpressionUnary)syntax);

                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((SyntaxExpressionBinary)syntax);

                case SyntaxKind.NameExpression:
                    return BindNameExpression((SyntaxExpressionName)syntax);

                case SyntaxKind.VariableAssignmentExpression:
                    return BindVariableAssignmentExpression((SyntaxExpressionVariableAssignment)syntax);

                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");

            }
        }

        private BoundLiteralExpression BindLiteralExpression(SyntaxExpressionLiteral syntax)
        {
            object value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(SyntaxExpressionUnary syntax)
        {
            BoundExpression boundExpression = BindExpression(syntax.Expression);
            BoundUnaryOperator boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundExpression.Type);

            if (boundOperator == null)
            {
                _errors.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundExpression.Type);
                return boundExpression;
            }

            return new BoundUnaryExpression(boundOperator, boundExpression);
        }

        private BoundExpression BindBinaryExpression(SyntaxExpressionBinary syntax)
        {
            BoundExpression boundLeft = BindExpression(syntax.Left);
            BoundExpression boundRight = BindExpression(syntax.Right);
            BoundBinaryOperator boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator == null)
            {
                _errors.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return boundLeft;
            }

            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

        private BoundExpression BindNameExpression(SyntaxExpressionName syntax)
        {
            string name = syntax.IdentifierToken.Text;
            VariableSymbol? variable = _variables.Keys.FirstOrDefault(v => v.Name == name);

            if (variable == null)
            {
                _errors.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundLiteralExpression(0.0);
            }

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindVariableAssignmentExpression(SyntaxExpressionVariableAssignment syntax)
        {
            string name = syntax.IdentifierToken.Text;
            BoundExpression boundExpression = BindExpression(syntax.Expression);

            VariableSymbol? existingVariable = _variables.Keys.FirstOrDefault(v => v.Name == name);
            if (existingVariable != null)
                _variables.Remove(existingVariable);

            VariableSymbol variable = new VariableSymbol(name, boundExpression.Type);
            _variables[variable] = null;

            return new BoundVariableAssignmentExpression(variable, boundExpression);
        }
    }
}