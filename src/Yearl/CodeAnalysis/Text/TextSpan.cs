namespace Yearl.CodeAnalysis.Text
{
    public struct TextSpan(int start, int length)
    {
        public int Start { get; } = start;
        public int Length { get; } = length;
        public readonly int End => Start + Length;

        public static TextSpan FromBounds(int start, int end)
        {
            int length = end - start;
            return new TextSpan(start, length);
        }

        public bool OverlapsWith(TextSpan span)
        {
            return Start < span.End &&
                   End > span.Start;
        }

        public override string ToString() => $"{Start}..{End}";
    }
}