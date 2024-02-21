using Yearl.CodeAnalysis.Text;

namespace Yearl.Language
{
    public sealed class Error(TextSpan span, string message)
    {
        public TextSpan Span { get; } = span;
        public string Message { get; } = message;

        public override string ToString() => Message;
    }

}