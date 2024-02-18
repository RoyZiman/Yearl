using Yearl.Language.Syntax;

namespace Yearl.Language.Syntax
{
    public sealed class SyntaxToken(SyntaxKind kind, string text, object? value, int position) : SyntaxNode
    {

        public override SyntaxKind Kind { get; } = kind;
        public string Text { get; } = text;
        public object? Value { get; } = value;
        public int Position { get; } = position;
        public int Length { get; } = text.Length;
        public TextSpan Span => new TextSpan(Position, Text.Length);

        public bool MatchesKind(SyntaxKind kind)
        {
            return Kind == kind;
        }
        public bool MatchesValue(SyntaxKind kind, object? value)
        {
            return Kind == kind && Value == value;
        }
        public override string ToString()
        {
            if (Text == null)
                return Kind.ToString();
            else
                return $"{Kind}:{Text}";
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    }
}
