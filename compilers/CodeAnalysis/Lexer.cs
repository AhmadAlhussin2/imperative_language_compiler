namespace compilers.CodeAnalysis
{
    internal sealed class Lexer
    {
        private readonly string _text;
        private int _position;
        private List<string> _errors = new List<string>();

        public IEnumerable<string> Errors => _errors;

        public Lexer(string text)
        {
            _text = text;
        }

        private char Current
        {
            get
            {
                if (_position >= _text.Length)
                    return '\0';
                return _text[_position];
            }
        }

        private void Next()
        {
            _position++;
        }

        private SyntaxToken ReadNumber(int start)
        {
            while (char.IsDigit(Current))
            {
                Next();
            }
            var number = _text[start.._position];
            if (!int.TryParse(number, out var value))
            {
                _errors.Add($"The number '{_text}' cannot be represented");
            }
            return new SyntaxToken(SyntaxKind.NumberToken, start, number, value);
        }

        public SyntaxToken NextToken()
        {
            if (_position >= _text.Length)
                return new SyntaxToken(SyntaxKind.EOFToken, _position, "\0", null);
            var start = _position;
            if (char.IsDigit(Current))
            {
                return ReadNumber(start);
            }
            if (char.IsWhiteSpace(Current))
            {
                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }
                var number = _text[start.._position];
                return new SyntaxToken(SyntaxKind.WhiteSpace, start, number, null);
            }
            if (Current == '+')
                return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
            else if (Current == '-')
                return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
            else if (Current == '*')
                return new SyntaxToken(SyntaxKind.StarToken, _position++, "*", null);
            else if (Current == '/')
                return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null);
            else if (Current == '(')
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null);
            else if (Current == ')')
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", null);

            _errors.Add($"Error bad character in the input: '{Current}'");
            return new SyntaxToken(SyntaxKind.UnknowToken, _position++, _text.Substring(_position - 1, 1), null);
        }

    }
}