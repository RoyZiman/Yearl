using System.Collections.Immutable;
using Yearl.CodeAnalysis.Symbols;
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
            ImmutableArray<Error> errors = binder.Errors.ToImmutableArray();

            if (previous != null)
                errors = errors.InsertRange(0, previous.Errors);

            return new BoundGlobalScope(previous, errors, variables, expression);
        }

        private static BoundScope CreateParentScope(BoundGlobalScope previous)
        {
            Stack<BoundGlobalScope> stack = new();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                BoundScope scope = new(parent);
                foreach (VariableSymbol v in previous.Variables)
                    scope.TryDeclareVariable(v);

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            BoundScope result = new(null);

            foreach (FunctionSymbol f in BuiltinFunctions.GetAll())
                result.TryDeclareFunction(f);

            return result;
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
            bool isReadOnly = syntax.Keyword.Kind == SyntaxKind.ConstKeyword;
            BoundExpression initializer = BindExpression(syntax.Initializer);

            VariableSymbol variable = BindVariable(syntax.Identifier, isReadOnly, initializer.Type);

            return new BoundVariableDeclarationStatement(variable, initializer);
        }

        private BoundIfStatement BindIfStatement(SyntaxStatementIf syntax)
        {
            BoundExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            BoundStatement BodyStatement = BindStatement(syntax.BodyStatement);
            BoundStatement? elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, BodyStatement, elseStatement);
        }

        private BoundForStatement BindForStatement(SyntaxStatementFor syntax)
        {
            BoundExpression firstBound = BindExpression(syntax.Bound1, TypeSymbol.Number);
            BoundExpression secondBound = BindExpression(syntax.Bound2, TypeSymbol.Number);

            _scope = new BoundScope(_scope);

            VariableSymbol variable = BindVariable(syntax.Identifier, isReadOnly: true, TypeSymbol.Number);

            BoundExpression step = new BoundLiteralExpression(1.0);
            if (syntax.StepExpression != null)
                step = BindExpression(syntax.StepExpression, TypeSymbol.Number);

            BoundStatement body = BindStatement(syntax.BodyStatement);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, firstBound, secondBound, step, body);

        }

        private BoundWhileStatement BindWhileStatement(SyntaxStatementWhile syntax)
        {
            BoundExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            BoundStatement BodyStatement = BindStatement(syntax.BodyStatement);
            return new BoundWhileStatement(condition, BodyStatement);
        }

        private BoundExpressionStatement BindExpressionStatement(SyntaxStatementExpression syntax)
        {
            BoundExpression expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(SyntaxExpression syntax, TypeSymbol targetType)
        {
            BoundExpression result = BindExpression(syntax);
            if (targetType != TypeSymbol.Error &&
                result.Type != TypeSymbol.Error &&
                result.Type != targetType)
                _errors.ReportCannotConvert(syntax.Span, result.Type, targetType);

            return result;
        }

        private BoundExpression BindExpression(SyntaxExpression syntax, bool canBeVoid = false)
        {
            BoundExpression result = BindExpressionInternal(syntax);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _errors.ReportExpressionMustHaveValue(syntax.Span);
                return new BoundErrorExpression();
            }

            return result;
        }

        private BoundExpression BindExpressionInternal(SyntaxExpression syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.LiteralExpression => BindLiteralExpression((SyntaxExpressionLiteral)syntax),
                SyntaxKind.ParenthesizedExpression => BindExpression(((SyntaxExpressionParenthesized)syntax).Expression),
                SyntaxKind.UnaryExpression => BindUnaryExpression((SyntaxExpressionUnary)syntax),
                SyntaxKind.BinaryExpression => BindBinaryExpression((SyntaxExpressionBinary)syntax),
                SyntaxKind.NameExpression => BindNameExpression((SyntaxExpressionName)syntax),
                SyntaxKind.VariableAssignmentExpression => BindVariableAssignmentExpression((SyntaxExpressionVariableAssignment)syntax),
                SyntaxKind.CallExpression => BindCallExpression((SyntaxExpressionCall)syntax),
                _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
            };
        }

        private BoundExpression BindUnaryExpression(SyntaxExpressionUnary syntax)
        {
            BoundExpression boundExpression = BindExpression(syntax.Expression);
            if (boundExpression.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            BoundUnaryOperator boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundExpression.Type);
            if (boundOperator == null)
            {
                _errors.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundExpression.Type);
                return new BoundErrorExpression();
            }

            return new BoundUnaryExpression(boundOperator, boundExpression);
        }

        private BoundExpression BindBinaryExpression(SyntaxExpressionBinary syntax)
        {
            BoundExpression boundLeft = BindExpression(syntax.Left);
            BoundExpression boundRight = BindExpression(syntax.Right);

            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            BoundBinaryOperator boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator == null)
            {
                _errors.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
            }

            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

        private BoundLiteralExpression BindLiteralExpression(SyntaxExpressionLiteral syntax)
        {
            object value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindNameExpression(SyntaxExpressionName syntax)
        {
            string name = syntax.IdentifierToken.Text;

            if (syntax.IdentifierToken.IsMissing)
            {
                // Parser inserted token and included necessary Errors
                return new BoundErrorExpression();
            }

            if (!_scope.TryLookupVariable(name, out VariableSymbol variable))
            {
                _errors.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();
            }

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindVariableAssignmentExpression(SyntaxExpressionVariableAssignment syntax)
        {
            string name = syntax.IdentifierToken.Text;
            BoundExpression boundExpression = BindExpression(syntax.Expression);

            if (!_scope.TryLookupVariable(name, out VariableSymbol? variable))
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

        private BoundExpression BindCallExpression(SyntaxExpressionCall syntax)
        {
            ImmutableArray<BoundExpression>.Builder boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (SyntaxExpression argument in syntax.Arguments)
            {
                BoundExpression boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }

            IEnumerable<FunctionSymbol> functions = BuiltinFunctions.GetAll();

            FunctionSymbol? function = functions.SingleOrDefault(f => f.Name == syntax.Identifier.Text);
            if (function == null)
            {
                _errors.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (syntax.Arguments.Count != function.Parameter.Length)
            {
                _errors.ReportWrongArgumentCount(syntax.Span, function.Name, function.Parameter.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }

            for (int i = 0; i < syntax.Arguments.Count; i++)
            {
                BoundExpression argument = boundArguments[i];
                ParameterSymbol parameter = function.Parameter[i];

                if (argument.Type != parameter.Type)
                {
                    _errors.ReportWrongArgumentType(syntax.Span, parameter.Name, parameter.Type, argument.Type);
                    return new BoundErrorExpression();
                }
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
        {
            string name = identifier.Text ?? "?";
            bool declare = !identifier.IsMissing;
            VariableSymbol variable = new(name, isReadOnly, type);

            if (declare && !_scope.TryDeclareVariable(variable))
                _errors.ReportVariableAlreadyDeclared(identifier.Span, name);

            return variable;
        }
    }
}