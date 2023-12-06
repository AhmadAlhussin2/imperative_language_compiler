using System.Collections.Immutable;
using compilers.CodeAnalysis.Symbol;
using compilers.CodeAnalysis.Text;

namespace compilers.CodeAnalysis
{
    internal sealed class Parser
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SourceText _text;
        private int _position;


        public Parser(SyntaxTree syntaxTree)
        {
            var tokens = new List<SyntaxToken>();
            var Lexer = new Lexer(syntaxTree);
            SyntaxToken token;
            do
            {
                token = Lexer.NextToken();
                if (token.Kind != SyntaxKind.WhiteSpace && token.Kind != SyntaxKind.UnknowToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EOFToken);
            _syntaxTree = syntaxTree;
            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(Lexer.Diagnostics);
            _text = syntaxTree.Text;
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
            return new SyntaxToken(_syntaxTree, kind, Current.Position, null, null);
        }
        private ExpressionSyntax ParseExpression()
        {
            return ParseAssignmentExpresion();
        }

        private ExpressionSyntax ParseAssignmentExpresion()
        {
            if (Peek(0).Kind != SyntaxKind.IdentifierToken)
            {
                return ParseBinaryExpression();
            }
            var pos = _position;
            var variable = ParseVariableExpression();
            if (Peek(0).Kind == SyntaxKind.AssignmentToken)
            {
                var operatorToken = NextToken();
                var right = ParseAssignmentExpresion();
                return new AssignmentExpressionSyntax(_syntaxTree, variable, operatorToken, right);
            }
            _position = pos;
            return ParseBinaryExpression();
        }

        private Variable ParseVariableExpression()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            List<ExpressionSyntax> indices = new();
            while (Current.Kind == SyntaxKind.OpenSquareBracketToken)
            {
                MatchToken(SyntaxKind.OpenSquareBracketToken);
                var expression = ParseExpression();
                indices.Add(expression);
                MatchToken(SyntaxKind.CloseSquareBracketToken);
            }
            return new Variable(_syntaxTree, identifier, indices);
        }

