using System.Collections.Immutable;

namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxUnitCompilation(ImmutableArray<SyntaxMember> members, SyntaxToken endOfFileToken) : SyntaxNode
    {
        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public ImmutableArray<SyntaxMember> Members { get; } = members;
        public SyntaxToken EndOfFileToken { get; } = endOfFileToken;
    }
}