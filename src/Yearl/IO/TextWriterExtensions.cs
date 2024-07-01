using System.CodeDom.Compiler;
using Yearl.CodeAnalysis;
using Yearl.CodeAnalysis.Syntax;
using Yearl.CodeAnalysis.Text;

namespace Yearl.IO
{
    public static class TextWriterExtensions
    {
        private static bool IsConsoleOut(this TextWriter writer)
        {
            if (writer == Console.Out)
                return true;

            if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsoleOut())
                return true;

            return false;
        }

        private static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsoleOut())
                Console.ForegroundColor = color;
        }

        private static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsoleOut())
                Console.ResetColor();
        }

        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Blue);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteKeyword(this TextWriter writer, SyntaxKind kind)
        {
            writer.WriteKeyword(SyntaxFacts.GetText(kind));
        }

        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.DarkYellow);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Cyan);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteString(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Magenta);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.DarkGray);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WritePunctuation(this TextWriter writer, SyntaxKind kind)
        {
            writer.WritePunctuation(SyntaxFacts.GetText(kind));
        }

        public static void WriteSpace(this TextWriter writer)
        {
            writer.WritePunctuation(" ");
        }

        public static void WriteErrors(this TextWriter writer, IEnumerable<Error> errors, SyntaxTree syntaxTree)
        {
            foreach (var err in errors.OrderBy(d => d.Span.Start)
                                      .ThenBy(d => d.Span.Length))
            {
                var lineIndex = syntaxTree.Text.GetLineIndex(err.Span.Start);
                var line = syntaxTree.Text.Lines[lineIndex];
                var lineNumber = lineIndex + 1;
                var character = err.Span.Start - line.Start + 1;

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write($"({lineNumber}, {character}): ");
                Console.WriteLine(err);
                Console.ResetColor();

                var prefixSpan = TextSpan.FromBounds(line.Start, err.Span.Start);
                var suffixSpan = TextSpan.FromBounds(err.Span.End, line.End);

                var prefix = syntaxTree.Text.ToString(prefixSpan);
                var error = syntaxTree.Text.ToString(err.Span);
                var suffix = syntaxTree.Text.ToString(suffixSpan);

                Console.Write("    ");
                Console.Write(prefix);

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write(error);
                Console.ResetColor();

                Console.Write(suffix);

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}