using System.Collections.Immutable;
using System.Text.RegularExpressions;
using compilers.CodeAnalysis.Text;

namespace compilers.CodeAnalysis
{
    internal sealed class Parser
    {
        private readonly SyntaxToken[] _tokens;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SourceText _text;
        private int _position;


        public Parser(SourceText text)
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
            _diagnostics.AddRange(Lexer.Diagnostics);
            _text = text;
        }

        public DiagnosticBag Diagnostics => _diagnostics;
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
            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, "", null);
        }
        private ExpressionSyntax ParseExpression()
        {
            return ParseAssignmentExpresion();
        }
        
        private ExpressionSyntax ParseAssignmentExpresion()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.AssignmentToken)
            {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseAssignmentExpresion();
                return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
            }
            return ParseBinaryExpression();
        }
        private ExpressionSyntax ParseBinaryExpression(int parentPriority = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPriority = Current.Kind.GetUnaryOperatorPriority();
            if (unaryOperatorPriority != 0 && unaryOperatorPriority >= parentPriority)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPriority);
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
                var right = ParseBinaryExpression(priority);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }
            return left;
        }
        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var members = ParseMembers();
            var EOFToken = MatchToken(SyntaxKind.EOFToken);
            return new CompilationUnitSyntax(members, EOFToken);
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            var members = ImmutableArray.CreateBuilder<MemberSyntax>();
            while (Current.Kind != SyntaxKind.EOFToken)
            {
                var c = Current;
                var member = ParseMember();
                members.Add(member);
                if (Current == c)
                {
                    NextToken();
                }
            }
            return members.ToImmutable();
        }

        private MemberSyntax ParseMember()
        {
            if (Current.Kind == SyntaxKind.RoutineKeyword)
                return ParseFunctionDecleration();
            else
                return ParseGlobalStatement();
        }

        private MemberSyntax ParseFunctionDecleration()
        {
            var routineKeyword = MatchToken(SyntaxKind.RoutineKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            var parameters = ParseParametersList();
            var closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
            var typeClause = ParseOptionalTypeClause();
            var isKeyword = MatchToken(SyntaxKind.IsKeyword);
            var body = ParseBlockStatement();
            var endKeyword = MatchToken(SyntaxKind.EndKeyword);
            return new FunctionDeclerationSyntax(routineKeyword, identifier, openParenthesisToken, parameters, closeParenthesisToken, typeClause, isKeyword, body, endKeyword);
        }

        private SeparatedSyntaxList<ParameterSyntax> ParseParametersList()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            var parseNextParameter = true;
            while(parseNextParameter && 
                  Current.Kind != SyntaxKind.CloseParenthesisToken &&
                  Current.Kind != SyntaxKind.EOFToken )
            {
                var parameter = ParseParameter();
                nodesAndSeparators.Add(parameter);
                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else
                {
                    parseNextParameter = false;
                }
            }
            return new SeparatedSyntaxList<ParameterSyntax>(nodesAndSeparators.ToImmutable());
        }

        private ParameterSyntax ParseParameter()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var type = ParseTypeClause();
            return new ParameterSyntax(identifier, type);
        }

        private MemberSyntax ParseGlobalStatement()
        {
            var statement = ParseStatement();
            return new GlobalStatementSyntax(statement);
        }


        private StatementSyntax ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.IsKeyword:
                case SyntaxKind.ThenKeyword:
                case SyntaxKind.LoopKeyword:
                    return ParseBlockStatement();
                case SyntaxKind.VarKeyword:
                    return ParseVariableDeclaration();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
                case SyntaxKind.RecordKeyword:
                    return ParseRecordStatement();
                case SyntaxKind.TypeKeyword:
                    return ParseTypeDeclaration();
                default:
                    return ParseExpressionStatement();

            }
        }

        private StatementSyntax ParseTypeDeclaration()
        {
            var typeKeyword = MatchToken(SyntaxKind.TypeKeyword);
            var name = MatchToken(SyntaxKind.IdentifierToken);
            var isKeyword = MatchToken(SyntaxKind.IsKeyword);
            var type = ParseTypeStatement();

            return new TypeDeclarationSyntax(typeKeyword, name, isKeyword, type);
        }

        private SyntaxToken ParseTypeStatement()
        {
            if(Current.Kind == SyntaxKind.RealKeyword || Current.Kind == SyntaxKind.IntegerKeyword || Current.Kind ==        SyntaxKind.BooleanKeyword)
            {
                return NextToken();
            }
            var type = MatchToken(SyntaxKind.IdentifierToken);
            return type;
        }

        private StatementSyntax ParseRecordStatement()
        {
            throw new NotImplementedException();
        }

        private StatementSyntax ParseForStatement()
        {
            var forKeyword = MatchToken(SyntaxKind.ForKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var inKeyword = MatchToken(SyntaxKind.InKeyword);
            var lowerBound = ParseExpression();
            var rangeToken = MatchToken(SyntaxKind.RangeToken);
            var upperBound = ParseExpression();
            var loopKeyword = MatchToken(SyntaxKind.LoopKeyword);
            var body = ParseBlockStatement();
            var endKeyword = MatchToken(SyntaxKind.EndKeyword);
            return new ForStatementSyntax(forKeyword, identifier, inKeyword, lowerBound, rangeToken, upperBound, loopKeyword, body, endKeyword);
        }

        private StatementSyntax ParseWhileStatement()
        {
            var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var loopKeyword = MatchToken(SyntaxKind.LoopKeyword);
            var body = ParseBlockStatement();
            var endKeyword = MatchToken(SyntaxKind.EndKeyword);
            return new WhileStatementSyntax(whileKeyword, condition, loopKeyword, body, endKeyword);
        }

        private StatementSyntax ParseIfStatement()
        {
            var ifKeyword = MatchToken(SyntaxKind.IfKeyword);

            var condition = ParseExpression();
            var thenKeyword = MatchToken(SyntaxKind.ThenKeyword);
            var thenStatement = ParseStatement();
            var elseClause = ParseElseClause();
            var endKeyword = MatchToken(SyntaxKind.EndKeyword);
            return new IfStatementSyntax(ifKeyword, condition, thenKeyword, thenStatement, elseClause, endKeyword);

        }

        private ElseClauseSyntax? ParseElseClause()
        {
            if (Current.Kind != SyntaxKind.ElseKeyword)
                return null;
            var keyword = MatchToken(SyntaxKind.ElseKeyword);
            var statement = ParseStatement();
            return new ElseClauseSyntax(keyword, statement);
        }

        private StatementSyntax ParseVariableDeclaration()
        {
            var varKeyword = MatchToken(SyntaxKind.VarKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var typeClause = ParseOptionalTypeClause();
            var isKeyword = MatchToken(SyntaxKind.IsKeyword);
            var initializer = ParseExpression();
            return new VariableDeclerationSyntax(varKeyword, identifier, typeClause, isKeyword, initializer);
        }

        private TypeClauseSyntax? ParseOptionalTypeClause()
        {
            if (Current.Kind != SyntaxKind.ColonToken)
                return null;
            return ParseTypeClause();
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colonToken = MatchToken(SyntaxKind.ColonToken);
            if(Current.Kind == SyntaxKind.IntegerKeyword || Current.Kind == SyntaxKind.RealKeyword || Current.Kind == SyntaxKind.   BooleanKeyword)
            {
                var identifierType = NextToken();
                return new TypeClauseSyntax(colonToken, identifierType);
            }
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            return new TypeClauseSyntax(colonToken, identifier);
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatementSyntax(expression);
        }

        private BlockStatementSyntax ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            //var startToken = NextToken();
            while (Current.Kind != SyntaxKind.EOFToken && Current.Kind != SyntaxKind.EndKeyword)
            {
                var c = Current;
                var statement = ParseStatement();
                statements.Add(statement);
                if (Current == c)
                {
                    NextToken();
                }
            }
            //var endToken = MatchToken(SyntaxKind.EndKeyword);
            return new BlockStatementSyntax(statements.ToImmutable());
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    return ParseParenthesisedExpression();
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                    return ParseBooleanLiteral();
                case SyntaxKind.NumberToken:
                case SyntaxKind.RealNumberToken:
                    return ParseNumberLiteral();
                case SyntaxKind.IdentifierToken:
                default:
                    return ParseNameOrCallExpression();
            }
        }

        private ExpressionSyntax ParseNumberLiteral()
        {
            if (Current.Kind == SyntaxKind.RealNumberToken)
            {
                var realNumberToken = NextToken();
                return new LiteralExpressionSyntax(realNumberToken);
            }
            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken);
        }

        private ExpressionSyntax ParseParenthesisedExpression()
        {
            var left = MatchToken(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpression();
            var right = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(left, expression, right);
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            var keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(keywordToken, isTrue);
        }

        private ExpressionSyntax ParseNameOrCallExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
                return ParseCallExpression();

            return ParseNameExpression();
        }
        private ExpressionSyntax ParseCallExpression()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            var arguments = ParseArguments();
            var closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new CallExpressionSyntax(identifier, openParenthesisToken, arguments, closeParenthesisToken);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            while (Current.Kind != SyntaxKind.CloseParenthesisToken &&
                  Current.Kind != SyntaxKind.EOFToken)
            {
                var expression = ParseExpression();
                nodesAndSeparators.Add(expression);
                if (Current.Kind != SyntaxKind.CloseParenthesisToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
            }
            return new SeparatedSyntaxList<ExpressionSyntax>(nodesAndSeparators.ToImmutable());
        }
        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            if(Current.Kind == SyntaxKind.DotToken)
            {
                var operatorToken = NextToken();
                var right = ParseNameExpression();
                return new NameExpressionSyntax(identifierToken, operatorToken,right);
            }
            return new NameExpressionSyntax(identifierToken, null, null);
        }
    }
}