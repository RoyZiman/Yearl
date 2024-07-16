using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, string? text, object? value, int position) : SyntaxNode(syntaxTree)
    {
        public override SyntaxKind Kind { get; } = kind;
        public string Text { get; } = text ?? string.Empty;
        public object? Value { get; } = value;
        public int Position { get; } = position;
        public int Length { get; } = text?.Length ?? 0;
        public override TextSpan Span => new(Position, Length);


        public override string ToString()
        {
            if (Text == null)
                return Kind.ToString();
            else
                return $"{Kind}:{Text}";
        }

        /// <summary>
        /// A token is missing if it was inserted by the parser and doesn't
        /// appear in source.
        /// </summary>
        public bool IsMissing { get; } = text == null;
    }
}