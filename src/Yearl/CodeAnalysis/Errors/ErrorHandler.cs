using Mono.Cecil;
using System.Collections;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;
using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis.Errors
{
    internal sealed class ErrorHandler : IEnumerable<Error>
    {
        private readonly List<Error> _errors = [];

        public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(IEnumerable<Error> Errors) => _errors.AddRange(Errors);

        private void Report(TextLocation location, string message) => _errors.Add(new Error(location, message));



        public void ReportInvalidNumber(TextLocation location, string text, TypeSymbol type)
        {
            string message = $"The number {text} isn't of valid type <{type}>.";
            Report(location, message);
        }
        public void ReportUnterminatedString(TextLocation location)
        {
            string message = "Unterminated string literal.";
            Report(location, message);
        }

        public void ReportInvalidCharacter(TextLocation location, char character)
        {
            string message = $"Invalid character input: '{character}'.";
            Report(location, message);
        }

        public void ReportUnterminatedMultiLineComment(TextLocation location)
        {
            string message = "Unterminated multi-line comment.";
            Report(location, message);
        }

        public void ReportUnexpectedToken(TextLocation location, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            string message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(location, message);
        }

        public void ReportUndefinedUnaryOperator(TextLocation location, string operatorText, TypeSymbol operandType)
        {
            string message = $"Unary operator '{operatorText}' is not defined for type '{operandType}'.";
            Report(location, message);
        }

        public void ReportUndefinedBinaryOperator(TextLocation location, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            string message = $"Binary operator '{operatorText}' is not defined for types '{leftType}' and '{rightType}'.";
            Report(location, message);
        }

        public void ReportParameterAlreadyDeclared(TextLocation location, string parameterName)
        {
            string message = $"A parameter with the name '{parameterName}' already exists.";
            Report(location, message);
        }

        public void ReportUndefinedVariable(TextLocation location, string name)
        {
            string message = $"Variable '{name}' doesn't exist.";
            Report(location, message);
        }

        public void ReportNotAVariable(TextLocation location, string name)
        {
            string message = $"'{name}' is not a variable.";
            Report(location, message);
        }

        public void ReportUndefinedType(TextLocation location, string name)
        {
            string message = $"Type '{name}' doesn't exist.";
            Report(location, message);
        }

        public void ReportCannotConvert(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            string message = $"Cannot convert type '{fromType}' to '{toType}'.";
            Report(location, message);
        }

        public void ReportCannotConvertImplicitly(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
        {
            string message = $"Cannot convert type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?)";
            Report(location, message);
        }

        public void ReportSymbolAlreadyDeclared(TextLocation location, string name)
        {
            string message = $"'{name}' is already declared.";
            Report(location, message);
        }

        public void ReportCannotAssign(TextLocation location, string name)
        {
            string message = $"Variable '{name}' is read-only and cannot be assigned to.";
            Report(location, message);
        }
        public void ReportUndefinedFunction(TextLocation location, string name)
        {
            string message = $"Function '{name}' doesn't exist.";
            Report(location, message);
        }

        public void ReportNotAFunction(TextLocation location, string name)
        {
            string message = $"'{name}' is not a function.";
            Report(location, message);
        }

        public void ReportWrongArgumentCount(TextLocation location, string name, int expectedCount, int actualCount)
        {
            string message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
            Report(location, message);
        }

        public void ReportExpressionMustHaveValue(TextLocation location)
        {
            string message = "Expression must have a value.";
            Report(location, message);
        }

        public void ReportInvalidBreakOrContinue(TextLocation location, string text)
        {
            string message = $"The keyword '{text}' can only be used inside of loops.";
            Report(location, message);
        }

        public void ReportAllPathsMustReturn(TextLocation location)
        {
            string message = "Not all code paths return a value.";
            Report(location, message);
        }

        public void ReportInvalidReturnExpression(TextLocation location, string functionName)
        {
            string message = $"Since the function '{functionName}' does not return a value the return statement cannot contain an expression.";
            Report(location, message);
        }
        public void ReportInvalidReturnWithValueInGlobalStatements(TextLocation location)
        {
            string message = "The return statement cannot have a value when used as global statement.";
            Report(location, message);
        }

        public void ReportMissingReturnExpression(TextLocation location, TypeSymbol returnType)
        {
            string message = $"An expression of type '{returnType}' is expected.";
            Report(location, message);
        }

        public void ReportInvalidExpressionStatement(TextLocation location)
        {
            string message = $"Only assignment and call expressions can be used as a statement.";
            Report(location, message);
        }
        public void ReportOnlyOneFileCanHaveGlobalStatements(TextLocation location)
        {
            string message = $"At most one file can have global statements.";
            Report(location, message);
        }

        public void ReportMainMustHaveCorrectSignature(TextLocation location)
        {
            string message = $"main must not take arguments and not return anything.";
            Report(location, message);
        }

        public void ReportCannotMixMainAndGlobalStatements(TextLocation location)
        {
            string message = $"Cannot declare main function when global statements are used.";
            Report(location, message);
        }

        public void ReportInvalidReference(string path)
        {
            string message = $"The reference is not a valid .NET assembly: '{path}'.";
            Report(default, message);
        }

        public void ReportRequiredTypeNotFound(string? yearlName, string metadataName)
        {
            string message = yearlName == null
                ? $"The required type '{metadataName}' cannot be resolved among the given references."
                : $"The required type '{yearlName}' ('{metadataName}') cannot be resolved among the given references.";
            Report(default, message);
        }

        public void ReportRequiredTypeAmbiguous(string? yearlName, string metadataName, TypeDefinition[] foundTypes)
        {
            var assemblyNames = foundTypes.Select(t => t.Module.Assembly.Name.Name);
            string assemblyNameList = string.Join(", ", assemblyNames);
            string message = yearlName == null
                ? $"The required type '{metadataName}' was found in multiple references: {assemblyNameList}."
                : $"The required type '{yearlName}' ('{metadataName}') was found in multiple references: {assemblyNameList}.";
            Report(default, message);
        }

        public void ReportRequiredMethodNotFound(string typeName, string methodName, string[] parameterTypeNames)
        {
            string parameterTypeNameList = string.Join(", ", parameterTypeNames);
            string message = $"The required method '{typeName}.{methodName}({parameterTypeNameList})' cannot be resolved among the given references.";
            Report(default, message);
        }

        public void ReportUnreachableCode(TextLocation location)
        {
            string message = $"Unreachable code detected.";
            Report(location, message);
        }

        public void ReportUnreachableCode(SyntaxNode node)
        {
            switch (node.Kind)
            {
                case SyntaxKind.BlockStatement:
                    var firstStatement = ((SyntaxStatementBlock)node).Statements.FirstOrDefault();
                    // Report just for non empty blocks.
                    if (firstStatement != null)
                        ReportUnreachableCode(firstStatement);
                    return;
                case SyntaxKind.VariableDeclarationStatement:
                    ReportUnreachableCode(((SyntaxStatementVariableDeclaration)node).Keyword.Location);
                    return;
                case SyntaxKind.IfStatement:
                    ReportUnreachableCode(((SyntaxStatementIf)node).IfKeyword.Location);
                    return;
                case SyntaxKind.WhileStatement:
                    ReportUnreachableCode(((SyntaxStatementWhile)node).WhileKeyword.Location);
                    return;
                case SyntaxKind.ForStatement:
                    ReportUnreachableCode(((SyntaxStatementFor)node).ForKeyword.Location);
                    return;
                case SyntaxKind.BreakStatement:
                    ReportUnreachableCode(((SyntaxStatementBreak)node).Keyword.Location);
                    return;
                case SyntaxKind.ContinueStatement:
                    ReportUnreachableCode(((SyntaxStatementContinue)node).Keyword.Location);
                    return;
                case SyntaxKind.ReturnStatement:
                    ReportUnreachableCode(((SyntaxStatementReturn)node).ReturnKeyword.Location);
                    return;
                case SyntaxKind.ExpressionStatement:
                    var expression = ((SyntaxStatementExpression)node).Expression;
                    ReportUnreachableCode(expression);
                    return;
                case SyntaxKind.CallExpression:
                    ReportUnreachableCode(((SyntaxExpressionCall)node).Identifier.Location);
                    return;
                default:
                    throw new Exception($"Unexpected syntax {node.Kind}");
            }
        }
    }
}