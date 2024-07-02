namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxExpressionBinary(SyntaxTree syntaxTree, SyntaxExpression left, SyntaxToken operatorToken, SyntaxExpression right) 
        : SyntaxExpression(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
        public SyntaxExpression Left { get; } = left;
        public SyntaxToken OperatorToken { get; } = operatorToken;
        public SyntaxExpression Right { get; } = right;
    }
}