namespace Yearl.Language.Syntax
{
    public sealed class SyntaxExpressionVariableAssignment(SyntaxToken identifierToken, SyntaxToken equalsToken, SyntaxExpression expression) : SyntaxExpression
    {
        public override SyntaxKind Kind => SyntaxKind.VariableAssignmentExpression;
        public SyntaxToken IdentifierToken { get; } = identifierToken;
        public SyntaxToken EqualsToken { get; } = equalsToken;
        public SyntaxExpression Expression { get; } = expression;
    }
}
