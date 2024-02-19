namespace Yearl.Language
{
    public sealed class Error(TextSpan span, string message)
    {
        public TextSpan Span { get; } = span;
        public string Message { get; } = message;

        public override string ToString() => Message;
    }

    public struct TextSpan(int start, int length)
    {
        public int Start { get; } = start;
        public int Length { get; } = length;
        public int End => Start + Length;
    }
}