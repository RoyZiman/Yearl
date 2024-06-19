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

        private void Report(TextSpan span, string message)
        {
            _errors.Add(new Error(span, message));
        }



        public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
        {
            string message = $"The number {text} isn't of valid type <{type}.";
            Report(span, message);
        }
        public void ReportUnterminatedString(TextSpan span)
        {
            string message = "Unterminated string literal.";
            Report(span, message);
        }

        public void ReportInvalidCharacter(int position, char character)
        {
            TextSpan span = new(position, 1);
            string message = $"Invalid character input: '{character}'.";
            Report(span, message);
        }

        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            string message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(span, message);
        }

        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, TypeSymbol operandType)
        {
            string message = $"Unary operator '{operatorText}' is not defined for type '{operandType}'.";
            Report(span, message);
        }

        public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            string message = $"Binary operator '{operatorText}' is not defined for types '{leftType}' and '{rightType}'.";
            Report(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            string message = $"Variable '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
        {
            string message = $"Cannot convert type '{fromType}' to '{toType}'.";
            Report(span, message);
        }

        public void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            string message = $"Variable '{name}' is already declared.";
            Report(span, message);
        }

        public void ReportCannotAssign(TextSpan span, string name)
        {
            string message = $"Variable '{name}' is read-only and cannot be assigned to.";
            Report(span, message);
        }
        public void ReportUndefinedFunction(TextSpan span, string name)
        {
            string message = $"Function '{name}' doesn't exist.";
            Report(span, message);
        }

        public void ReportWrongArgumentCount(TextSpan span, string name, int expectedCount, int actualCount)
        {
            string message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
            Report(span, message);
        }

        public void ReportWrongArgumentType(TextSpan span, string name, TypeSymbol expectedType, TypeSymbol actualType)
        {
            string message = $"Parameter '{name}' requires a value of type '{expectedType}' but was given a value of type '{actualType}'.";
            Report(span, message);
        }

        public void ReportExpressionMustHaveValue(TextSpan span)
        {
            string message = "Expression must have a value.";
            Report(span, message);
        }
    }
}