using System.Collections.Immutable;
using Yearl.CodeAnalysis.Syntax;
using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis
{
    internal sealed class Parser
    {
        private ErrorHandler _errors = new();
        private readonly SourceText _text;
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private int _position = 0;

        public ErrorHandler Errors => _errors;

        public Parser(SourceText text)
        {
            List<SyntaxToken> tokens = new();

            Lexer lexer = new(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();

                if (token.Kind is not SyntaxKind.WhitespaceToken and
                    not SyntaxKind.InvalidToken)
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
            return CurrentToken.Kind switch
            {
                SyntaxKind.LeftCurlyBraceToken => ParseBlockStatement(),
                SyntaxKind.VarKeyword or SyntaxKind.ConstKeyword => ParseVariableDeclerationStatement(),
                SyntaxKind.IfKeyword => ParseIfStatement(),
                _ => ParseExpressionStatement(),
            };
        }

        private SyntaxStatementBlock ParseBlockStatement()
        {
            ImmutableArray<SyntaxStatement>.Builder statements = ImmutableArray.CreateBuilder<SyntaxStatement>();

            SyntaxToken openBraceToken = MatchToken(SyntaxKind.LeftCurlyBraceToken);

            while (CurrentToken.Kind is not SyntaxKind.EndOfFileToken and
                   not SyntaxKind.RightCurlyBraceToken)
            {
                SyntaxToken startToken = CurrentToken;

                SyntaxStatement statement = ParseStatement();
                statements.Add(statement);

                if (CurrentToken == startToken)
                    NextToken();
            }

            SyntaxToken closeBraceToken = MatchToken(SyntaxKind.RightCurlyBraceToken);

            return new SyntaxStatementBlock(openBraceToken, statements.ToImmutable(), closeBraceToken);
        }

        private SyntaxStatementVariableDecleration ParseVariableDeclerationStatement()
        {
            SyntaxKind expected = CurrentToken.Kind == SyntaxKind.ConstKeyword ? SyntaxKind.ConstKeyword : SyntaxKind.VarKeyword;
            SyntaxToken keyword = MatchToken(expected);
            SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
            SyntaxToken equals = MatchToken(SyntaxKind.EqualsToken);
            SyntaxExpression initializer = ParseExpression();
            return new SyntaxStatementVariableDecleration(keyword, identifier, equals, initializer);
        }

        private SyntaxStatementIf ParseIfStatement()
        {
            SyntaxToken keyword = MatchToken(SyntaxKind.IfKeyword);
            SyntaxExpression condition = ParseExpression();
            SyntaxStatement statement = ParseStatement();
            SyntaxStatementElseClause? elseClause = ParseElseClause();
            return new SyntaxStatementIf(keyword, condition, statement, elseClause);
        }

        private SyntaxStatementElseClause? ParseElseClause()
        {
            if (CurrentToken.Kind != SyntaxKind.ElseKeyword)
                return null;

            SyntaxToken keyword = NextToken();
            SyntaxStatement statement = ParseStatement();
            return new SyntaxStatementElseClause(keyword, statement);
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
            return CurrentToken.Kind switch
            {
                SyntaxKind.LeftParenthesisToken => ParseParenthesizedExpression(),
                SyntaxKind.FalseKeyword or SyntaxKind.TrueKeyword => ParseBooleanLiteral(),
                SyntaxKind.NumberToken => ParseNumberLiteral(),
                _ => ParseNameExpression(),
            };
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