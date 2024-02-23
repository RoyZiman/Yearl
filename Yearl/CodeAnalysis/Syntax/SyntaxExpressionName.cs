using System.Collections.Immutable;

namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionName(SyntaxToken identifierToken) : SyntaxExpression
    {
        public override SyntaxKind Kind => SyntaxKind.NameExpression;
        public SyntaxToken IdentifierToken { get; } = identifierToken;
    }
    public sealed class BlockStatementSyntax : SyntaxStatement
    {
        public BlockStatementSyntax(SyntaxToken leftCurlyBraceToken, ImmutableArray<SyntaxStatement> statements, SyntaxToken rightCurlyBraceToken)
        {
            LeftCurlyBraceToken = leftCurlyBraceToken;
            Statements = statements;
            RightCurlyBraceToken = rightCurlyBraceToken;
        }

        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
        public SyntaxToken LeftCurlyBraceToken { get; }
        public ImmutableArray<SyntaxStatement> Statements { get; }
        public SyntaxToken RightCurlyBraceToken { get; }
    }
}
