namespace Yearl.CodeAnalysis.Text
{
    public struct TextLocation(SourceText text, TextSpan span)
    {
        public SourceText Text { get; } = text;
        public TextSpan Span { get; } = span;

        public string FileName => Text.FileName;
        public int StartLine => Text.GetLineIndex(Span.Start);
        public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;
        public int EndLine => Text.GetLineIndex(Span.End);
        public int EndCharacter => Span.End - Text.Lines[EndLine].Start;
    }
}