using compilers.CodeAnalysis.Symbol;
using compilers.CodeAnalysis.Text;

namespace compilers.CodeAnalysis
{
    internal sealed class Lexer
    {
        private readonly SourceText _text;
        private int _position;
        private DiagnosticBag _diagnostics = new();

        public DiagnosticBag Diagnostics => _diagnostics;

        
        public Lexer(SourceText text)
        {
            _text = text;
        }
        

        private char Current => Peek(0);
        private char Lookahead => Peek(1);
        private char Peek(int offset){
            var index = _position + offset;
            if (index >= _text.Length)
                return '\0';
            return _text[index];
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
            var identifier = _text.ToString(start,_position-start);
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
                "return" => SyntaxKind.ReturnKeyword,
                "in" => SyntaxKind.InKeyword,
                _ => SyntaxKind.IdentifierToken,
            };
            return new SyntaxToken(kind, start, identifier, null);
        }
        private SyntaxToken ReadNumber(int start)
        {
            while (char.IsDigit(Current) || Current=='.')
            {
                if (Current == '.')
                {
                    _position++;
                    if (Current == '.')
                    {
                        _position--;
                        break;
                    }
                }
                Next();
            }
            var length = _position - start;
            var number = _text.ToString(start,length);
            if (!int.TryParse(number, out var value))
            {
                if(!double.TryParse(number, out var real_value))
                {
                    _diagnostics.ReportInvalidNumber(new TextSpan(start, length), number, TypeSymbol.Int);
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
                var number = _text.ToString(start,_position-start);
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
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.NotEqualToken, _position+=2, "/=", null);
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
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.LessThanOrEqualToken, _position+=2, "<=", null);
                    else
                        return new SyntaxToken(SyntaxKind.LessThanToken, _position++, "<", null);
                case '>':
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.GreaterThanOrEqualToken, _position+=2, ">=", null);
                    else
                        return new SyntaxToken(SyntaxKind.GreaterThanToken, _position++, ">", null);
                case '%':
                    return new SyntaxToken(SyntaxKind.ModuloToken, _position++, "%", null);
                case ':':
                    if (Lookahead == '=')
                        return new SyntaxToken(SyntaxKind.AssignmentToken, _position+=2, ":=", null);
                    else
                        return new SyntaxToken(SyntaxKind.ColonToken, _position++, ":", null);
                case '=':
                    return new SyntaxToken(SyntaxKind.EqualToken, _position++, "=", null);
                case '.':
                    if (Lookahead == '.')
                        return new SyntaxToken(SyntaxKind.RangeToken, _position+=2, "..", null);
                    else
                        return new SyntaxToken(SyntaxKind.DotToken, _position++, ".", null);
                case ',':
                    return new SyntaxToken(SyntaxKind.CommaToken, _position++, ",", null);
                default:
                    if (Current == '_' || char.IsLetter(Current))
                        return ReadVarOrKeyword(start);
                    _diagnostics.ReportBadCharacter(_position, Current);
                    return new SyntaxToken(SyntaxKind.UnknowToken, _position++, _text.ToString(_position - 1, 1), null);
            }
        }

    }
}