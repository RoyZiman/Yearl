using System.Collections;
using Yearl.Language.Syntax;

namespace Yearl.Language
{
    internal sealed class ErrorHandler : IEnumerable<Error>
    {
        private readonly List<Error> _errors = new List<Error>();
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


        public void ReportInvalidNumber(TextSpan span, string text, Type type)
        {
            string message = $"The number {text} isn't valid {type}.";
            Report(span, message);
        }

        public void ReportInvalidCharacter(int position, char character)
        {
            TextSpan span = new TextSpan(position, 1);
            string message = $"Invalid character input: '{character}'.";
            Report(span, message);
        }

        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            string message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
            Report(span, message);
        }

        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, Type operandType)
        {
            string message = $"Unary operator '{operatorText}' is not defined for type {operandType}.";
            Report(span, message);
        }

        public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, Type leftType, Type rightType)
        {
            string message = $"Binary operator '{operatorText}' is not defined for types {leftType} and {rightType}.";
            Report(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            string message = $"Variable '{name}' doesn't exist.";
            Report(span, message);
        }

    }
}