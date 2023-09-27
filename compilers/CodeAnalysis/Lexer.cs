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
        public List<string> ViewErrors ()
        {
            return _errors;
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

        private SyntaxToken ReadVarOrKeyword(int start)
        {
            while (char.IsDigit(Current) || char.IsLetter(Current) || Current == '_')
            {
                Next();
            }
            var identifier = _text[start.._position];
            var kind = identifier switch
            {
                "and" => SyntaxKind.AndKeyword,
                "or" => SyntaxKind.OrKeyword,
                "xor" => SyntaxKind.XorKeyword,
                "if" => SyntaxKind.IfKeyword,
                "is" => SyntaxKind.IsKeyword,
                "integer" => SyntaxKind.IntegerKeyword,
                "real" => SyntaxKind.RealKeyword,
                "var" => SyntaxKind.VarKeyword,
                "type" => SyntaxKind.TypeKeyword,
                "boolean" => SyntaxKind.BooleanKeyword,
                "true" => SyntaxKind.TrueKeyword,
                "false" => SyntaxKind.FalseKeyword,
                "record" => SyntaxKind.RecordKeyword,
                "end" => SyntaxKind.EndKeyword,
                "array" => SyntaxKind.ArrayKeyword,
                "while" => SyntaxKind.WhileKeyword,
                "loop" => SyntaxKind.LoopKeyword,
                "for" => SyntaxKind.ForKeyword,
                "reverse" => SyntaxKind.ReverseKeyword,
                "then" => SyntaxKind.ThenKeyword,
                "else" => SyntaxKind.ElseKeyword,
                "routine" => SyntaxKind.RoutineKeyword,
                "not" => SyntaxKind.NotKeyword,
                _ => SyntaxKind.IdentifierToken,
            };
            return new SyntaxToken(kind, start, identifier, null);
        }
        private SyntaxToken ReadNumber(int start)
        {
            while (char.IsDigit(Current) || Current=='.')
            {
                Next();
            }
            var number = _text[start.._position];
            if (!int.TryParse(number, out var value))
            {
                if(!double.TryParse(number, out var real_value))
                {
                    _errors.Add($"The term '{number}' cannot be parsed into decimal number");
                    return new SyntaxToken(SyntaxKind.RealNumberToken, start, number, "Nan");
                }
                return new SyntaxToken(SyntaxKind.RealNumberToken, start, number, real_value);
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
            switch (Current)
            {
                case '+':
                    return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
                case '-':
                    return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
                case '*':
                    return new SyntaxToken(SyntaxKind.StarToken, _position++, "*", null);
                case '/':
                    _position++;
                    if (Current == '=')
                        return new SyntaxToken(SyntaxKind.NotEqualToken, _position++, "/=", null);
                    else
                        return new SyntaxToken(SyntaxKind.NegationToken, _position++, "/", null);
                case '(':
                    return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null);
                case ')':
                    return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", null);
                case '[':
                    return new SyntaxToken(SyntaxKind.OpenSquareBracketToken, _position++, "[", null);
                case ']':
                    return new SyntaxToken(SyntaxKind.CloseSquareBracketToken, _position++, "]", null);
                case '<':
                    _position++;
                    if (Current == '=')
                        return new SyntaxToken(SyntaxKind.LessThanOrEqualToken, _position++, "<=", null);
                    else
                        return new SyntaxToken(SyntaxKind.LessThanToken, _position, "<", null);
                case '>':
                    _position++;
                    if (Current == '=')
                        return new SyntaxToken(SyntaxKind.GreaterThanOrEqualToken, _position++, ">=", null);
                    else
                        return new SyntaxToken(SyntaxKind.GreaterThanToken, _position, ">", null);
                case '%':
                    return new SyntaxToken(SyntaxKind.ModuloToken, _position++, "%", null);
                case ':':
                    _position++;
                    if (Current == '=')
                        return new SyntaxToken(SyntaxKind.AssignmentToken, _position++, ":=", null);
                    else
                        return new SyntaxToken(SyntaxKind.ColonToken, _position, ":", null);
                case '=':
                    return new SyntaxToken(SyntaxKind.EqualToken, _position++, "=", null);
                case '.':
                    _position++;
                    if (Current == '.')
                        return new SyntaxToken(SyntaxKind.RangeToken, _position++, "..", null);
                    else
                        return new SyntaxToken(SyntaxKind.DotToken, _position, ".", null);
                default:
                    if (Current == '_' || char.IsLetter(Current))
                        return ReadVarOrKeyword(start);
                    _errors.Add($"Error bad character in the input: '{Current}'");
                    return new SyntaxToken(SyntaxKind.UnknowToken, _position++, _text.Substring(_position - 1, 1), null);
            }
        }

    }
}