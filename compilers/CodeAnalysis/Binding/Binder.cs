using System.Collections.Immutable;
using compilers.CodeAnalysis.Lowering;
using compilers.CodeAnalysis.Symbol;

namespace compilers.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag _diagnostics = new();
        private readonly FunctionSymbol? _function;
        private BoundScope _scope;

        public Binder(BoundScope? parent, FunctionSymbol? function)
        {
            _scope = new BoundScope(parent);
            _function = function;

            if (function != null)
            {
                foreach (var p in function.Parameters)
                    _scope.TryDeclareVariable(p);

            }
        }
        private static BoundScope CreateParentScopes(BoundGlobalScope? previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }
            var parent = CreateRootScope();
            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                foreach (var f in previous.Functions)
                {
                    scope.TryDeclareFunction(f);
                }
                foreach (var v in previous.Variables)
                {
                    scope.TryDeclareVariable(v);
                }
                parent = scope;
            }
            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null);
            foreach (var f in BuiltinFunctions.GetAll())
            {
                result.TryDeclareFunction(f);
            }
            return result;
        }

        public static BoundProgram BindProgram(BoundGlobalScope globalScope)
        {
            var parentScope = CreateParentScopes(globalScope);
            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var diagnostics = new DiagnosticBag();

            var scope = globalScope;
            while (scope != null)
            {
                foreach (var function in scope.Functions)
                {
                    var binder = new Binder(parentScope, function);
                    if (function.Decleration != null)
                    {
                        BoundStatement? body = binder.BindStatement(function.Decleration.Body);
                        var loweredBody = Lowerer.Lower(body);
                        functionBodies.Add(function, loweredBody);
                        diagnostics.AddRange(binder.Diagnostics);
                    }
                }
                scope = scope.Previous;
            }
            return new BoundProgram(globalScope, diagnostics, functionBodies.ToImmutable());
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnitSyntax syntax)
        {
            var parentScope = CreateParentScopes(previous);
            var binder = new Binder(parentScope, null);

            foreach (var function in syntax.Members.OfType<FunctionDeclerationSyntax>())
                binder.BindFunctionDecleration(function);
            var statementBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
            foreach (var globalStatement in syntax.Members.OfType<GlobalStatementSyntax>())
            {
                var s = binder.BindStatement(globalStatement.Statement);
                statementBuilder.Add(s);
            }
            var statement = new BoundBlockStatement(statementBuilder.ToImmutable());

            var functions = binder._scope.GetDeclaredFunctions();

            var variables = binder._scope.GetDeclaredVariables();
            var diagnostic = binder.Diagnostics.ToImmutableArray();
            if (previous != null)
            {
                diagnostic = diagnostic.InsertRange(0, previous.Diagnostics);
            }
            return new BoundGlobalScope(previous, diagnostic, functions, variables, statement);
        }

        private void BindFunctionDecleration(FunctionDeclerationSyntax syntax)
        {
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameterNames = new HashSet<string>();
            foreach (var parameterSyntax in syntax.Parameters)
            {
                var parameterName = parameterSyntax.Identifier.Text;
                var parameterType = BindTypeClause(parameterSyntax.Type);
                if (!seenParameterNames.Add(parameterName))
                {
                    _diagnostics.ReportParameterAlreadyDeclared(parameterSyntax.Span, parameterName);
                }
                else
                {
                    var parameter = new ParameterSymbol(parameterName, parameterType!);
                    parameters.Add(parameter);
                }
            }
            var type = BindTypeClause(syntax.TypeClause) ?? TypeSymbol.Void;
            var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);
            if (!_scope.TryDeclareFunction(function))
            {
                _diagnostics.ReportFunctionAlreadyDeclared(syntax.Identifier.Span, function.Name);
            }

        }

        public DiagnosticBag Diagnostics => _diagnostics;
        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BlockStatement:
                    return BindBlockStatement((BlockStatementSyntax)syntax);
                case SyntaxKind.VariableDecleration:
                    return BindVariableDeclaration((VariableDeclerationSyntax)syntax);
                case SyntaxKind.IfStatement:
                    return BindIfStatement((IfStatementSyntax)syntax);
                case SyntaxKind.WhileStatement:
                    return BindWhileStatement((WhileStatementSyntax)syntax);
                case SyntaxKind.ForStatement:
                    return BindForStatement((ForStatementSyntax)syntax);
                case SyntaxKind.ExpressionStatement:
                    return BindExpressionStatement((ExpressionStatementSyntax)syntax);
                case SyntaxKind.ReturnStatement:
                    return BindReturnStatement((ReturnStatementSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundStatement BindReturnStatement(ReturnStatementSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression);
            if (_function == null)
            {
                _diagnostics.ReportInvalidReturn(syntax.ReturnKeyword.Span);
            }
            else if (_function.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportCannotReturnInVoidFunctions(syntax.Expression.Span);
            }
            else
            {
                expression = BindConversion(syntax.Expression.Span, expression, _function.Type);
            }
            return new BoundReturnStatement(expression);
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
            var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

            _scope = new BoundScope(_scope);
            var variable = BindVariable(syntax.Identifier, TypeSymbol.Int);
            var body = BindStatement(syntax.Body);
            _scope = _scope.Parent!;
            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }

        private VariableSymbol BindVariable(SyntaxToken identifier, TypeSymbol type)
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;
            var variable = _function == null
                                ? (VariableSymbol)new GlobalVariableSymbol(name, type)
                                : new LocalVariableSymbol(name, type);


            if (declare && !_scope.TryDeclareVariable(variable))
                _diagnostics.ReportVariableAlreadyDeclared(identifier.Span, name);

            return variable;
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var body = BindStatement(syntax.Body);
            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var statement = BindStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, statement, elseStatement);
        }

        private BoundStatement BindVariableDeclaration(VariableDeclerationSyntax syntax)
        {
            var type = BindTypeClause(syntax.TypeClause);
            var initializer = BindExpression(syntax.Initializer!);
            var variableType = type ?? initializer.Type;
            var convertedInitializer = BindConversion(syntax.Initializer!.Span, initializer, variableType);
            var variable = BindVariable(syntax.Identifier, variableType);
            return new BoundVariableDeclaration(variable, convertedInitializer);
        }

        private TypeSymbol? BindTypeClause(TypeClauseSyntax? syntaxt)
        {
            if (syntaxt == null)
                return null;
            var type = LookupType(syntaxt.Identifier.Text);
            if (type == null)
                _diagnostics.ReportUndefinedType(syntaxt.Identifier.Span, syntaxt.Identifier.Text);

            return type;
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression, canBeVoid: true);
            return new BoundExpressionStatement(expression);
        }

        private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);
            foreach (var statementSyntax in syntax.Statements)
            {
                var statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }
            _scope = _scope!.Parent!;
            return new BoundBlockStatement(statements.ToImmutable());
        }

        public BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType)
        {
            return BindConversion(syntax, targetType);
        }
        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var result = BindExpressionInternal(syntax);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionMustHaveValue(syntax.Span);
                return new BoundErrorExpression();
            }
            return result;
        }
        public BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.CallExpression:
                    return BindCallExpression((CallExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                case SyntaxKind.ParenthesizedExpression:
                    return BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax);
                case SyntaxKind.NameExpression:
                    return BindNameExpression((NameExpressionSyntax)syntax);
                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private TypeSymbol? LookupType(string name)
        {
            switch (name)
            {
                case "boolean":
                    return TypeSymbol.Bool;
                case "integer":
                    return TypeSymbol.Int;
                case "real":
                    return TypeSymbol.Real;
                default:
                    return null;
            }
        }
        private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
        {
            if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
            {
                return BindConversion(syntax.Arguments[0], type, allowExplicit: true);
            }
            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();
            foreach (var argument in syntax.Arguments)
            {
                var boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }
            if (!_scope.TryLookupFunction(syntax.Identifier.Text, out var function))
            {
                _diagnostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (syntax.Arguments.Count != function!.Parameters.Length)
            {
                _diagnostics.ReportWrongArgumentCount(syntax.Span, function.Name, function.Parameters.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }


            for (var i = 0; i < syntax.Arguments.Count; i++)
            {
                var argument = boundArguments[i];
                var parameter = function.Parameters[i];

                if (argument.Type != parameter.Type)
                {
                    _diagnostics.ReportWrongArgumentType(syntax.Arguments[i].Span, function.Name, parameter.Name, parameter.Type, argument.Type);
                    return new BoundErrorExpression();
                }

            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = BindExpression(syntax);
            return BindConversion(syntax.Span, expression, type, allowExplicit);
        }
        private BoundExpression BindConversion(TextSpan diagnosticSpan, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, type);
            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                    _diagnostics.ReportCannotConvert(diagnosticSpan, expression.Type, type);
                return new BoundErrorExpression();
            }
            if (!allowExplicit && conversion.IsExplicit)
            {
                _diagnostics.ReportCannotConvertImplicitly(diagnosticSpan, expression.Type, type);
            }
            if (conversion.IsIdentity)
                return expression;
            return new BoundConversionExpression(type, expression);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);
            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }
            var convertedExpression = BindConversion(syntax.Expression.Span, boundExpression, variable!.Type);
            return new BoundAssignmentExpression(variable, convertedExpression);
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            if (syntax.IdentifierToken.IsMissing)
            {
                return new BoundErrorExpression();
            }
            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();
            }
            return new BoundVariableExpression(variable!);
        }

        private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            return BindExpression(syntax.Expression);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            if (boundOperand.Type == TypeSymbol.Error)
                return new BoundErrorExpression();
            var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return new BoundErrorExpression();
            }
            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var left = BindExpression(syntax.Left);
            var right = BindExpression(syntax.Right);
            if (left.Type == TypeSymbol.Error || right.Type == TypeSymbol.Error)
            {
                return new BoundErrorExpression();
            }
            var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, left.Type, right.Type);
            if (boundOperator == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, left.Type, right.Type);
                return new BoundErrorExpression();
            }
            return new BoundBinaryExpression(left, boundOperator, right);
        }
    }
}