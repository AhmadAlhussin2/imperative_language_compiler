using System.Collections.Immutable;
using compilers.CodeAnalysis.Binding;
using compilers.CodeAnalysis.Lowering;
using compilers.CodeAnalysis.Symbol;
using LLVMSharp.Interop;

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
        public Compilation ContinueWith(SyntaxTree syntaxTree)
        {
            return new Compilation(this, syntaxTree);
        }
        public EvaluationResult Evaluate(LLVMBuilderRef builder, LLVMValueRef function)
        {
            var diagnostics = Syntax.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics, null);
            }

            var program = Binder.BindProgram(GlobalScope);

            if (program.Diagnostics.Any())
            {
                return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);
            }

            var statement = GetStatement();
            var evaluator = new Evaluator(builder, program.FunctionBodies, statement);
            var value = evaluator.Evaluate(function);
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
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
}