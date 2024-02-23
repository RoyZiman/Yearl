using Yearl.CodeAnalysis.Syntax;

namespace Yearl.Tests.CodeAnalysis.Syntax
{
    internal sealed class AssertingEnumerator(SyntaxNode node) : IDisposable
    {
        private readonly IEnumerator<SyntaxNode> _enumerator = Flatten(node).GetEnumerator();
        private bool _hasErrors;

        private bool MarkFailed()
        {
            _hasErrors = true;
            return false;
        }

        public void Dispose()
        {
            if (!_hasErrors)
                Assert.False(_enumerator.MoveNext());

            _enumerator.Dispose();
        }

        private static IEnumerable<SyntaxNode> Flatten(SyntaxNode node)
        {
            Stack<SyntaxNode> stack = new Stack<SyntaxNode>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                SyntaxNode n = stack.Pop();
                yield return n;

                foreach (SyntaxNode? child in n.GetChildren().Reverse())
                    stack.Push(child);
            }
        }

        public void AssertNode(SyntaxKind kind)
        {
            try
            {
                Assert.True(_enumerator.MoveNext());
                Assert.Equal(kind, _enumerator.Current.Kind);
                Assert.IsNotType<SyntaxToken>(_enumerator.Current);
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }

        public void AssertToken(SyntaxKind kind, string text)
        {
            try
            {
                Assert.True(_enumerator.MoveNext());
                Assert.Equal(kind, _enumerator.Current.Kind);
                SyntaxToken token = Assert.IsType<SyntaxToken>(_enumerator.Current);
                Assert.Equal(text, token.Text);
            }
            catch when (MarkFailed())
            {
                throw;
            }
        }
    }
}