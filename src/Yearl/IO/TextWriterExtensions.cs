﻿using System.CodeDom.Compiler;
using System.Diagnostics;
using Yearl.CodeAnalysis.Errors;
using Yearl.CodeAnalysis.Syntax;
using Yearl.CodeAnalysis.Text;

namespace Yearl.IO
{
    public static class TextWriterExtensions
    {
        private static bool IsConsole(this TextWriter writer)
        {
            if (writer == Console.Out)
                return !Console.IsOutputRedirected;

            if (writer == Console.Error)
                return !Console.IsErrorRedirected && !Console.IsOutputRedirected; // Color codes are always output to Console.Out

            if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole())
                return true;

            return false;
        }

        private static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsole())
                Console.ForegroundColor = color;
        }

        private static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsole())
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
            string? text = SyntaxFacts.GetText(kind);
            Debug.Assert(kind.IsKeyword() && text != null);

            writer.WriteKeyword(text);
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
            string? text = SyntaxFacts.GetText(kind);
            Debug.Assert(text != null);

            writer.WritePunctuation(text);
        }

        public static void WriteSpace(this TextWriter writer) => writer.WritePunctuation(" ");

        public static void WriteErrors(this TextWriter writer, IEnumerable<Error> errors)
        {
            foreach (var error in errors.Where(d => d.Location.Text != null)
                                        .OrderBy(d => d.Location.FileName)
                                        .ThenBy(d => d.Location.Span.Start)
                                        .ThenBy(d => d.Location.Span.Length))
            {
                var text = error.Location.Text;
                string fileName = error.Location.FileName;
                int startLine = error.Location.StartLine + 1;
                int startCharacter = error.Location.StartCharacter + 1;
                int endLine = error.Location.EndLine + 1;
                int endCharacter = error.Location.EndCharacter + 1;

                var span = error.Location.Span;
                int lineIndex = text.GetLineIndex(span.Start);
                var line = text.Lines[lineIndex];

                writer.WriteLine();

                writer.SetForeground(ConsoleColor.DarkRed);
                writer.Write($"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): ");
                writer.WriteLine(error);
                writer.ResetColor();

                var prefixSpan = TextSpan.FromBounds(line.Start, span.Start);
                var suffixSpan = TextSpan.FromBounds(span.End, line.End);

                string prefix = text.ToString(prefixSpan);
                string fullError = text.ToString(span);
                string suffix = text.ToString(suffixSpan);

                writer.Write("    ");
                writer.Write(prefix);

                writer.SetForeground(ConsoleColor.DarkRed);
                writer.Write(fullError);
                writer.ResetColor();

                writer.Write(suffix);

                writer.WriteLine();
            }

            writer.WriteLine();
        }
    }
}