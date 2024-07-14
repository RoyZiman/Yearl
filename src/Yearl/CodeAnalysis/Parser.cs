using System.Collections.Immutable;
using Yearl.CodeAnalysis.Errors;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis
{
    internal sealed class Parser
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private int _position = 0;

        public ErrorHandler Errors { get; } = new();

        public Parser(SyntaxTree syntaxTree)
        {
            List<SyntaxToken> tokens = [];

            Lexer lexer = new(syntaxTree);
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

            _syntaxTree = syntaxTree;
            _tokens = tokens.ToImmutableArray();
            Errors.AddRange(lexer.Errors);
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
            var currentToken = CurrentToken;
            _position++;
            return currentToken;
        }

        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (CurrentToken.Kind == kind)
                return NextToken();
            Errors.ReportUnexpectedToken(CurrentToken.Location, CurrentToken.Kind, kind);
            return new SyntaxToken(_syntaxTree, kind, null, null, CurrentToken.Position);
        }



        public SyntaxUnitCompilation ParseCompilationUnit()
        {
            var members = ParseMembers();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new SyntaxUnitCompilation(_syntaxTree, members, endOfFileToken);
        }

        private ImmutableArray<SyntaxMember> ParseMembers()
        {
            var members = ImmutableArray.CreateBuilder<SyntaxMember>();

            while (CurrentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                var startToken = CurrentToken;

                var member = ParseMember();
                members.Add(member);

                if (CurrentToken == startToken)
                    NextToken();
            }

            return members.ToImmutable();
        }

        private SyntaxMember ParseMember()
        {
            if (CurrentToken.Kind == SyntaxKind.FuncKeyword)
                return ParseFunctionDeclaration();

            return ParseGlobalStatement();
        }

        private SyntaxMember ParseFunctionDeclaration()
        {
            var functionKeyword = MatchToken(SyntaxKind.FuncKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenthesisToken = MatchToken(SyntaxKind.LeftParenthesisToken);
            var parameters = ParseParameterList();
            var closeParenthesisToken = MatchToken(SyntaxKind.RightParenthesisToken);
            var type = ParseOptionalTypeClause();
            var body = ParseBlockStatement();
            return new SyntaxStatementFunctionDeclaration(_syntaxTree, functionKeyword, identifier, openParenthesisToken, parameters, closeParenthesisToken, type, body);
        }

        private SeparatedSyntaxList<SyntaxParameter> ParseParameterList()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

            bool parseNextParameter = true;
            while (parseNextParameter &&
                   CurrentToken.Kind != SyntaxKind.RightParenthesisToken &&
                   CurrentToken.Kind != SyntaxKind.EndOfFileToken)
            {
                var parameter = ParseParameter();
                nodesAndSeparators.Add(parameter);

                if (CurrentToken.Kind == SyntaxKind.CommaToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else
                {
                    parseNextParameter = false;
                }
            }

            return new SeparatedSyntaxList<SyntaxParameter>(nodesAndSeparators.ToImmutable());
        }

        private SyntaxParameter ParseParameter()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var type = ParseTypeClause();
            return new SyntaxParameter(_syntaxTree, identifier, type);
        }

        private SyntaxMember ParseGlobalStatement()
        {
            var statement = ParseStatement();
            return new SyntaxStatementGlobal(_syntaxTree, statement);
        }

        private SyntaxStatement ParseStatement()
        {
            return CurrentToken.Kind switch
            {
                SyntaxKind.LeftCurlyBraceToken => ParseBlockStatement(),
                SyntaxKind.VarKeyword or SyntaxKind.ConstKeyword => ParseVariableDeclarationStatement(),
                SyntaxKind.IfKeyword => ParseIfStatement(),
                SyntaxKind.ForKeyword => ParseForStatement(),
                SyntaxKind.WhileKeyword => ParseWhileStatement(),
                SyntaxKind.BreakKeyword => ParseBreakStatement(),
                SyntaxKind.ContinueKeyword => ParseContinueStatement(),
                SyntaxKind.ReturnKeyword => ParseReturnStatement(),
                _ => ParseExpressionStatement(),
            };
        }


        private SyntaxStatementBlock ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<SyntaxStatement>();

            var openBraceToken = MatchToken(SyntaxKind.LeftCurlyBraceToken);

            while (CurrentToken.Kind is not SyntaxKind.EndOfFileToken and
                   not SyntaxKind.RightCurlyBraceToken)
            {
                var startToken = CurrentToken;

                var statement = ParseStatement();
                statements.Add(statement);

                if (CurrentToken == startToken)
                    NextToken();
            }

            var closeBraceToken = MatchToken(SyntaxKind.RightCurlyBraceToken);

            return new SyntaxStatementBlock(_syntaxTree, openBraceToken, statements.ToImmutable(), closeBraceToken);
        }

        private SyntaxStatementVariableDeclaration ParseVariableDeclarationStatement()
        {
            var expected = CurrentToken.Kind == SyntaxKind.ConstKeyword ? SyntaxKind.ConstKeyword : SyntaxKind.VarKeyword;
            var keyword = MatchToken(expected);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var typeClause = ParseOptionalTypeClause();
            var equals = MatchToken(SyntaxKind.EqualsToken);
            var initializer = ParseExpression();
            return new SyntaxStatementVariableDeclaration(_syntaxTree, keyword, identifier, typeClause, equals, initializer);
        }

        private SyntaxTypeClause ParseOptionalTypeClause()
        {
            if (CurrentToken.Kind != SyntaxKind.ColonToken)
                return null;

            return ParseTypeClause();
        }

        private SyntaxTypeClause ParseTypeClause()
        {
            var colonToken = MatchToken(SyntaxKind.ColonToken);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            return new SyntaxTypeClause(_syntaxTree, colonToken, identifier);
        }

        private SyntaxStatementIf ParseIfStatement()
        {
            var keyword = MatchToken(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            var bodyStatement = ParseStatement();
            var elseClause = ParseElseClause();
            return new SyntaxStatementIf(_syntaxTree, keyword, condition, bodyStatement, elseClause);
        }

        private SyntaxStatementElseClause ParseElseClause()
        {
            if (CurrentToken.Kind != SyntaxKind.ElseKeyword)
                return null;

            var keyword = NextToken();
            var statement = ParseStatement();
            return new SyntaxStatementElseClause(_syntaxTree, keyword, statement);
        }

        private SyntaxStatementFor ParseForStatement()
        {
            var forKeyword = MatchToken(SyntaxKind.ForKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var fromKeyword = MatchToken(SyntaxKind.FromKeyword);
            var bound1 = ParseExpression();
            var toKeyword = MatchToken(SyntaxKind.ToKeyword);
            var bound2 = ParseExpression();
            SyntaxToken stepKeyword = null;
            SyntaxExpression stepExpression = null;
            if (CurrentToken.Kind == SyntaxKind.StepKeyword)
            {
                stepKeyword = MatchToken(SyntaxKind.StepKeyword);
                stepExpression = ParseExpression();
            }
            var statement = ParseStatement();
            return new SyntaxStatementFor(_syntaxTree, forKeyword, identifier, fromKeyword, bound1, toKeyword, bound2, stepKeyword, stepExpression, statement);
        }

        private SyntaxStatementWhile ParseWhileStatement()
        {
            var keyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var statementStatement = ParseStatement();
            return new SyntaxStatementWhile(_syntaxTree, keyword, condition, statementStatement);
        }

        private SyntaxStatementBreak ParseBreakStatement()
        {
            var keyword = MatchToken(SyntaxKind.BreakKeyword);
            return new SyntaxStatementBreak(_syntaxTree, keyword);
        }

        private SyntaxStatementContinue ParseContinueStatement()
        {
            var keyword = MatchToken(SyntaxKind.ContinueKeyword);
            return new SyntaxStatementContinue(_syntaxTree, keyword);
        }

        private SyntaxStatementReturn ParseReturnStatement()
        {
            var keyword = MatchToken(SyntaxKind.ReturnKeyword);
            var openParenthesisToken = MatchToken(SyntaxKind.LeftParenthesisToken);
            bool hasExpression = CurrentToken.Kind != SyntaxKind.RightParenthesisToken;

            var expression = hasExpression ? ParseExpression() : null;

            var closeParenthesisToken = MatchToken(SyntaxKind.RightParenthesisToken);
            return new SyntaxStatementReturn(_syntaxTree, keyword, openParenthesisToken, expression, closeParenthesisToken);
        }

        private SyntaxStatementExpression ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new SyntaxStatementExpression(_syntaxTree, expression);
        }

        private SyntaxExpression ParseExpression() => ParseVariableAssignmentExpression();

        private SyntaxExpression ParseVariableAssignmentExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken &&
                Peek(1).Kind == SyntaxKind.EqualsToken)
            {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseVariableAssignmentExpression();
                return new SyntaxExpressionVariableAssignment(_syntaxTree, identifierToken, operatorToken, right);
            }
            return ParseBinaryExpression();
        }

        private SyntaxExpression ParseBinaryExpression(int parentPrecedence = 0)
        {
            SyntaxExpression left;
            int unaryOperatorPrecedence = CurrentToken.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var expression = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new SyntaxExpressionUnary(_syntaxTree, operatorToken, expression);
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

                var operatorToken = NextToken();
                var right = ParseBinaryExpression(precedence);
                left = new SyntaxExpressionBinary(_syntaxTree, left, operatorToken, right);
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
                SyntaxKind.StringToken => ParseStringLiteral(),
                _ => ParseNameOrCallExpression(),
            };
        }

        private SyntaxExpressionParenthesized ParseParenthesizedExpression()
        {
            var leftParenthesis = NextToken();
            var expression = ParseExpression();
            var rightParenthesis = MatchToken(SyntaxKind.RightParenthesisToken);

            return new SyntaxExpressionParenthesized(_syntaxTree, leftParenthesis, expression, rightParenthesis);
        }

        private SyntaxExpressionLiteral ParseBooleanLiteral()
        {
            var keywordToken = NextToken();
            bool value = keywordToken.Kind == SyntaxKind.TrueKeyword;
            return new SyntaxExpressionLiteral(_syntaxTree, keywordToken, value);
        }

        private SyntaxExpressionLiteral ParseNumberLiteral()
        {
            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new SyntaxExpressionLiteral(_syntaxTree, numberToken);
        }

        private SyntaxExpressionLiteral ParseStringLiteral()
        {
            var stringToken = MatchToken(SyntaxKind.StringToken);
            return new SyntaxExpressionLiteral(_syntaxTree, stringToken);
        }

        private SyntaxExpression ParseNameOrCallExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.LeftParenthesisToken)
                return ParseCallExpression();

            return ParseNameExpression();
        }

        private SyntaxExpressionCall ParseCallExpression()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenthesisToken = MatchToken(SyntaxKind.LeftParenthesisToken);
            var arguments = ParseArguments();
            var closeParenthesisToken = MatchToken(SyntaxKind.RightParenthesisToken);
            return new SyntaxExpressionCall(_syntaxTree, identifier, openParenthesisToken, arguments, closeParenthesisToken);
        }

        private SeparatedSyntaxList<SyntaxExpression> ParseArguments()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

            bool parseNextArgument = true;
            while (parseNextArgument &&
                   CurrentToken.Kind is not SyntaxKind.RightParenthesisToken and
                   not SyntaxKind.EndOfFileToken)
            {
                var expression = ParseExpression();
                nodesAndSeparators.Add(expression);

                if (CurrentToken.Kind == SyntaxKind.CommaToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else
                {
                    parseNextArgument = false;
                }
            }

            return new SeparatedSyntaxList<SyntaxExpression>(nodesAndSeparators.ToImmutable());
        }

        private SyntaxExpressionName ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new SyntaxExpressionName(_syntaxTree, identifierToken);
        }
    }
}