namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementVariableDeclaration(SyntaxTree syntaxTree, SyntaxToken keyword, SyntaxToken identifier, SyntaxTypeClause typeClause, SyntaxToken equalsToken, SyntaxExpression initializer)
        : SyntaxStatement(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;
        public SyntaxToken Keyword { get; } = keyword;
        public SyntaxToken Identifier { get; } = identifier;
        public SyntaxTypeClause TypeClause { get; } = typeClause;
        public SyntaxToken EqualsToken { get; } = equalsToken;
        public SyntaxExpression Initializer { get; } = initializer;
    }
}