using System.Collections.Immutable;

namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxUnitCompilation(SyntaxTree syntaxTree, ImmutableArray<SyntaxMember> members, SyntaxToken endOfFileToken) 
        : SyntaxNode(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public ImmutableArray<SyntaxMember> Members { get; } = members;
        public SyntaxToken EndOfFileToken { get; } = endOfFileToken;
    }
}