        private ExpressionSyntax ParseBinaryExpression(int parentPriority = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPriority = Current.Kind.GetUnaryOperatorPriority();
            if (unaryOperatorPriority != 0 && unaryOperatorPriority >= parentPriority)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPriority);
                left = new UnaryExpressionSyntax(_syntaxTree, operatorToken, operand);
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
                left = new BinaryExpressionSyntax(_syntaxTree, left, operatorToken, right);
            }
            return left;
        }
        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var members = ParseMembers();
            var EOFToken = MatchToken(SyntaxKind.EOFToken);
            return new CompilationUnitSyntax(_syntaxTree, members, EOFToken);
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
            var typeClause = ParseOptionalType();
            var isKeyword = MatchToken(SyntaxKind.IsKeyword);
            var body = ParseBlockStatement();
            var endKeyword = MatchToken(SyntaxKind.EndKeyword);
            return new FunctionDeclerationSyntax(_syntaxTree, routineKeyword, identifier, openParenthesisToken, parameters, closeParenthesisToken, typeClause, isKeyword, body, endKeyword);
        }

        private SeparatedSyntaxList<ParameterSyntax> ParseParametersList()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            var parseNextParameter = true;
            while (parseNextParameter &&
                  Current.Kind != SyntaxKind.CloseParenthesisToken &&
                  Current.Kind != SyntaxKind.EOFToken)
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
            MatchToken(SyntaxKind.ColonToken);
            var type = ParseType();
            return new ParameterSyntax(_syntaxTree, identifier, type);
        }

        private MemberSyntax ParseGlobalStatement()
        {
            var statement = ParseStatement();
            return new GlobalStatementSyntax(_syntaxTree, statement);
        }


        private StatementSyntax ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.VarKeyword:
                    return ParseVariableDeclaration();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
                case SyntaxKind.RecordKeyword:
                    return ParseRecordDeclaration();
                case SyntaxKind.TypeKeyword:
                    return ParseTypeDeclaration();
                case SyntaxKind.ReturnKeyword:
                    return ParseReturnStatement();
                default:
                    return ParseExpressionStatement();

            }
        }

        private StatementSyntax ParseReturnStatement()
        {
            var keyword = MatchToken(SyntaxKind.ReturnKeyword);
            var expression = ParseExpression();
            return new ReturnStatementSyntax(_syntaxTree, keyword, expression);
        }

        // private SyntaxNode ParseArrayDeclaration()
        // {
        //     var arrayKeyworld = MatchToken(SyntaxKind.ArrayKeyword);
        //     var openBracket = MatchToken(SyntaxKind.OpenSquareBracketToken);
        //     var size = ParseExpression();
        //     var closeBracket = MatchToken(SyntaxKind.CloseSquareBracketToken);
        //     return new ArrayDeclarationSyntax(arrayKeyworld, openBracket, size, closeBracket);
        // }

        private StatementSyntax ParseTypeDeclaration()
        {
            var typeKeyword = MatchToken(SyntaxKind.TypeKeyword);
            var name = MatchToken(SyntaxKind.IdentifierToken);
            var isKeyword = MatchToken(SyntaxKind.IsKeyword);
            var type = ParseTypeStatement();
            return new TypeDeclarationSyntax(_syntaxTree, typeKeyword, name, isKeyword, type);
        }

        private SyntaxNode ParseTypeStatement()
        {
            if (Current.Kind == SyntaxKind.RealKeyword || Current.Kind == SyntaxKind.IntegerKeyword || Current.Kind == SyntaxKind.BooleanKeyword)
            {
                return NextToken();
            }
            if (Current.Kind == SyntaxKind.RecordKeyword)
            {
                return ParseRecordDeclaration();
            }
            // if(Current.Kind == SyntaxKind.ArrayKeyword)
            // {
            //     return ParseArrayDeclaration();
            // }
            var type = MatchToken(SyntaxKind.IdentifierToken);
            return type;
        }

        private StatementSyntax ParseRecordDeclaration()
        {
            var recordKeyword = MatchToken(SyntaxKind.RecordKeyword);
            var parameters = ParseParametersList();
            var endKeyword = MatchToken(SyntaxKind.EndKeyword);
            return new RecordDeclerationSyntax(_syntaxTree, recordKeyword, parameters, endKeyword);
        }

        private StatementSyntax ParseForStatement()
        {
            var forKeyword = MatchToken(SyntaxKind.ForKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var inKeyword = MatchToken(SyntaxKind.InKeyword);
            SyntaxToken? reverseKeyword = null;
            if (Current.Kind == SyntaxKind.ReverseKeyword)
            {
                reverseKeyword = MatchToken(SyntaxKind.ReverseKeyword);
            }
            var lowerBound = ParseExpression();
            var rangeToken = MatchToken(SyntaxKind.RangeToken);
            var upperBound = ParseExpression();
            var loopKeyword = MatchToken(SyntaxKind.LoopKeyword);
            var body = ParseBlockStatement();
            var endKeyword = MatchToken(SyntaxKind.EndKeyword);
            return new ForStatementSyntax(_syntaxTree, forKeyword, identifier, inKeyword, reverseKeyword, lowerBound, rangeToken, upperBound, loopKeyword, body, endKeyword);
        }
        
        private StatementSyntax ParseWhileStatement()
        {
            var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var loopKeyword = MatchToken(SyntaxKind.LoopKeyword);
            var body = ParseBlockStatement();
            var endKeyword = MatchToken(SyntaxKind.EndKeyword);
            return new WhileStatementSyntax(_syntaxTree, whileKeyword, condition, loopKeyword, body, endKeyword);
        }

        private StatementSyntax ParseIfStatement()
        {
            var ifKeyword = MatchToken(SyntaxKind.IfKeyword);

            var condition = ParseExpression();
            var thenKeyword = MatchToken(SyntaxKind.ThenKeyword);
            var thenStatement = ParseBlockStatement(true);
            var elseClause = ParseElseClause();
            var endKeyword = MatchToken(SyntaxKind.EndKeyword);
            return new IfStatementSyntax(_syntaxTree, ifKeyword, condition, thenKeyword, thenStatement, elseClause, endKeyword);

        }

        private ElseClauseSyntax? ParseElseClause()
        {
            if (Current.Kind != SyntaxKind.ElseKeyword)
                return null;
            var keyword = MatchToken(SyntaxKind.ElseKeyword);
            var statement = ParseBlockStatement();
            return new ElseClauseSyntax(_syntaxTree, keyword, statement);
        }

        private StatementSyntax ParseVariableDeclaration()
        {
            var varKeyword = MatchToken(SyntaxKind.VarKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var typeClause = ParseOptionalType();
            // if (Current.Kind != SyntaxKind.IsKeyword)
            // {
            //      return new VariableDeclerationSyntax(varKeyword, identifier, typeClause, null, null);
            // }
            var isKeyword = MatchToken(SyntaxKind.IsKeyword);
            var initializer = ParseExpression();
            return new VariableDeclerationSyntax(_syntaxTree, varKeyword, identifier, typeClause, isKeyword, initializer);
        }

        // private TypeClauseSyntax? ParseOptionalTypeClause()
        // {
        //     if (Current.Kind != SyntaxKind.ColonToken)
        //         return null;
        //     return ParseTypeClause();
        // }
        private TypeSyntax? ParseOptionalType()
        {
            if (Current.Kind != SyntaxKind.ColonToken)
                return null;
            var colonToken = MatchToken(SyntaxKind.ColonToken);
            return ParseType();
        }
        private TypeSyntax ParseType()
        {
            if (Current.Kind == SyntaxKind.IntegerKeyword)
            {
                var keyword = MatchToken(SyntaxKind.IntegerKeyword);
                return new PrimativeType(_syntaxTree, keyword);
            }
            else if (Current.Kind == SyntaxKind.RealKeyword)
            {
                var keyword = MatchToken(SyntaxKind.RealKeyword);
                return new PrimativeType(_syntaxTree, keyword);
            }
            else if (Current.Kind == SyntaxKind.BooleanKeyword)
            {
                var keyword = MatchToken(SyntaxKind.BooleanKeyword);
                return new PrimativeType(_syntaxTree, keyword);
            }
            else if (Current.Kind == SyntaxKind.ArrayKeyword)
            {
                var arrayKeyword = MatchToken(SyntaxKind.ArrayKeyword);
                var openSquare = MatchToken(SyntaxKind.OpenSquareBracketToken);
                var size = ParseExpression();
                var closeSquare = MatchToken(SyntaxKind.CloseSquareBracketToken);
                var type = ParseType();
                return new ArrayType(_syntaxTree, arrayKeyword, openSquare, size, closeSquare, type);
            }
            Console.WriteLine(Current.Kind);
            throw new Exception("Unknown type");
        }
        // private TypeClauseSyntax ParseTypeClause()
        // {
        //     var colonToken = MatchToken(SyntaxKind.ColonToken);
        //     if (Current.Kind == SyntaxKind.IntegerKeyword || Current.Kind == SyntaxKind.RealKeyword || Current.Kind == SyntaxKind.BooleanKeyword)
        //     {
        //         var identifierType = NextToken();
        //         return new TypeClauseSyntax(_syntaxTree, colonToken, identifierType);
        //     }
        //     var identifier = MatchToken(SyntaxKind.IdentifierToken);
        //     return new TypeClauseSyntax(_syntaxTree, colonToken, identifier);
        // }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatementSyntax(_syntaxTree, expression);
        }

        private BlockStatementSyntax ParseBlockStatement(bool waitForElse = false)
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
            //var startToken = NextToken();
            while (Current.Kind != SyntaxKind.EOFToken && Current.Kind != SyntaxKind.EndKeyword && (Current.Kind != SyntaxKind.ElseKeyword || !waitForElse))
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
            return new BlockStatementSyntax(_syntaxTree, statements.ToImmutable());
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
                return new LiteralExpressionSyntax(_syntaxTree, realNumberToken);
            }
            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(_syntaxTree, numberToken);
        }

        private ExpressionSyntax ParseParenthesisedExpression()
        {
            var left = MatchToken(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpression();
            var right = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(_syntaxTree, left, expression, right);
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            var keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(_syntaxTree, keywordToken, isTrue);
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
            return new CallExpressionSyntax(_syntaxTree, identifier, openParenthesisToken, arguments, closeParenthesisToken);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArguments()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            var parseNextArgument = true;
            while (parseNextArgument && Current.Kind != SyntaxKind.CloseParenthesisToken &&
                  Current.Kind != SyntaxKind.EOFToken)
            {
                var expression = ParseExpression();
                nodesAndSeparators.Add(expression);
                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else
                {
                    parseNextArgument = false;
                }
            }
            return new SeparatedSyntaxList<ExpressionSyntax>(nodesAndSeparators.ToImmutable());
        }
        private ExpressionSyntax ParseNameExpression()
        {
            var variable = ParseVariableExpression();
            return new NameExpressionSyntax(_syntaxTree, variable);
        }
    }
}