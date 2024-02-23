namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionVariableAssignment(SyntaxToken identifierToken, SyntaxToken equalsToken, SyntaxExpression expression) : SyntaxExpression
    {
        public override SyntaxKind Kind => SyntaxKind.VariableAssignmentExpression;
        public SyntaxToken IdentifierToken { get; } = identifierToken;
        public SyntaxToken EqualsToken { get; } = equalsToken;
        public SyntaxExpression Expression { get; } = expression;
    }

    public sealed class SyntaxStatementVariableDecleration : SyntaxStatement
    {
        public SyntaxStatementVariableDecleration(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken equalsToken, SyntaxExpression initializer)
        {
            Keyword = keyword;
            Identifier = identifier;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public SyntaxExpression Initializer { get; }
    }
}