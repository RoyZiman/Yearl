namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionVariableAssignment(SyntaxTree syntaxTree, SyntaxToken identifierToken, SyntaxToken equalsToken, SyntaxExpression expression)
        : SyntaxExpression(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.VariableAssignmentExpression;
        public SyntaxToken IdentifierToken { get; } = identifierToken;
        public SyntaxToken EqualsToken { get; } = equalsToken;
        public SyntaxExpression Expression { get; } = expression;
    }
}