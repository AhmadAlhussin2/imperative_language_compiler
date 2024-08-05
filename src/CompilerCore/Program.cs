using System.Runtime.InteropServices;
using System.Text;
using ImperativeCompiler.CodeAnalysis;
using ImperativeCompiler.CodeAnalysis.IO;
using ImperativeCompiler.CodeAnalysis.Syntax;
using LLVMSharp.Interop;
namespace ImperativeCompiler;

internal abstract class Program
{
    private static readonly StreamWriter SyntaxTreeWriter = new("AST.txt");
    private static readonly StreamWriter BoundSyntaxTreeWriter = new("B_AST.txt");
    private static readonly StreamWriter ErrorFileStream = new("ERROR_LOG.txt");
    private static readonly StreamWriter OutputFileStream = new("output.txt");

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


    private static void Main(string[] args)
    {
        var reporter = new Reporter();
        if (args.Length < 2 || args.First() is not ("file" or "text"))
        {
            ErrorFileStream.WriteLineAsync("Usage: dotnet run [file|text] (file containing code or code)");
            ErrorFileStream.Close();
            reporter.ReportFailure();
            return;
        }

        string text;
        if (args.First() == "file")
        {
            var fileName = args[1];
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            try
            {
                text = File.ReadAllText(path);
            }
            catch (FileNotFoundException)
            {
                ErrorFileStream.WriteLine($"{path} not found");
                ErrorFileStream.Close();
                reporter.ReportFailure();
                return;
            }
        }
        else
        {
            var sb = new StringBuilder();
            for (var i = 1; i < args.Length; i++)
            {
                sb.Append(args[i]);
                sb.Append(" ");
            }
            text = sb.ToString();
        }

        unsafe
        {
            try
            {
                var module = LLVM.ModuleCreateWithName(StringToSBytePtr("MyModule"));
                var builder = LLVM.CreateBuilder();
                var functionType = LLVM.FunctionType(LLVM.VoidType(), null, 0, 0);
                var mainFunction = LLVM.AddFunction(module, StringToSBytePtr("main"), functionType);
                var entryBlock = LLVM.AppendBasicBlock(mainFunction, StringToSBytePtr("entry"));
                LLVM.PositionBuilderAtEnd(builder, entryBlock);
                var syntaxTree = SyntaxTree.Parse(text);
                syntaxTree.Root.WriteTo(SyntaxTreeWriter);
                var compilation = new Compilation(syntaxTree);
                compilation.WriteTree(BoundSyntaxTreeWriter);
                var result = compilation.Evaluate(builder, module, mainFunction);
                if (result.Diagnostics.Any())
                {
                    ErrorFileStream.WriteDiagnostics(result.Diagnostics, syntaxTree);
                    ErrorFileStream.Close();
                    reporter.ReportFailure();
                    return;
                }
                if (result.Value != null)
                {
                    OutputFileStream.WriteLine(result.Value);
                    OutputFileStream.Close();
                }

                var error = StringToSBytePtr("");

                LLVM.BuildRetVoid(builder);
                LLVM.PrintModuleToFile(module, StringToSBytePtr("output.ll"), &error);

                LLVM.DisposeBuilder(builder);
                LLVM.DisposeModule(module);
            }
            catch (Exception)
            {
                ErrorFileStream.WriteLine("Unknown error");
                ErrorFileStream.Close();
                reporter.ReportFailure();
                return;
            }
        }
        SyntaxTreeWriter.Close();
        BoundSyntaxTreeWriter.Close();
        reporter.ReportSuccess();
    }
}