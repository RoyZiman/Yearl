using System.Collections.Immutable;
using Yearl.CodeAnalysis.Text;
using Yearl.Language.Syntax;

namespace Yearl.Language
{
    internal sealed class Parser
    {
        private ErrorHandler _errors = new ErrorHandler();
        private readonly SourceText _text;
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private int _position = 0;

        public ErrorHandler Errors => _errors;

        public Parser(SourceText text)
        {
            List<SyntaxToken> tokens = new List<SyntaxToken>();

            Lexer lexer = new Lexer(text);
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

            _text = text;
            _tokens = tokens.ToImmutableArray();
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



        public SyntaxUnitCompilation ParseCompilationUnit()
        {
            SyntaxStatement statement = ParseStatement();
            SyntaxToken endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new SyntaxUnitCompilation(statement, endOfFileToken);
        }

        private SyntaxStatement ParseStatement()
        {
            switch (CurrentToken.Kind)
            {
                case SyntaxKind.LeftCurlyBraceToken:
                    return ParseBlockStatement();
                case SyntaxKind.VarKeyword:
                case SyntaxKind.ConstKeyword:
                    return ParseVariableDeclerationStatement();
                default:
                    return ParseExpressionStatement();
            }
        }

        private SyntaxStatement ParseVariableDeclerationStatement()
        {
            SyntaxKind expected = CurrentToken.Kind == SyntaxKind.ConstKeyword ? SyntaxKind.ConstKeyword : SyntaxKind.VarKeyword;
            SyntaxToken keyword = MatchToken(expected);
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken equals = MatchToken(SyntaxKind.EqualsToken);
            SyntaxExpression initializer = ParseExpression();
            return new SyntaxStatementVariableDecleration(keyword, identifier, equals, initializer);
        }

        private BlockStatementSyntax ParseBlockStatement()
        {
            ImmutableArray<SyntaxStatement>.Builder statements = ImmutableArray.CreateBuilder<SyntaxStatement>();

            SyntaxToken openBraceToken = MatchToken(SyntaxKind.LeftCurlyBraceToken);

            while (CurrentToken.Kind != SyntaxKind.EndOfFileToken &&
                   CurrentToken.Kind != SyntaxKind.RightCurlyBraceToken)
            {
                SyntaxStatement statement = ParseStatement();
                statements.Add(statement);
            }

            SyntaxToken closeBraceToken = MatchToken(SyntaxKind.RightCurlyBraceToken);

            return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);
        }

        private SyntaxStatementExpression ParseExpressionStatement()
        {
            SyntaxExpression expression = ParseExpression();
            return new SyntaxStatementExpression(expression);
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
                    return ParseParenthesizedExpression();

                case SyntaxKind.FalseKeyword:
                case SyntaxKind.TrueKeyword:
                    return ParseBooleanLiteral();


                case SyntaxKind.NumberToken:
                    return ParseNumberLiteral();

                case SyntaxKind.IdentifierToken:
                default:
                    return ParseNameExpression();
            }
        }

        private SyntaxExpressionParenthesized ParseParenthesizedExpression()
        {
            SyntaxToken leftParenthesis = NextToken();
            SyntaxExpression expression = ParseExpression();
            SyntaxToken rightParenthesis = MatchToken(SyntaxKind.RightParenthesisToken);

            return new SyntaxExpressionParenthesized(leftParenthesis, expression, rightParenthesis);
        }

        private SyntaxExpressionLiteral ParseBooleanLiteral()
        {
            SyntaxToken keywordToken = NextToken();
            bool value = keywordToken.Kind == SyntaxKind.TrueKeyword;
            return new SyntaxExpressionLiteral(keywordToken, value);
        }

        private SyntaxExpressionLiteral ParseNumberLiteral()
        {
            SyntaxToken numberToken = MatchToken(SyntaxKind.NumberToken);
            return new SyntaxExpressionLiteral(numberToken);
        }

        private SyntaxExpressionName ParseNameExpression()
        {
            SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new SyntaxExpressionName(identifierToken);
        }


    }
}
