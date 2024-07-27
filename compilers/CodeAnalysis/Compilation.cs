using System.Collections.Immutable;
using compilers.CodeAnalysis.Binding;
using compilers.CodeAnalysis.Lowering;
using LLVMSharp.Interop;
using System.Text;
using System.Runtime.InteropServices;
using compilers.CodeAnalysis.Symbols;
using compilers.CodeAnalysis.Syntax;
namespace compilers.CodeAnalysis;

public sealed class Compilation
{
    private static readonly StreamWriter BoundSyntaxTreeWriter = new("B_AST(functions).txt");
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

    private Compilation? Previous { get; }
    private SyntaxTree Syntax { get; }
    private BoundGlobalScope GlobalScope
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
    
    private LLVMTypeRef TypeConverter(TypeSymbol type)
    {
        unsafe
        {
            if (type == TypeSymbol.Int)
            {
                return LLVM.Int32Type();
            }
            if (type == TypeSymbol.Real)
            {
                return LLVM.DoubleType();
            }
            if (type == TypeSymbol.Bool)
            {
                return LLVM.Int1Type();
            }
            throw new Exception("Type Error");
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
            return new EvaluationResult([..program.Diagnostics], null);
        }
        foreach (var func in program.FunctionBodies)
        {
            BoundSyntaxTreeWriter.Write("routine " + func.Key.Name + " (");
            int cntC = 0;
            foreach (var parameter in func.Key.Parameters)
            {
                if (cntC > 0) BoundSyntaxTreeWriter.Write(", ");
                BoundSyntaxTreeWriter.Write(parameter.Name + " : " + parameter.Type);
                cntC++;

            }
            BoundSyntaxTreeWriter.WriteLine(") : " + func.Key.Type + " is");
            func.Value.WriteTo(BoundSyntaxTreeWriter);
        }
        var llvmFunctions = new Dictionary<FunctionSymbol, LLVMValueRef>();
        var llvmFunctionTypes = new Dictionary<FunctionSymbol, LLVMTypeRef>();
        foreach (var func in program.FunctionBodies) unsafe
        {
            var len = func.Key.Parameters.Length;
            var paramTypes = new LLVMOpaqueType*[len];
            for (int i = 0; i < len; i++)
            {
                paramTypes[i] = TypeConverter(func.Key.Parameters[i].Type);
            }
            LLVMTypeRef funcType;
            fixed (LLVMOpaqueType** ptr = paramTypes)
            {
                funcType = LLVM.FunctionType(TypeConverter(func.Key.Type), ptr, 1, 0);
            }
            var thisFunction = LLVM.AddFunction(module, StringToSBytePtr(func.Key.Name), funcType);
            var thisEntry = LLVM.AppendBasicBlock(thisFunction, StringToSBytePtr("entry"));
            var thisBuilder = LLVM.CreateBuilder();
            LLVM.PositionBuilderAtEnd(thisBuilder, thisEntry);
            var evalulator = new Evaluator(thisBuilder, program.FunctionBodies, llvmFunctions, llvmFunctionTypes, func.Value);
            var locals = new Dictionary<VariableSymbol, object>();
            var llvmLocals = new Dictionary<VariableSymbol, LLVMValueRef>();
            for (var i = 0; i < len; i++)
            {
                var parameter = func.Key.Parameters[i];
                if (parameter.Type == TypeSymbol.Int || parameter.Type == TypeSymbol.Real) locals.Add(parameter, 0);
                else locals.Add(parameter, false);

                var llvmParameter = LLVM.GetParam(thisFunction, Convert.ToUInt32(i));
                llvmLocals.Add(parameter, llvmParameter);
            }
            evalulator.LlvmLocals.Push(llvmLocals);
            evalulator.Locals.Push(locals); 
            evalulator.Evaluate(thisFunction);
            evalulator.Locals.Pop();
            evalulator.LlvmLocals.Pop();
            llvmFunctions.Add(func.Key, thisFunction);
            llvmFunctionTypes.Add(func.Key, funcType);
        }
        var statement = GetStatement();
        var evaluator = new Evaluator(builder, program.FunctionBodies, llvmFunctions, llvmFunctionTypes, statement);
        var value = evaluator.Evaluate(function);
        BoundSyntaxTreeWriter.Close();
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