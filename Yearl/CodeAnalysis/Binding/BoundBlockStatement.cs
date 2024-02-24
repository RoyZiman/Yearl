using System.Collections.Immutable;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundBlockStatement(ImmutableArray<BoundStatement> statements) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
        public ImmutableArray<BoundStatement> Statements { get; } = statements;
    }
}