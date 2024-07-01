using Yearl.CodeAnalysis.Text;

namespace msi
{
    internal class TextSpanComparer : IComparer<TextSpan>
    {
        public int Compare(TextSpan x, TextSpan y)
        {
            int res = x.Start - y.Start;
            if (res == 0)
                res = x.Length - y.Length;
            return res;
        }
    }
}