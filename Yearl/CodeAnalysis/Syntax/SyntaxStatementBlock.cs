using System.Collections.Immutable;

namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementBlock(SyntaxToken leftCurlyBraceToken, ImmutableArray<SyntaxStatement> statements, SyntaxToken rightCurlyBraceToken) : SyntaxStatement
    {
        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
        public SyntaxToken LeftCurlyBraceToken { get; } = leftCurlyBraceToken;
        public ImmutableArray<SyntaxStatement> Statements { get; } = statements;
        public SyntaxToken RightCurlyBraceToken { get; } = rightCurlyBraceToken;
    }
}