/*

using Yearl.Language.Syntax;

namespace Yearl.Language
{
    public class Error(ErrorContext context, string errorType, string errorName, string details)
    {
        private ErrorContext Context { get; } = context;
        private string ErrorType { get; } = errorType;
        private string ErrorName { get; } = errorName;
        private string Details { get; } = details;

        public override string ToString()
        {
            return $"\n\n{ErrorType}:\n- {ErrorName}: {Details}\n{Context}\n";
        }
    }
    public class ErrorContext
    {
        public string Path { get; }
        public string Text { get; }
        public int Line { get; }
        public int ColumnStart { get; }
        public int Length { get; }


        internal ErrorContext(string path, string text, int position, int length)
        {
            string[] lines = text.Replace("\r", "").Split('\n');
            int overallCount = 0;

            int i = 0;
            while (i < lines.Length && overallCount <= position)
            {
                overallCount += lines[i].Length;
                i++;
            }

            Line = i - 1;
            ColumnStart = position - overallCount + lines[Line].Length;
            Length = length;
            Text = lines[Line];
            Path = path;
        }
        public override string ToString()
        {
            return $"From Path: {Path}\n\nLine: {Line + 1}, Column: {ColumnStart + 1} \n {Text}\n " + new string(' ', ColumnStart) + new string('^', Length);
        }
    }

    abstract class GrammerError(ErrorContext context, string errorName, string details) : Error(context, "Grammer Error", errorName, details);

    class IllegalCharError(string path, string text, int position, char character)
        : GrammerError(new ErrorContext(path, text, position, 1), "Illegal Character", $"Unknown character '{character}'.");
    class UnexpectedCharError(string path, string text, int position, string expected, char received)
        : GrammerError(new ErrorContext(path, text, position, 1), "Unexpected Character", $"Expected '{expected}', got '{received}'.");

    class InvalidNumberError(string path, string text, int position, string number)
        : GrammerError(new ErrorContext(path, text, position, number.Length), "Invalid Number", $"'{number}' isn't valid number of type Double.");



    abstract class SyntaxError(ErrorContext context, string errorName, string details) : Error(context, "Syntax Error", errorName, details);

    class UnexpectedTokenError(string path, string text, SyntaxKind expectedKind, SyntaxToken receivedToken)
        : SyntaxError(new ErrorContext(path, text, receivedToken.StartPos, receivedToken.Length), "Unexpected Token", $"Expected '{expectedKind}', got '{receivedToken}'.");



    abstract class TypeError(ErrorContext context, string errorName, string details) : Error(context, "Type Error", errorName, details);
}

 */