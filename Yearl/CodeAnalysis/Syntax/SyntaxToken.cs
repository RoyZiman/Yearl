using Yearl.CodeAnalysis.Text;

namespace Yearl.Language.Syntax
{
    public sealed class SyntaxToken(SyntaxKind kind, string text, object? value, int position) : SyntaxNode
    {

        public override SyntaxKind Kind { get; } = kind;
        public string Text { get; } = text;
        public object? Value { get; } = value;
        public int Position { get; } = position;
        public int Length { get; } = text.Length;
        public override TextSpan Span => new TextSpan(Position, Text?.Length ?? 0);

        public override string ToString()
        {
            if (Text == null)
                return Kind.ToString();
            else
                return $"{Kind}:{Text}";
        }
    }
}
