using System.Collections;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;
using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis
{
    internal sealed class ErrorHandler : IEnumerable<Error>
    {
        private readonly List<Error> _errors = [];

        public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(ErrorHandler Errors)
        {
            _errors.AddRange(Errors);
        }

        private void Report(TextLocation location, string message)
        {
            _errors.Add(new Error(location, message));
        }



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

        public void ReportWrongArgumentType(TextLocation location, string name, TypeSymbol expectedType, TypeSymbol actualType)
        {
            string message = $"Parameter '{name}' requires a value of type '{expectedType}' but was given a value of type '{actualType}'.";
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

        public void ReportInvalidReturn(TextLocation location)
        {
            string message = "The 'return' keyword can only be used inside of functions.";
            Report(location, message);
        }

        public void ReportInvalidReturnExpression(TextLocation location, string functionName)
        {
            string message = $"Since the function '{functionName}' does not return a value the return statement cannot contain an expression.";
            Report(location, message);
        }

        public void ReportMissingReturnExpression(TextLocation location, TypeSymbol returnType)
        {
            string message = $"An expression of type '{returnType}' is expected.";
            Report(location, message);
        }
    }
}