using System.Collections.Immutable;
using compilers.CodeAnalysis.Binding;
using compilers.CodeAnalysis.Lowering;
using compilers.CodeAnalysis.Symbol;
using LLVMSharp.Interop;
using LLVMSharp;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
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
        static unsafe sbyte* StringToSBytePtr(string str)
        {
            // Convert the string to a byte array using UTF-8 encoding
            byte[] bytes = Encoding.UTF8.GetBytes(str + '\0');

            // Allocate unmanaged memory to hold the null-terminated string
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);

            // Copy the byte array to the allocated memory
            Marshal.Copy(bytes, 0, ptr, bytes.Length);

            // Return a pointer to the allocated memory
            return (sbyte*)ptr;
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
        public LLVMTypeRef TypeConverter(TypeSymbol type)
        {   unsafe 
            {
                if (type == TypeSymbol.Int)
                {
                    return LLVM.Int32Type();
                }
                else if (type == TypeSymbol.Real)
                {
                    return LLVM.DoubleType();
                } 
                else if (type == TypeSymbol.Bool)
                {
                    return LLVM.Int1Type();
                }
                else {
                    throw new Exception("Type Erorr");
                }
            }
        }
        public EvaluationResult Evaluate(LLVMBuilderRef builder, LLVMModuleRef module, LLVMValueRef function)
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
            Dictionary<FunctionSymbol, LLVMValueRef> LLVMfunctions = new();
            Dictionary<FunctionSymbol, LLVMTypeRef> LLVMfunctiontypes = new();
            foreach (var funco in program.FunctionBodies) unsafe 
            {
                var len = funco.Key.Parameters.Length;
                LLVMOpaqueType*[] paramTypes = new LLVMOpaqueType*[len];
                for (int i = 0 ; i < len ; i++)
                {
                    paramTypes[i] = TypeConverter(funco.Key.Parameters[i].Type);
                }
                LLVMTypeRef funcType;
                fixed (LLVMOpaqueType** ptr = paramTypes)
                {
                    funcType = LLVM.FunctionType(TypeConverter(funco.Key.Type),ptr, 1,0);
                }
                var thisfunction = LLVM.AddFunction(module, StringToSBytePtr(funco.Key.Name), funcType);
                var thisEntry = LLVM.AppendBasicBlock(thisfunction, StringToSBytePtr("entry"));
                var thisbuilder = LLVM.CreateBuilder();
                LLVM.PositionBuilderAtEnd(thisbuilder, thisEntry);
                var evalulator = new Evaluator(thisbuilder, program.FunctionBodies, LLVMfunctions, LLVMfunctiontypes, funco.Value);
                var locals = new Dictionary<VariableSymbol, object>();
                var LLVMlocals = new Dictionary<VariableSymbol, LLVMValueRef>();
                for (int i = 0; i < len; i++)
                {
                    var parameter = funco.Key.Parameters[i];
                    if( parameter.Type == TypeSymbol.Int || parameter.Type == TypeSymbol.Real)locals.Add(parameter, 0);
                    else locals.Add(parameter, false);

                    var LLVMparameter = LLVM.GetParam(thisfunction, Convert.ToUInt32(i));
                    LLVMlocals.Add(parameter, LLVMparameter);
                }
                evalulator._LLVMlocals.Push(LLVMlocals);
                evalulator._locals.Push(locals);
                var result = evalulator.Evaluate(thisfunction);
                evalulator._locals.Pop();
                evalulator._LLVMlocals.Pop();
                LLVMfunctions.Add(funco.Key, thisfunction);
                LLVMfunctiontypes.Add(funco.Key, funcType);
            }
            var statement = GetStatement();
            var evaluator = new Evaluator(builder, program.FunctionBodies, LLVMfunctions, LLVMfunctiontypes, statement);
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