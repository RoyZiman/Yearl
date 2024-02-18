using Yearl.Language.Syntax;

namespace Yearl.Language
{
    internal sealed class Lexer(string code)
    {
        private readonly string _code = code;
        private int _position = 0;
        private ErrorHandler _errors = new ErrorHandler();
        public ErrorHandler Errors => _errors;

        private char CurrentChar => Peek(0);
        private char Lookahead => Peek(1);

        private char Peek(int offset)
        {
            int index = _position + offset;

            if (index >= _code.Length)
                return '\0';

            return _code[index];
        }

        private void Next() => _position++;

        public SyntaxToken Lex()
        {
            if (_position >= _code.Length)
                return new SyntaxToken(SyntaxKind.EndOfFileToken, "\0", null, _position);


            if (char.IsWhiteSpace(CurrentChar))
            {
                int startPosition = _position;

                while (char.IsWhiteSpace(CurrentChar))
                    Next();

                int length = _position - startPosition;
                string text = _code.Substring(startPosition, length);

                return new SyntaxToken(SyntaxKind.WhitespaceToken, text, null, startPosition);
            }


            if (char.IsDigit(CurrentChar))
            {
                int startPosition = _position;

                while (char.IsDigit(CurrentChar) || CurrentChar == '.')
                    Next();

                int length = _position - startPosition;
                string text = _code.Substring(startPosition, length);

                if (double.TryParse(text, out double value))
                    return new SyntaxToken(SyntaxKind.NumberToken, text, value, startPosition);

                _errors.ReportInvalidNumber(new TextSpan(startPosition, length), _code, typeof(double));
            }

            if (char.IsLetter(CurrentChar))
            {
                int start = _position;

                while (char.IsLetter(CurrentChar))
                    Next();

                int length = _position - start;
                string text = _code.Substring(start, length);
                SyntaxKind kind = text.GetKeywordKind();
                return new SyntaxToken(kind, text, null, start);
            }

            switch (CurrentChar)
            {
                case '+':
                    return new SyntaxToken(SyntaxKind.PlusToken, "+", null, _position++);
                case '-':
                    return new SyntaxToken(SyntaxKind.MinusToken, "-", null, _position++);
                case '*':
                    return new SyntaxToken(SyntaxKind.StarToken, "*", null, _position++);
                case '/':
                    return new SyntaxToken(SyntaxKind.SlashToken, "/", null, _position++);
                case '^':
                    return new SyntaxToken(SyntaxKind.HatToken, "^", null, _position++);
                case '(':
                    return new SyntaxToken(SyntaxKind.LeftParenthesisToken, "(", null, _position++);
                case ')':
                    return new SyntaxToken(SyntaxKind.RightParenthesisToken, ")", null, _position++);
                case '&':
                    if (Lookahead == '&')
                        return new SyntaxToken(SyntaxKind.AndToken, "&&", null, (_position += 2) - 2);
                    break;
                case '|':
                    if (Lookahead == '|')
                        return new SyntaxToken(SyntaxKind.OrToken, "||", null, (_position += 2) - 2);
                    break;
                case '=':
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.DoubleEqualsToken, "==", null, (_position += 2) -2);
                    else
                        return new SyntaxToken(SyntaxKind.EqualsToken, "=", null, _position ++);
                case '!':
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.NotEqualsToken, "!=", null, (_position += 2) - 2);
                    else
                        return new SyntaxToken(SyntaxKind.NotToken, "!", null, _position++);
                case '>':
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.GreaterThanEqualsToken, ">=", null, (_position += 2) - 2);
                    else
                        return new SyntaxToken(SyntaxKind.GreaterThanToken, ">", null, _position++);
                case '<':
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.LessThanEqualsToken, "<=", null, (_position += 2) - 2);
                    else
                        return new SyntaxToken(SyntaxKind.LessThanToken, "<", null, _position++);
            }

            _errors.ReportInvalidCharacter(_position, CurrentChar);
            return new SyntaxToken(SyntaxKind.InvalidToken, _code.Substring(_position, 1), null, _position++);

        }
    }
}
