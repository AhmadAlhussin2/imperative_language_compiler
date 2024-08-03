using System.Runtime.InteropServices;
using System.Text;
using ImperativeCompiler.CodeAnalysis;
using ImperativeCompiler.CodeAnalysis.IO;
using ImperativeCompiler.CodeAnalysis.Syntax;
using LLVMSharp.Interop;
namespace ImperativeCompiler;

internal abstract class Program
{
    private static readonly StreamWriter SyntaxTreeWriter = new StreamWriter("AST.txt");
    private static readonly StreamWriter BoundSyntaxTreeWriter = new StreamWriter("B_AST.txt");
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
        if (args.Length != 2 || args.First() is not ("file" or "text"))
        {
            Console.WriteLine("Usage: dotnet run [file|text] (file containing code or code)");
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
                Console.WriteLine($"{path} not found");
                return;
            }
        }
        else
        {
            text = args[1];
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
                    Console.Error.WriteDiagnostics(result.Diagnostics, syntaxTree);
                }
                else
                {
                    if (result.Value != null)
                    {
                        Console.WriteLine(result.Value);
                    }
                    compilation.WriteTree(Console.Out);

                    var error = StringToSBytePtr("");

                    LLVM.BuildRetVoid(builder);
                    LLVM.PrintModuleToFile(module, StringToSBytePtr("output.ll"), &error);

                    LLVM.DisposeBuilder(builder);
                    LLVM.DisposeModule(module);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            catch (Exception error)
            {
                Console.ResetColor();
                Console.WriteLine(error);
            }
        }
        SyntaxTreeWriter.Close();
        BoundSyntaxTreeWriter.Close();
    }
}