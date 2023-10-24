using compilers.CodeAnalysis.Binding;
using compilers.CodeAnalysis.Lowering;
using compilers.CodeAnalysis.Symbol;

namespace compilers.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope? _globalScope;
        public Compilation(SyntaxTree syntax) : this(null, syntax)
        {
        }
        private Compilation(Compilation? previous, SyntaxTree syntax)
        {
            Previous = previous;
            Syntax = syntax;
        }

        public Compilation? Previous { get; }
        public SyntaxTree Syntax { get; }
        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    _globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, Syntax.Root);
                }
                return _globalScope;
            }
        }
        public Compilation continueWith(SyntaxTree syntaxTree)
        {
            return new Compilation(this, syntaxTree);
        }
        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var diagnostics = Syntax.Diagnostics.Concat(GlobalScope.Diagnostics).ToArray();
            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics, null);
            }
            var statement = GetStatement();
            var evaluator = new Evaluator(statement, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(Array.Empty<Diagnostic>(), value);
        }

        internal void WriteTree(TextWriter boundSyntaxTreeWriter)
        {
            var statement = GetStatement();
            statement.WriteTo(boundSyntaxTreeWriter);
        }

        private BoundBlockStatement GetStatement()
        {
            var result = GlobalScope.Statement;
            return Lowerer.Lower(result);
        }
    }

    public abstract class MemberSyntax : SyntaxNode
    {

    }
    public sealed class GlobalStatementSyntax : MemberSyntax
    {
        public GlobalStatementSyntax(StatementSyntax statement)
        {
            Statement = statement;
        }

        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;

        public StatementSyntax Statement { get; }
    }
    public sealed class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax(SyntaxToken identifier, TypeClauseSyntax type)
        {
            Identifier = identifier;
            Type = type;
        }

        public override SyntaxKind Kind => SyntaxKind.Parameter;

        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax Type { get; }
    }
    public sealed class FunctionDeclerationSyntax : MemberSyntax
    {
        public FunctionDeclerationSyntax(
            SyntaxToken routineKeyword, SyntaxToken identifier,
            SyntaxToken openParenthesisToken, SeparatedSyntaxList<ParameterSyntax> parameters,
            SyntaxToken closeParenthesisToken, TypeClauseSyntax typeClause, SyntaxToken isKeyword)
        {
            RoutineKeyword = routineKeyword;
            Identifier = identifier;
            OpenParenthesisToken = openParenthesisToken;
            Parameters = parameters;
            CloseParenthesisToken = closeParenthesisToken;
            TypeClause = typeClause;
            IsKeyword = isKeyword;
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionDecleration;

        public SyntaxToken RoutineKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public TypeClauseSyntax TypeClause { get; }
        public SyntaxToken IsKeyword { get; }
    }
}