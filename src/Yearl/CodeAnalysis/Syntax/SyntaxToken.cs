using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, string text, object value, int position) : SyntaxNode(syntaxTree)
    {
        public override SyntaxKind Kind { get; } = kind;
        public string Text { get; } = text;
        public object Value { get; } = value;
        public int Position { get; } = position;
        public int Length { get; } = text?.Length ?? 0;
        public override TextSpan Span => new(Position, Text?.Length ?? 0);


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
        public bool IsMissing => Text == null;
    }
}