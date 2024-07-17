using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Yearl.CodeAnalysis.Errors;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;
using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly bool _isScript;
        private readonly FunctionSymbol? _function;

        private readonly Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> _loopStack = new();
        private int _labelCounter;
        private BoundScope _scope;

        public ErrorHandler Errors { get; } = new();

        private Binder(bool isScript, BoundScope? parent, FunctionSymbol? function)
        {
            _scope = new BoundScope(parent);
            _isScript = isScript;
            _function = function;

            if (function != null)
            {
                foreach (var p in function.Parameters)
                    _scope.TryDeclareVariable(p);
            }
        }

        public static BoundGlobalScope BindGlobalScope(bool isScript, BoundGlobalScope? previous, ImmutableArray<SyntaxTree> syntaxTrees)
        {
            var parentScope = CreateParentScope(previous);
            Binder binder = new(isScript, parentScope, function: null);

            binder.Errors.AddRange(syntaxTrees.SelectMany(st => st.Errors));
            if (binder.Errors.Any())
                return new BoundGlobalScope(previous, [.. binder.Errors], null, null, [], [], []);

            var functionDeclarations = syntaxTrees.SelectMany(st => st.Root.Members)
                                                  .OfType<SyntaxStatementFunctionDeclaration>();

            foreach (var function in functionDeclarations)
                binder.BindFunctionDeclarationStatement(function);

            var globalStatements = syntaxTrees.SelectMany(st => st.Root.Members)
                                              .OfType<SyntaxStatementGlobal>();

            var statements = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach (var globalStatement in globalStatements)
            {
                var statement = binder.BindGlobalStatement(globalStatement.Statement);
                statements.Add(statement);
            }

            // Check global statements

            var firstGlobalStatementPerSyntaxTree = syntaxTrees.Select(st => st.Root.Members.OfType<SyntaxStatementGlobal>().FirstOrDefault())
                                                                 .Where(g => g != null)
                                                                 .ToArray();

            if (firstGlobalStatementPerSyntaxTree.Length > 1)
            {
                foreach (var globalStatement in firstGlobalStatementPerSyntaxTree)
                    binder.Errors.ReportOnlyOneFileCanHaveGlobalStatements(globalStatement!.Location);
            }

            // Check for main/script with global statements

            var functions = binder._scope.GetDeclaredFunctions();

            FunctionSymbol? mainFunction;
            FunctionSymbol? scriptFunction;

            if (isScript)
            {
                mainFunction = null;
                if (globalStatements.Any())
                {
                    scriptFunction = new FunctionSymbol("$eval", [], TypeSymbol.Dynamic, null);
                }
                else
                {
                    scriptFunction = null;
                }
            }
            else
            {
                mainFunction = functions.FirstOrDefault(f => f.Name == "main");
                scriptFunction = null;

                if (mainFunction != null)
                {
                    if (mainFunction.Type != TypeSymbol.Void || mainFunction.Parameters.Any())
                        binder.Errors.ReportMainMustHaveCorrectSignature(mainFunction.Declaration!.Identifier.Location);
                }

                if (globalStatements.Any())
                {
                    if (mainFunction != null)
                    {
                        binder.Errors.ReportCannotMixMainAndGlobalStatements(mainFunction.Declaration!.Identifier.Location);

                        foreach (var globalStatement in firstGlobalStatementPerSyntaxTree)
                            binder.Errors.ReportCannotMixMainAndGlobalStatements(mainFunction.Declaration!.Identifier.Location);
                    }
                    else
                    {
                        mainFunction = new FunctionSymbol("main", [], TypeSymbol.Void, null);
                    }
                }
            }

            var errors = binder.Errors.ToImmutableArray();
            var variables = binder._scope.GetDeclaredVariables();

            if (previous != null)
                errors = errors.InsertRange(0, previous.Errors);

            return new BoundGlobalScope(previous, errors, mainFunction, scriptFunction, functions, variables, statements.ToImmutable());
        }

        public static BoundProgram BindProgram(bool isScript, BoundProgram? previous, BoundGlobalScope globalScope)
        {
            var parentScope = CreateParentScope(globalScope);

            if (globalScope.Errors.Any())
                return new BoundProgram(previous, [.. globalScope.Errors], null, null, ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Empty);

            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var errors = ImmutableArray.CreateBuilder<Error>();

            foreach (var function in globalScope.Functions)
            {
                var binder = new Binder(isScript, parentScope, function);
                var body = binder.BindStatement(function.Declaration!.Body);
                var loweredBody = Lowerer.Lower(function, body);

                if (function.Type != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                    binder.Errors.ReportAllPathsMustReturn(function.Declaration.Identifier.Location);

                functionBodies.Add(function, loweredBody);

                errors.AddRange(binder.Errors);
            }

            if (globalScope.MainFunction != null && globalScope.Statements.Any())
            {
                var body = Lowerer.Lower(globalScope.MainFunction, new BoundBlockStatement(globalScope.Statements));
                functionBodies.Add(globalScope.MainFunction, body);
            }
            else if (globalScope.ScriptFunction != null)
            {
                var statements = globalScope.Statements;
                if (statements.Length == 1 &&
                    statements[0] is BoundExpressionStatement es &&
                    es.Expression.Type != TypeSymbol.Void)
                {
                    statements = statements.SetItem(0, new BoundReturnStatement(es.Expression));
                }
                else if (statements.Any() && statements.Last().Kind != BoundNodeKind.ReturnStatement)
                {
                    var nullValue = new BoundLiteralExpression("");
                    statements = statements.Add(new BoundReturnStatement(nullValue));
                }

                var body = Lowerer.Lower(globalScope.ScriptFunction, new BoundBlockStatement(statements));
                functionBodies.Add(globalScope.ScriptFunction, body);
            }

            return new BoundProgram(previous,
                                    errors.ToImmutable(),
                                    globalScope.MainFunction,
                                    globalScope.ScriptFunction,
                                    functionBodies.ToImmutable());
        }

        private static BoundScope CreateParentScope(BoundGlobalScope? previous)
        {
            Stack<BoundGlobalScope> stack = new();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            var parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                BoundScope scope = new(parent);

                foreach (var f in previous.Functions)
                    scope.TryDeclareFunction(f);

                foreach (var v in previous.Variables)
                    scope.TryDeclareVariable(v);

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            BoundScope result = new(null);

            foreach (var f in BuiltinFunctions.GetAll())
                result.TryDeclareFunction(f);

            return result;
        }



        private BoundExpressionStatement BindErrorStatement() => new(new BoundErrorExpression());


        private BoundStatement BindGlobalStatement(SyntaxStatement syntax) => BindStatement(syntax, isGlobal: true);

        private BoundStatement BindStatement(SyntaxStatement syntax, bool isGlobal = false)
        {
            var result = BindStatementInternal(syntax);

            if (!_isScript || !isGlobal)
            {
                if (result is BoundExpressionStatement es)
                {
                    bool isAllowedExpression = es.Expression.Kind == BoundNodeKind.ErrorExpression ||
                                               es.Expression.Kind == BoundNodeKind.VariableAssignmentExpression ||
                                               es.Expression.Kind == BoundNodeKind.VariableCompoundAssignmentExpression ||
                                               es.Expression.Kind == BoundNodeKind.CallExpression;
                    if (!isAllowedExpression)
                        Errors.ReportInvalidExpressionStatement(syntax.Location);
                }
            }

            return result;
        }

        private BoundStatement BindStatementInternal(SyntaxStatement syntax)
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
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);

            foreach (var statementSyntax in syntax.Statements)
            {
                var statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }

            _scope = _scope.Parent!;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundVariableDeclarationStatement BindVariableDeclarationStatement(SyntaxStatementVariableDeclaration syntax)
        {
            bool isReadOnly = syntax.Keyword.Kind == SyntaxKind.ConstKeyword;
            var type = BindTypeClause(syntax.TypeClause);
            var initializer = BindExpression(syntax.Initializer);
            var variableType = type ?? initializer.Type;
            var variable = BindVariableDeclaration(syntax.Identifier, isReadOnly, variableType, initializer.ConstantValue);
            var convertedInitializer = BindConversion(syntax.Initializer.Location, initializer, variableType);

            return new BoundVariableDeclarationStatement(variable, convertedInitializer);
        }

        [return: NotNullIfNotNull(nameof(syntax))]
        private TypeSymbol? BindTypeClause(SyntaxTypeClause? syntax)
        {
            if (syntax == null)
                return null;

            var type = LookupType(syntax.Identifier.Text);
            if (type == null)
                Errors.ReportUndefinedType(syntax.Identifier.Location, syntax.Identifier.Text);

            return type!;
        }

        private BoundIfStatement BindIfStatement(SyntaxStatementIf syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);

            if (condition.ConstantValue != null)
            {
                if ((bool)condition.ConstantValue.Value == false)
                    Errors.ReportUnreachableCode(syntax.BodyStatement);
                else if (syntax.ElseClause != null)
                    Errors.ReportUnreachableCode(syntax.ElseClause.ElseStatement);
            }

            var BodyStatement = BindStatement(syntax.BodyStatement);
            var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, BodyStatement, elseStatement);
        }

        private BoundForStatement BindForStatement(SyntaxStatementFor syntax)
        {
            var firstBound = BindExpression(syntax.Bound1, TypeSymbol.Number);
            var secondBound = BindExpression(syntax.Bound2, TypeSymbol.Number);

            _scope = new BoundScope(_scope);

            var variable = BindVariableDeclaration(syntax.Identifier, isReadOnly: true, TypeSymbol.Number);

            BoundExpression step = new BoundLiteralExpression(1d);
            if (syntax.StepExpression != null)
                step = BindExpression(syntax.StepExpression, TypeSymbol.Number);

            var body = BindLoopBody(syntax.BodyStatement, out var breakLabel, out var continueLabel);

            _scope = _scope.Parent!;

            return new BoundForStatement(variable, firstBound, secondBound, step, body, breakLabel, continueLabel);

        }

        private BoundWhileStatement BindWhileStatement(SyntaxStatementWhile syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);

            if (condition.ConstantValue != null)
            {
                if (!(bool)condition.ConstantValue.Value)
                {
                    Errors.ReportUnreachableCode(syntax.BodyStatement);
                }
            }

            var body = BindLoopBody(syntax.BodyStatement, out var breakLabel, out var continueLabel);
            return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
        }

        private BoundStatement BindLoopBody(SyntaxStatement body, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            _labelCounter++;
            breakLabel = new BoundLabel($"break{_labelCounter}");
            continueLabel = new BoundLabel($"continue{_labelCounter}");

            _loopStack.Push((breakLabel, continueLabel));
            var boundBody = BindStatement(body);
            _loopStack.Pop();

            return boundBody;
        }

        private BoundStatement BindBreakStatement(SyntaxStatementBreak syntax)
        {
            if (_loopStack.Count == 0)
            {
                Errors.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement();
            }

            var breakLabel = _loopStack.Peek().BreakLabel;
            return new BoundGotoStatement(breakLabel);
        }

        private BoundStatement BindContinueStatement(SyntaxStatementContinue syntax)
        {
            if (_loopStack.Count == 0)
            {
                Errors.ReportInvalidBreakOrContinue(syntax.Keyword.Location, syntax.Keyword.Text);
                return BindErrorStatement();
            }

            var continueLabel = _loopStack.Peek().ContinueLabel;
            return new BoundGotoStatement(continueLabel);
        }

        private BoundStatement BindReturnStatement(SyntaxStatementReturn syntax)
        {
            var expression = syntax.Expression == null ? null : BindExpression(syntax.Expression);

            if (_function == null)
            {
                if (_isScript)
                {
                    // Ignore because we allow both return with and without values.
                    expression ??= new BoundLiteralExpression("");
                }
                else if (expression != null)
                {
                    // Main does not support return values.
                    Errors.ReportInvalidReturnWithValueInGlobalStatements(syntax.Expression!.Location);
                }
            }
            else
            {
                if (_function.Type == TypeSymbol.Void)
                {
                    if (expression != null)
                        Errors.ReportInvalidReturnExpression(syntax.Expression!.Location, _function.Name);
                }
                else
                {
                    if (expression == null)
                        Errors.ReportMissingReturnExpression(syntax.ReturnKeyword.Location, _function.Type);
                    else
                        expression = BindConversion(syntax.Expression!.Location, expression, _function.Type);
                }
            }

            return new BoundReturnStatement(expression);
        }

        private BoundExpressionStatement BindExpressionStatement(SyntaxStatementExpression syntax)
        {
            var expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }



        private BoundExpression BindExpression(SyntaxExpression syntax, TypeSymbol targetType) => BindConversion(syntax, targetType);

        private BoundExpression BindExpression(SyntaxExpression syntax, bool canBeVoid = false)
        {
            var result = BindExpressionInternal(syntax);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                Errors.ReportExpressionMustHaveValue(syntax.Location);
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
            var boundExpression = BindExpression(syntax.Expression);
            if (boundExpression.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundExpression.Type);
            if (boundOperator == null)
            {
                Errors.ReportUndefinedUnaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundExpression.Type);
                return new BoundErrorExpression();
            }

            return new BoundUnaryExpression(boundOperator, boundExpression);
        }

        private BoundExpression BindBinaryExpression(SyntaxExpressionBinary syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);

            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperator == null)
            {
                Errors.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
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

            var variable = BindVariableReference(syntax.IdentifierToken);
            if (variable == null)
                return new BoundErrorExpression();

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindVariableAssignmentExpression(SyntaxExpressionVariableAssignment syntax)
        {
            string name = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            var variable = BindVariableReference(syntax.IdentifierToken);
            if (variable == null)
                return boundExpression;

            if (variable.IsReadOnly)
                Errors.ReportCannotAssign(syntax.AssignmentToken.Location, name);

            if (syntax.AssignmentToken.Kind != SyntaxKind.EqualsToken)
            {
                var equivalentOperatorTokenKind = SyntaxFacts.GetBinaryOperatorOfAssignmentOperator(syntax.AssignmentToken.Kind);
                var boundOperator = BoundBinaryOperator.Bind(equivalentOperatorTokenKind, variable.Type, boundExpression.Type);

                if (boundOperator == null)
                {
                    Errors.ReportUndefinedBinaryOperator(syntax.AssignmentToken.Location, syntax.AssignmentToken.Text, variable.Type, boundExpression.Type);
                    return new BoundErrorExpression();
                }

                var convertedExpression = BindConversion(syntax.Expression.Location, boundExpression, variable.Type);
                return new BoundVariableCompoundAssignmentExpression(variable, boundOperator, convertedExpression);
            }
            else
            {
                var convertedExpression = BindConversion(syntax.Expression.Location, boundExpression, variable.Type);
                return new BoundVariableAssignmentExpression(variable, convertedExpression);
            }
        }

        private void BindFunctionDeclarationStatement(SyntaxStatementFunctionDeclaration syntax)
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();

            HashSet<string> seenParameterNames = [];

            int ordinal = 0;
            foreach (var parameterSyntax in syntax.Parameters)
            {
                string parameterName = parameterSyntax.Identifier.Text;
                var parameterType = BindTypeClause(parameterSyntax.Type);
                if (!seenParameterNames.Add(parameterName))
                {
                    Errors.ReportParameterAlreadyDeclared(parameterSyntax.Location, parameterName);
                }
                else
                {
                    ParameterSymbol parameter = new(parameterName, parameterType, ordinal);
                    parameters.Add(parameter);
                    ordinal++;
                }
            }

            var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;

            FunctionSymbol function = new(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);

            if (syntax.Identifier.Text != null && !_scope.TryDeclareFunction(function))
                Errors.ReportSymbolAlreadyDeclared(syntax.Identifier.Location, function.Name);
        }

        private BoundExpression BindCallExpression(SyntaxExpressionCall syntax)
        {
            if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
                return BindConversion(syntax.Arguments[0], type, allowExplicit: true);

            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (var argument in syntax.Arguments)
            {
                var boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }

            var symbol = _scope.TryLookupSymbol(syntax.Identifier.Text);
            if (symbol == null)
            {
                Errors.ReportUndefinedFunction(syntax.Identifier.Location, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (symbol is not FunctionSymbol function)
            {
                Errors.ReportNotAFunction(syntax.Identifier.Location, syntax.Identifier.Text);
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
                    var lastExceedingArgument = syntax.Arguments[^1];
                    span = TextSpan.FromBounds(firstExceedingNode.Span.Start, lastExceedingArgument.Span.End);
                }
                else
                {
                    span = syntax.CloseParenthesisToken.Span;
                }
                TextLocation location = new(syntax.SyntaxTree.Text, span);
                Errors.ReportWrongArgumentCount(location, function.Name, function.Parameters.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }

            for (int i = 0; i < syntax.Arguments.Count; i++)
            {
                var argumentLocation = syntax.Arguments[i].Location;
                var argument = boundArguments[i];
                var parameter = function.Parameters[i];
                boundArguments[i] = BindConversion(argumentLocation, argument, parameter.Type);
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(SyntaxExpression syntax, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = BindExpression(syntax);
            return BindConversion(syntax.Location, expression, type, allowExplicit);
        }

        private BoundExpression BindConversion(TextLocation errorLocation, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                    Errors.ReportCannotConvert(errorLocation, expression.Type, type);

                return new BoundErrorExpression();
            }

            if (!allowExplicit && conversion.IsExplicit)
            {
                Errors.ReportCannotConvertImplicitly(errorLocation, expression.Type, type);
            }

            if (conversion.IsIdentity)
                return expression;

            return new BoundConversionExpression(type, expression);
        }

        private VariableSymbol BindVariableDeclaration(SyntaxToken identifier, bool isReadOnly, TypeSymbol type, BoundConstant? constant = null)
        {
            string name = identifier.Text ?? "?";
            bool declare = !identifier.IsMissing;
            VariableSymbol variable = _function == null
                                ? new GlobalVariableSymbol(name, isReadOnly, type, constant)
                                : new LocalVariableSymbol(name, isReadOnly, type, constant);

            if (declare && !_scope.TryDeclareVariable(variable))
                Errors.ReportSymbolAlreadyDeclared(identifier.Location, name);

            return variable;
        }

        private VariableSymbol? BindVariableReference(SyntaxToken identifierToken)
        {
            string name = identifierToken.Text;
            switch (_scope.TryLookupSymbol(name))
            {
                case VariableSymbol variable:
                    return variable;

                case null:
                    Errors.ReportUndefinedVariable(identifierToken.Location, name);
                    return null;

                default:
                    Errors.ReportNotAVariable(identifierToken.Location, name);
                    return null;
            }
        }

        private TypeSymbol? LookupType(string name)
        {
            return name switch
            {
                "Bool" => TypeSymbol.Bool,
                "Dynamic" => TypeSymbol.Dynamic,
                "Number" => TypeSymbol.Number,
                "String" => TypeSymbol.String,
                "Void" => TypeSymbol.Void,
                _ => null,
            };
        }
    }
}