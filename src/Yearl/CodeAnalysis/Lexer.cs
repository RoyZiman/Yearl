using Yearl.CodeAnalysis.Errors;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;
using Yearl.CodeAnalysis.Text;

namespace Yearl.CodeAnalysis
{
    internal sealed class Lexer(SyntaxTree syntaxTree)
    {
        private readonly SyntaxTree _syntaxTree = syntaxTree;
        private readonly SourceText _text = syntaxTree.Text;
        private readonly ErrorHandler _errors = new();
        public ErrorHandler Errors => _errors;

        private int _position = 0;
        private int _start;
        private SyntaxKind _kind;
        private object _value;

        private char CurrentChar => Peek(0);
        private char Lookahead => Peek(1);

        private char Peek(int offset)
        {
            int index = _position + offset;

            if (index >= _text.Length)
                return '\0';

            return _text[index];
        }

        public SyntaxToken Lex()
        {
            _start = _position;
            _kind = SyntaxKind.InvalidToken;
            _value = null;

            switch (CurrentChar)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;
                case '+':
                    _kind = SyntaxKind.PlusToken;
                    _position++;
                    break;
                case '-':
                    _kind = SyntaxKind.MinusToken;
                    _position++;
                    break;
                case '*':
                    _kind = SyntaxKind.StarToken;
                    _position++;
                    break;
                case '/':
                    _kind = SyntaxKind.SlashToken;
                    _position++;
                    break;
                case '^':
                    _kind = SyntaxKind.HatToken;
                    _position++;
                    break;
                case '(':
                    _kind = SyntaxKind.LeftParenthesisToken;
                    _position++;
                    break;
                case ')':
                    _kind = SyntaxKind.RightParenthesisToken;
                    _position++;
                    break;
                case '{':
                    _kind = SyntaxKind.LeftCurlyBraceToken;
                    _position++;
                    break;
                case '}':
                    _kind = SyntaxKind.RightCurlyBraceToken;
                    _position++;
                    break;
                case ':':
                    _kind = SyntaxKind.ColonToken;
                    _position++;
                    break;
                case ',':
                    _kind = SyntaxKind.CommaToken;
                    _position++;
                    break;
                case '&':
                    if (Lookahead == '&')
                    {
                        _kind = SyntaxKind.AndToken;
                        _position += 2;
                    }
                    break;
                case '|':
                    if (Lookahead == '|')
                    {
                        _kind = SyntaxKind.OrToken;
                        _position += 2;
                    }
                    break;
                case '=':
                    _position++;
                    if (CurrentChar != '=')
                    {
                        _kind = SyntaxKind.EqualsToken;
                    }
                    else
                    {
                        _position++;
                        _kind = SyntaxKind.DoubleEqualsToken;
                    }
                    break;
                case '!':
                    _position++;
                    if (CurrentChar != '=')
                    {
                        _kind = SyntaxKind.NotToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.NotEqualsToken;
                        _position++;
                    }
                    break;
                case '>':
                    _position++;
                    if (CurrentChar != '=')
                    {
                        _kind = SyntaxKind.GreaterThanToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.GreaterThanEqualsToken;
                        _position++;
                    }
                    break;
                case '<':
                    _position++;
                    if (CurrentChar != '=')
                    {
                        _kind = SyntaxKind.LessThanToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.LessThanEqualsToken;
                        _position++;
                    }
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    ReadNumberToken();
                    break;
                case '"':
                    ReadStringToken();
                    break;

                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    ReadWhiteSpace();
                    break;

                default:
                    if (char.IsLetter(CurrentChar))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else if (char.IsWhiteSpace(CurrentChar))
                    {
                        ReadWhiteSpace();
                    }
                    else
                    {
                        TextSpan span = new(_position, 1);
                        TextLocation location = new(_text, span);
                        _errors.ReportInvalidCharacter(location, CurrentChar);
                        _position++;
                    }
                    break;
            }

            int length = _position - _start;
            string text = SyntaxFacts.GetText(_kind) ?? _text.ToString(_start, length);

            return new SyntaxToken(_syntaxTree, _kind, text, _value, _start);
        }

        private void ReadWhiteSpace()
        {
            while (char.IsWhiteSpace(CurrentChar))
                _position++;

            _kind = SyntaxKind.WhitespaceToken;
        }

        private void ReadNumberToken()
        {
            while (char.IsDigit(CurrentChar) || CurrentChar == '.')
                _position++;

            int length = _position - _start;
            string text = _text.ToString(_start, length);

            if (!double.TryParse(text, out double value))
            {
                TextSpan span = new(_start, length);
                TextLocation location = new(_text, span);
                _errors.ReportInvalidNumber(location, text, TypeSymbol.Number);
            }

            _value = value;
            _kind = SyntaxKind.NumberToken;
        }

        private void ReadStringToken()
        {

            string result = "";
            _position++;


            while (CurrentChar is not '\0' and not '"')
            {
                switch (CurrentChar)
                {
                    case '"':
                        continue;
                    case '\\':
                        _position++;
                        result += CurrentChar switch
                        {
                            'a' => '\a',
                            'b' => '\b',
                            'f' => '\f',
                            'n' => '\n',
                            'r' => '\r',
                            't' => '\t',
                            'v' => '\v',
                            _ => CurrentChar,
                        };
                        break;
                    default:
                        result += CurrentChar;
                        break;
                }
                _position++;
            }

            if (CurrentChar != '"')
            {
                TextSpan span = new(_start, 1);
                TextLocation location = new(_text, span);
                _errors.ReportUnterminatedString(location);
            }
            else
                _position++;
            _value = result;
            _kind = SyntaxKind.StringToken;

        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(CurrentChar))
                _position++;

            int length = _position - _start;
            string text = _text.ToString(_start, length);
            _kind = text.GetKeywordKind();
        }
    }
}