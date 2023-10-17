namespace compilers.CodeAnalysis
{
    internal sealed class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private int _position;
        private List<string> _errors = new List<string>();
        public IEnumerable<string> Errors => _errors;

        public Parser(string text)
        {
            var tokens = new List<SyntaxToken>();
            var Lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = Lexer.NextToken();
                if (token.Kind != SyntaxKind.WhiteSpace && token.Kind != SyntaxKind.UnknowToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EOFToken);

            _tokens = tokens.ToArray();
            _errors.AddRange(Lexer.Errors);
        }
        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[_tokens.Length - 1];
            return _tokens[index];
        }
        private SyntaxToken Current => Peek(0);
        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }
        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();
            _errors.Add($"Error: unexpected token '{Current.Kind}' expected '{kind}'");
            return new SyntaxToken(kind, Current.Position, "", null);
        }
        private ExpressionSyntax ParseExpression(int parentPriority = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPriority = Current.Kind.GetUnaryOperatorPriority();
            if (unaryOperatorPriority != 0 && unaryOperatorPriority >= parentPriority)
            {
                var operatorToken = NextToken();
                var operand = ParseExpression(unaryOperatorPriority);
                left = new UnaryExpressionSyntax(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }
            while (true)
            {
                var priority = Current.Kind.GetBinaryOperatorPriority();
                if (priority == 0 || priority <= parentPriority)
                {
                    break;
                }
                var operatorToken = NextToken();
                var right = ParseExpression(priority);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }
            return left;
        }


        public SyntaxTree Parse()
        {
            var expression = ParseExpression();
            var EOFToken = MatchToken(SyntaxKind.EOFToken);
            return new SyntaxTree(_errors, expression, EOFToken);
        }
        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    {
                        var left = NextToken();
                        var expression = ParseExpression();
                        var right = MatchToken(SyntaxKind.CloseParenthesisToken);
                        return new ParenthesizedExpressionSyntax(left, expression, right);
                    }

                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                    {
                        var keywordToken = NextToken();
                        var value = keywordToken.Kind == SyntaxKind.TrueKeyword;
                        return new LiteralExpressionSyntax(keywordToken, value);
                    }
                default:
                    var numberToken = MatchToken(SyntaxKind.NumberToken);
                    return new LiteralExpressionSyntax(numberToken);
            }
        }
    }
}