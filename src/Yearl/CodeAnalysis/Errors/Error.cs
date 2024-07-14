using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis.Errors
{
    public sealed class Error(TextLocation location, string message)
    {
        public TextLocation Location { get; } = location;
        public string Message { get; } = message;

        public override string ToString() => Message;
    }
}