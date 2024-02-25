using System.Collections.Immutable;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class Binder(BoundScope parent)
    {
        private readonly ErrorHandler _errors = new();
        private BoundScope _scope = new(parent);

        public ErrorHandler Errors => _errors;

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, SyntaxUnitCompilation syntax)
        {
            BoundScope parentScope = CreateParentScope(previous);
            Binder binder = new(parentScope);
            BoundStatement expression = binder.BindStatement(syntax.Statement);
            ImmutableArray<VariableSymbol> variables = binder._scope.GetDeclaredVariables();
            ImmutableArray<Error> diagnostics = binder.Errors.ToImmutableArray();

            if (previous != null)
                diagnostics = diagnostics.InsertRange(0, previous.Errors);

            return new BoundGlobalScope(previous, diagnostics, variables, expression);
        }

        private static BoundScope CreateParentScope(BoundGlobalScope previous)
        {
            Stack<BoundGlobalScope> stack = new();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope? parent = null;

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                BoundScope scope = new(parent);
                foreach (VariableSymbol v in previous.Variables)
                    scope.TryDeclare(v);

                parent = scope;
            }

            return parent;
        }



        private BoundStatement BindStatement(SyntaxStatement syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.BlockStatement => BindBlockStatement((SyntaxStatementBlock)syntax),
                SyntaxKind.ExpressionStatement => BindExpressionStatement((SyntaxStatementExpression)syntax),
                SyntaxKind.VariableDeclarationStatement => BindVariableDeclarationStatement((SyntaxStatementVariableDeclaration)syntax),
                SyntaxKind.IfStatement => BindIfStatement((SyntaxStatementIf)syntax),
                SyntaxKind.ForStatement => BindForStatement((SyntaxStatementFor)syntax),
                SyntaxKind.WhileStatement => BindWhileStatement((SyntaxStatementWhile)syntax),
                _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
            };
        }

        private BoundBlockStatement BindBlockStatement(SyntaxStatementBlock syntax)
        {
            ImmutableArray<BoundStatement>.Builder statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);

            foreach (SyntaxStatement statementSyntax in syntax.Statements)
            {
                BoundStatement statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }

            _scope = _scope.Parent;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundVariableDeclarationStatement BindVariableDeclarationStatement(SyntaxStatementVariableDeclaration syntax)
        {
            string name = syntax.Identifier.Text;
            bool isReadOnly = syntax.Keyword.Kind == SyntaxKind.ConstKeyword;
            BoundExpression initializer = BindExpression(syntax.Initializer);
            VariableSymbol variable = new(name, isReadOnly, initializer.Type);

            if (!_scope.TryDeclare(variable))
                _errors.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);

            return new BoundVariableDeclarationStatement(variable, initializer);
        }

        private BoundIfStatement BindIfStatement(SyntaxStatementIf syntax)
        {
            BoundExpression condition = BindExpression(syntax.Condition, typeof(bool));
            BoundStatement BodyStatement = BindStatement(syntax.BodyStatement);
            BoundStatement? elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, BodyStatement, elseStatement);
        }

        private BoundForStatement BindForStatement(SyntaxStatementFor syntax)
        {
            BoundExpression firstBound = BindExpression(syntax.Bound1, typeof(double));
            BoundExpression secondBound = BindExpression(syntax.Bound2, typeof(double));

            _scope = new BoundScope(_scope);

            string name = syntax.Identifier.Text;
            VariableSymbol variable = new(name, true, typeof(double));
            if (!_scope.TryDeclare(variable))
                _errors.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);

            BoundExpression step = new BoundLiteralExpression(1.0);
            if (syntax.StepExpression != null)
                step = BindExpression(syntax.StepExpression, typeof(double));

            BoundStatement body = BindStatement(syntax.BodyStatement);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, firstBound, secondBound, step, body);

        }

        private BoundWhileStatement BindWhileStatement(SyntaxStatementWhile syntax)
        {
            BoundExpression condition = BindExpression(syntax.Condition, typeof(bool));
            BoundStatement BodyStatement = BindStatement(syntax.BodyStatement);
            return new BoundWhileStatement(condition, BodyStatement);
        }

        private BoundExpressionStatement BindExpressionStatement(SyntaxStatementExpression syntax)
        {
            BoundExpression expression = BindExpression(syntax.Expression);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(SyntaxExpression syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.LiteralExpression => BindLiteralExpression((SyntaxExpressionLiteral)syntax),
                SyntaxKind.ParenthesizedExpression => BindExpression(((SyntaxExpressionParenthesized)syntax).Expression),
                SyntaxKind.UnaryExpression => BindUnaryExpression((SyntaxExpressionUnary)syntax),
                SyntaxKind.BinaryExpression => BindBinaryExpression((SyntaxExpressionBinary)syntax),
                SyntaxKind.NameExpression => BindNameExpression((SyntaxExpressionName)syntax),
                SyntaxKind.VariableAssignmentExpression => BindVariableAssignmentExpression((SyntaxExpressionVariableAssignment)syntax),
                _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
            };
        }

        private BoundExpression BindExpression(SyntaxExpression syntax, Type targetType)
        {
            BoundExpression result = BindExpression(syntax);
            if (result.Type != targetType)
                _errors.ReportCannotConvert(syntax.Span, result.Type, targetType);

            return result;
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

            if (string.IsNullOrEmpty(name))
            {
                // Parser inserted token and included necessary Errors
                return new BoundLiteralExpression(0);
            }

            if (!_scope.TryLookup(name, out VariableSymbol variable))
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

            if (!_scope.TryLookup(name, out VariableSymbol? variable))
            {
                _errors.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }

            if (variable.IsReadOnly)
                _errors.ReportCannotAssign(syntax.EqualsToken.Span, name);

            if (boundExpression.Type != variable?.Type)
            {
                _errors.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
                return boundExpression;
            }

            return new BoundVariableAssignmentExpression(variable, boundExpression);
        }
    }
}