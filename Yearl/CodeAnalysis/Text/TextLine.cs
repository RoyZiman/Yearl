namespace Yearl.CodeAnalysis.Text
{
    public sealed record TextLine(SourceText Text, int Start, int Length, int LengthIncludingLineBreak)
    {
        public int End => Start + Length;

        public TextSpan Span => new TextSpan(Start, Length);
        public TextSpan SpanIncludingLineBreak => new TextSpan(Start, LengthIncludingLineBreak);
        public override string ToString() => Text.ToString(Span);
    }
}