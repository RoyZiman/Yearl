namespace Yearl.CodeAnalysis.Syntax
{
    public sealed class SyntaxStatementFunctionDeclaration(SyntaxTree syntaxTree,
                                                           SyntaxToken functionKeyword,
                                                           SyntaxToken identifier,
                                                           SyntaxToken openParenthesisToken,
                                                           SeparatedSyntaxList<SyntaxParameter> parameters,
                                                           SyntaxToken closeParenthesisToken,
                                                           SyntaxTypeClause? type,
                                                           SyntaxStatementBlock body)
        : SyntaxMember(syntaxTree)
    {
        public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;

        public SyntaxToken FunctionKeyword { get; } = functionKeyword;
        public SyntaxToken Identifier { get; } = identifier;
        public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
        public SeparatedSyntaxList<SyntaxParameter> Parameters { get; } = parameters;
        public SyntaxToken CloseParenthesisToken { get; } = closeParenthesisToken;
        public SyntaxTypeClause? Type { get; } = type;
        public SyntaxStatementBlock Body { get; } = body;
    }
}