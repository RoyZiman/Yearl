namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementVariableDeclaration(SyntaxToken keyword, SyntaxToken identifier, SyntaxTypeClause typeClause, SyntaxToken equalsToken, SyntaxExpression initializer) : SyntaxStatement
    {
        public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;
        public SyntaxToken Keyword { get; } = keyword;
        public SyntaxToken Identifier { get; } = identifier;
        public SyntaxTypeClause TypeClause { get; } = typeClause;
        public SyntaxToken EqualsToken { get; } = equalsToken;
        public SyntaxExpression Initializer { get; } = initializer;
    }
}