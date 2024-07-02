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

        public static void WriteErrors(this TextWriter writer, IEnumerable<Error> errors)
        {
            foreach (Error? error in errors.OrderBy(e => e.Location.FileName)
                                          .ThenBy(e => e.Location.Span.Start)
                                          .ThenBy(e => e.Location.Span.Length))
            {
                var text = error.Location.Text;
                var fileName = error.Location.FileName;
                var startLine = error.Location.StartLine + 1;
                var startCharacter = error.Location.StartCharacter + 1;
                var endLine = error.Location.EndLine + 1;
                var endCharacter = error.Location.EndCharacter + 1;

                var span = error.Location.Span;
                var lineIndex = text.GetLineIndex(span.Start);
                var line = text.Lines[lineIndex];

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write($"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): ");
                Console.WriteLine(error);
                Console.ResetColor();

                TextSpan prefixSpan = TextSpan.FromBounds(line.Start, span.Start);
                TextSpan suffixSpan = TextSpan.FromBounds(span.End, line.End);

                string prefix = text.ToString(prefixSpan);
                string fullError = text.ToString(span);
                string suffix = text.ToString(suffixSpan);

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