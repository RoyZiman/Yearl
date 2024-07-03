using System.Collections.Immutable;
using System.Text;
using Yearl.CodeAnalysis.Text;

namespace Yearl.Tests.CodeAnalysis.Text
{
    internal sealed class AnnotatedText(string text, ImmutableArray<TextSpan> spans)
    {
        public string Text { get; } = text;
        public ImmutableArray<TextSpan> Spans { get; } = spans;

        public static AnnotatedText Parse(string text)
        {
            text = Unindent(text);

            StringBuilder textBuilder = new();
            var spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();
            Stack<int> startStack = new();

            int position = 0;

            foreach (char c in text)
            {
                if (c == '[')
                {
                    startStack.Push(position);
                }
                else if (c == ']')
                {
                    if (startStack.Count == 0)
                        throw new ArgumentException("Too many ']' in text", nameof(text));

                    int start = startStack.Pop();
                    int end = position;
                    var span = TextSpan.FromBounds(start, end);
                    spanBuilder.Add(span);
                }
                else
                {
                    position++;
                    textBuilder.Append(c);
                }
            }

            if (startStack.Count != 0)
                throw new ArgumentException("Missing ']' in text", nameof(text));

            return new AnnotatedText(textBuilder.ToString(), spanBuilder.ToImmutable());
        }

        private static string Unindent(string text)
        {
            string[] lines = UnindentLines(text);
            return string.Join(Environment.NewLine, lines);
        }

        public static string[] UnindentLines(string text)
        {
            List<string> lines = [];

            using (StringReader reader = new(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    lines.Add(line);
            }

            int minIndentation = int.MaxValue;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if (line.Trim().Length == 0)
                {
                    lines[i] = string.Empty;
                    continue;
                }

                int indentation = line.Length - line.TrimStart().Length;
                minIndentation = Math.Min(minIndentation, indentation);
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length == 0)
                    continue;

                lines[i] = lines[i][minIndentation..];
            }

            while (lines.Count > 0 && lines[0].Length == 0)
                lines.RemoveAt(0);

            while (lines.Count > 0 && lines[^1].Length == 0)
                lines.RemoveAt(lines.Count - 1);

            return lines.ToArray();
        }
    }
}