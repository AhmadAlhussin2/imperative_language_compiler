using compilers.CodeAnalysis.Binding;

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
                    _globalScope = Binder.BindGlobalScope(Previous?.GlobalScope,Syntax.Root);
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
            var evaluator = new Evaluator(GlobalScope.Expression, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(Array.Empty<Diagnostic>(), value);
        }
    }



}