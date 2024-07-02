using System.Collections.Immutable;
using Yearl.CodeAnalysis.Lowering;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;
using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly ErrorHandler _errors = new();
        private readonly FunctionSymbol _function;

        private Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> _loopStack = new();
        private int _labelCounter;
        private BoundScope _scope;

        public ErrorHandler Errors => _errors;

        public Binder(BoundScope parent, FunctionSymbol function)
        {
            _scope = new BoundScope(parent);
            _function = function;

            if (function != null)
            {
                foreach (ParameterSymbol p in function.Parameters)
                    _scope.TryDeclareVariable(p);
            }
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, SyntaxUnitCompilation syntax)
        {
            BoundScope parentScope = CreateParentScope(previous);
            Binder binder = new(parentScope, function: null);

            foreach (SyntaxStatementFunctionDeclaration function in syntax.Members.OfType<SyntaxStatementFunctionDeclaration>())
                binder.BindFunctionDeclarationStatement(function);

            ImmutableArray<BoundStatement>.Builder statements = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach (SyntaxStatementGlobal globalStatement in syntax.Members.OfType<SyntaxStatementGlobal>())
            {
                BoundStatement statement = binder.BindStatement(globalStatement.Statement);
                statements.Add(statement);
            }

            ImmutableArray<FunctionSymbol> functions = binder._scope.GetDeclaredFunctions();
            ImmutableArray<VariableSymbol> variables = binder._scope.GetDeclaredVariables();
            var errors = binder.Errors.ToImmutableArray();

            if (previous != null)
                errors = errors.InsertRange(0, previous.Errors);

            return new BoundGlobalScope(previous, errors, functions, variables, statements.ToImmutable());
        }

        public static BoundProgram BindProgram(BoundGlobalScope globalScope)
        {
            BoundScope parentScope = CreateParentScope(globalScope);

            ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Builder functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            ImmutableArray<Error>.Builder errors = ImmutableArray.CreateBuilder<Error>();

            BoundGlobalScope scope = globalScope;

            while (scope != null)
            {
                foreach (FunctionSymbol function in scope.Functions)
                {
                    Binder binder = new(parentScope, function);
                    BoundStatement body = binder.BindStatement(function.Declaration.Body);
                    BoundBlockStatement loweredBody = Lowerer.Lower(body);

                    if (function.Type != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                        binder._errors.ReportAllPathsMustReturn(function.Declaration.Identifier.Location);

                    functionBodies.Add(function, loweredBody);

                    errors.AddRange(binder.Errors);
                }

                scope = scope.Previous;
            }

            BoundBlockStatement statement = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));

            return new BoundProgram(errors.ToImmutable(), functionBodies.ToImmutable(), statement);
        }

        private void BindFunctionDeclarationStatement(SyntaxStatementFunctionDeclaration syntax)
        {
            ImmutableArray<ParameterSymbol>.Builder parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();

            HashSet<string> seenParameterNames = new();

            foreach (SyntaxParameter parameterSyntax in syntax.Parameters)
            {
                string? parameterName = parameterSyntax.Identifier.Text;
                TypeSymbol parameterType = BindTypeClause(parameterSyntax.Type);
                if (!seenParameterNames.Add(parameterName))
                {
                    _errors.ReportParameterAlreadyDeclared(parameterSyntax.Location, parameterName);
                }
                else
                {
                    ParameterSymbol parameter = new(parameterName, parameterType);
                    parameters.Add(parameter);
                }
            }

            TypeSymbol type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;

            FunctionSymbol function = new(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);

            if (function.Declaration.Identifier.Text != null && !_scope.TryDeclareFunction(function))
                _errors.ReportSymbolAlreadyDeclared(syntax.Identifier.Location, function.Name);
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

                foreach (FunctionSymbol f in previous.Functions)
                    scope.TryDeclareFunction(f);

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



        private BoundExpressionStatement BindErrorStatement()
        {
            return new BoundExpressionStatement(new BoundErrorExpression());
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
                SyntaxKind.BreakStatement => BindBreakStatement((SyntaxStatementBreak)syntax),
                SyntaxKind.ContinueStatement => BindContinueStatement((SyntaxStatementContinue)syntax),
                SyntaxKind.ReturnStatement => BindReturnStatement((SyntaxStatementReturn)syntax),
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
            TypeSymbol type = BindTypeClause(syntax.TypeClause);
            BoundExpression initializer = BindExpression(syntax.Initializer);
            TypeSymbol variableType = type ?? initializer.Type;
            VariableSymbol variable = BindVariableDeclaration(syntax.Identifier, isReadOnly, variableType);
            BoundExpression convertedInitializer = BindConversion(syntax.Initializer.Location, initializer, variableType);

            return new BoundVariableDeclarationStatement(variable, convertedInitializer);
        }

        private TypeSymbol? BindTypeClause(SyntaxTypeClause syntax)
        {
            if (syntax == null)
                return null;

            TypeSymbol? type = LookupType(syntax.Identifier.Text);
            if (type == null)
                _errors.ReportUndefinedType(syntax.Identifier.Location, syntax.Identifier.Text);

            return type;
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

            VariableSymbol variable = BindVariableDeclaration(syntax.Identifier, isReadOnly: true, TypeSymbol.Number);

            BoundExpression step = new BoundLiteralExpression(1d);
            if (syntax.StepExpression != null)
                step = BindExpression(syntax.StepExpression, TypeSymbol.Number);

            BoundStatement body = BindLoopBody(syntax.BodyStatement, out BoundLabel? breakLabel, out BoundLabel? continueLabel);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, firstBound, secondBound, step, body, breakLabel, continueLabel);

        }

        private BoundWhileStatement BindWhileStatement(SyntaxStatementWhile syntax)
        {
            BoundExpression condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            BoundStatement body = BindLoopBody(syntax.BodyStatement, out BoundLabel? breakLabel, out BoundLabel? continueLabel);
            return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
        }

        private BoundStatement BindLoopBody(SyntaxStatement body, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            _labelCounter++;
            breakLabel = new BoundLabel($"break{_labelCounter}");
            continueLabel = new BoundLabel($"continue{_labelCounter}");

            _loopStack.Push((breakLabel, continueLabel));
            BoundStatement boundBody = BindStatement(body);
            _loopStack.Pop();

            return boundBody;
        }

        private BoundStatement BindBreakStatement(SyntaxStatementBreak syntax)
        {
            if (_loopStack.Count == 0)
            {
                _errors.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement();
            }

            BoundLabel breakLabel = _loopStack.Peek().BreakLabel;
            return new BoundGotoStatement(breakLabel);
        }

        private BoundStatement BindContinueStatement(SyntaxStatementContinue syntax)
        {
            if (_loopStack.Count == 0)
            {
                _errors.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement();
            }

            BoundLabel continueLabel = _loopStack.Peek().ContinueLabel;
            return new BoundGotoStatement(continueLabel);
        }

        private BoundStatement BindReturnStatement(SyntaxStatementReturn syntax)
        {
            BoundExpression? expression = syntax.Expression == null ? null : BindExpression(syntax.Expression);

            if (_function == null)
            {
                _errors.ReportInvalidReturn(syntax.ReturnKeyword.Location);
            }
            else
            {
                if (_function.Type == TypeSymbol.Void)
                {
                    if (expression != null)
                        _errors.ReportInvalidReturnExpression(syntax.Expression.Location, _function.Name);
                }
                else
                {
                    if (expression == null)
                        _errors.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, _function.Type);
                    else
                        expression = BindConversion(syntax.Expression.Location, expression, _function.Type);
                }
            }

            return new BoundReturnStatement(expression);
        }

        private BoundExpressionStatement BindExpressionStatement(SyntaxStatementExpression syntax)
        {
            BoundExpression expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }



        private BoundExpression BindExpression(SyntaxExpression syntax, TypeSymbol targetType)
        {
            return BindConversion(syntax, targetType);
        }

        private BoundExpression BindExpression(SyntaxExpression syntax, bool canBeVoid = false)
        {
            BoundExpression result = BindExpressionInternal(syntax);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _errors.ReportExpressionMustHaveValue(syntax.Location);
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

            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundExpression.Type);
            if (boundOperator == null)
            {
                _errors.ReportUndefinedUnaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundExpression.Type);
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

            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator == null)
            {
                _errors.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
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
            if (syntax.IdentifierToken.IsMissing)
            {
                // Parser inserted token and included necessary Errors
                return new BoundErrorExpression();
            }

            VariableSymbol variable = BindVariableReference(syntax.IdentifierToken);
            if (variable == null)
                return new BoundErrorExpression();

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindVariableAssignmentExpression(SyntaxExpressionVariableAssignment syntax)
        {
            string name = syntax.IdentifierToken.Text;
            BoundExpression boundExpression = BindExpression(syntax.Expression);

            VariableSymbol variable = BindVariableReference(syntax.IdentifierToken);
            if (variable == null)
                return boundExpression;

            if (variable.IsReadOnly)
                _errors.ReportCannotAssign(syntax.EqualsToken.Location, name);

            BoundExpression convertedExpression = BindConversion(syntax.Expression.Location, boundExpression, variable.Type);

            return new BoundVariableAssignmentExpression(variable, convertedExpression);
        }

        private BoundExpression BindCallExpression(SyntaxExpressionCall syntax)
        {
            if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
                return BindConversion(syntax.Arguments[0], type, allowExplicit: true);

            ImmutableArray<BoundExpression>.Builder boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (SyntaxExpression argument in syntax.Arguments)
            {
                BoundExpression boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }

            Symbol? symbol = _scope.TryLookupSymbol(syntax.Identifier.Text);
            if (symbol == null)
            {
                _errors.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (symbol is not FunctionSymbol function)
            {
                _errors.ReportNotAFunction(syntax.Identifier.Location, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (syntax.Arguments.Count != function.Parameters.Length)
            {
                TextSpan span;
                if (syntax.Arguments.Count > function.Parameters.Length)
                {
                    SyntaxNode firstExceedingNode;
                    if (function.Parameters.Length > 0)
                        firstExceedingNode = syntax.Arguments.GetSeparator(function.Parameters.Length - 1);
                    else
                        firstExceedingNode = syntax.Arguments[0];
                    SyntaxExpression lastExceedingArgument = syntax.Arguments[syntax.Arguments.Count - 1];
                    span = TextSpan.FromBounds(firstExceedingNode.Span.Start, lastExceedingArgument.Span.End);
                }
                else
                {
                    span = syntax.CloseParenthesisToken.Span;
                }
                var location = new TextLocation(syntax.SyntaxTree.Text, span);
                _errors.ReportWrongArgumentCount(location, function.Name, function.Parameters.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }

            for (int i = 0; i < syntax.Arguments.Count; i++)
            {
                BoundExpression argument = boundArguments[i];
                ParameterSymbol parameter = function.Parameters[i];

                if (argument.Type != parameter.Type)
                {
                    _errors.ReportWrongArgumentType(syntax.Arguments[i].Location, parameter.Name, parameter.Type, argument.Type);
                    return new BoundErrorExpression();
                }
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(SyntaxExpression syntax, TypeSymbol type, bool allowExplicit = false)
        {
            BoundExpression expression = BindExpression(syntax);
            return BindConversion(syntax.Location, expression, type, allowExplicit);
        }

        private BoundExpression BindConversion(TextLocation errorLocation, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                    _errors.ReportCannotConvert(errorLocation, expression.Type, type);

                return new BoundErrorExpression();
            }

            if (!allowExplicit && conversion.IsExplicit)
            {
                _errors.ReportCannotConvertImplicitly(errorLocation, expression.Type, type);
            }

            if (conversion.IsIdentity)
                return expression;

            return new BoundConversionExpression(type, expression);
        }

        private VariableSymbol BindVariableDeclaration(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
        {
            string name = identifier.Text ?? "?";
            bool declare = !identifier.IsMissing;
            VariableSymbol variable = _function == null
                                ? new GlobalVariableSymbol(name, isReadOnly, type)
                                : new LocalVariableSymbol(name, isReadOnly, type);

            if (declare && !_scope.TryDeclareVariable(variable))
                _errors.ReportSymbolAlreadyDeclared(identifier.Location, name);

            return variable;
        }

        private VariableSymbol? BindVariableReference(SyntaxToken identifierToken)
        {
            string? name = identifierToken.Text;
            switch (_scope.TryLookupSymbol(name))
            {
                case VariableSymbol variable:
                    return variable;

                case null:
                    _errors.ReportUndefinedVariable(identifierToken.Location, name);
                    return null;

                default:
                    _errors.ReportNotAVariable(identifierToken.Location, name);
                    return null;
            }
        }

        private TypeSymbol? LookupType(string name) => name switch
        {
            "bool" => TypeSymbol.Bool,
            "num" => TypeSymbol.Number,
            "string" => TypeSymbol.String,
            _ => null,
        };
    }
}