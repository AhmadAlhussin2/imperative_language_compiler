using System.Collections.Immutable;
using compilers.CodeAnalysis.Binding;
using compilers.CodeAnalysis.Lowering;
using compilers.CodeAnalysis.Symbol;
using LLVMSharp.Interop;

namespace compilers.CodeAnalysis
{
    public sealed class Compilation
    {
        static private readonly StreamWriter _boundSyntaxTreeWriter = new("B_AST(functions).txt");
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
            foreach (var funco in program.FunctionBodies){
                _boundSyntaxTreeWriter.Write("routine " + funco.Key.Name + " (");
                int cntC = 0;
                foreach (var parameter in funco.Key.Parameters){
                    if (cntC >0)_boundSyntaxTreeWriter.Write(", ");
                    _boundSyntaxTreeWriter.Write(parameter.Name + " : " + parameter.Type);
                    cntC++;

                }_boundSyntaxTreeWriter.WriteLine(") : "+ funco.Key.Type + " is");
                funco.Value.WriteTo(_boundSyntaxTreeWriter);
            }
            var statement = GetStatement();
            var evaluator = new Evaluator(builder, program.FunctionBodies, statement);
            var value = evaluator.Evaluate(function);
            _boundSyntaxTreeWriter.Close();
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