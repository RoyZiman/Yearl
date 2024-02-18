using Yearl.Language.Syntax;

namespace Yearl.Language
{
    internal sealed class Parser
    {
        private readonly string _path;
        private readonly string _code;
        private readonly SyntaxToken[] _tokens;
        private int _position = 0;
        private ErrorHandler _errors = new ErrorHandler();

        public ErrorHandler Errors => _errors;

        public Parser(string path, string code)
        {
            _path = path;
            _code = code;

            List<SyntaxToken> tokens = new List<SyntaxToken>();

            Lexer lexer = new Lexer(path, code);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();

                if (token.Kind != SyntaxKind.WhitespaceToken &&
                    token.Kind != SyntaxKind.InvalidToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _tokens = tokens.ToArray();
            _errors.AddRange(lexer.Errors);
        }

        private SyntaxToken CurrentToken => Peek(0);

        private SyntaxToken Peek(int offset)
        {
            int index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[^1];

            return _tokens[index];
        }
        private SyntaxToken NextToken()
        {
            SyntaxToken currentToken = CurrentToken;
            _position++;
            return currentToken;
        }

        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (CurrentToken.Kind == kind)
                return NextToken();

            _errors.ReportUnexpectedToken(CurrentToken.Span, CurrentToken.Kind, kind);
            return new SyntaxToken(kind, "", null, CurrentToken.Position);
           }



        public SyntaxTree Parse()
        {
            SyntaxNode Node = ParseNode();
            SyntaxToken endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new SyntaxTree(Errors, Node, endOfFileToken);
        }
        private SyntaxNode ParseNode()
        {
            return ParseExpression();
        }


        private SyntaxExpression ParseExpression()
        {
            return ParseVariableAssignmentExpression();
        }

        private SyntaxExpression ParseVariableAssignmentExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken &&
                Peek(1).Kind == SyntaxKind.EqualsToken)
            {
                SyntaxToken identifierToken = NextToken();
                SyntaxToken operatorToken = NextToken();
                SyntaxExpression right = ParseVariableAssignmentExpression();
                return new SyntaxExpressionVariableAssignment(identifierToken, operatorToken, right);
            }
        return ParseBinaryExpression();
        }

        private SyntaxExpression ParseBinaryExpression(int parentPrecedence = 0)
        {
            SyntaxExpression left;
            int unaryOperatorPrecedence = CurrentToken.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                SyntaxToken operatorToken = NextToken();
                SyntaxExpression expression = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new SyntaxExpressionUnary(operatorToken, expression);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            while (true)
            {
                int precedence = CurrentToken.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                SyntaxToken operatorToken = NextToken();
                SyntaxExpression right = ParseBinaryExpression(precedence);
                left = new SyntaxExpressionBinary(left, operatorToken, right);
            }

            return left;
        }

        private SyntaxExpression ParsePrimaryExpression()
        {
            switch (CurrentToken.Kind)
            {
                case SyntaxKind.LeftParenthesisToken:
                    {
                        SyntaxToken leftParenthesis = NextToken();
                        SyntaxExpression expression = ParseExpression();
                        SyntaxToken rightParenthesis = MatchToken(SyntaxKind.RightParenthesisToken);

                        return new SyntaxExpressionParenthesized(leftParenthesis, expression, rightParenthesis);
                    }
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.TrueKeyword:
                    {
                        SyntaxToken keywordToken = NextToken();
                        bool value = keywordToken.Kind == SyntaxKind.TrueKeyword;
                        return new SyntaxExpressionLiteral(keywordToken, value);
                    }

                case SyntaxKind.NumberToken:
                    return new SyntaxExpressionLiteral(NextToken());

                case SyntaxKind.IdentifierToken:
                    {
                        SyntaxToken identifierToken = NextToken();
                        return new SyntaxExpressionName(identifierToken);
                    }


                default:
                    SyntaxToken numberToken = MatchToken(SyntaxKind.NumberToken);
                    return new SyntaxExpressionLiteral(numberToken);
            }
        }


    }
}
