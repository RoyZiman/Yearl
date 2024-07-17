namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionVariableAssignment(SyntaxTree syntaxTree, SyntaxToken identifierToken, SyntaxToken assignmentToken, SyntaxExpression expression)
        : SyntaxExpression(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.VariableAssignmentExpression;
        public SyntaxToken IdentifierToken { get; } = identifierToken;
        public SyntaxToken AssignmentToken { get; } = assignmentToken;
        public SyntaxExpression Expression { get; } = expression;
    }
}