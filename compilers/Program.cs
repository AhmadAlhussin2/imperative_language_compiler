using System.Runtime.InteropServices;
using System.Text;
using compilers.CodeAnalysis;
using compilers.CodeAnalysis.Symbol;
using compilers.IO;
using LLVMSharp.Interop;

namespace compilers
{

    internal class Program
    {
        static private readonly StreamWriter _syntaxTreeWriter = new("AST.txt");
        static private readonly StreamWriter _boundSyntaxTreeWriter = new("B_AST.txt");
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
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run `path to your program`");
            }
            else if (args.Length == 1) unsafe
            {
                var path = args.Single();
                try
                {
                    var module = LLVM.ModuleCreateWithName(StringToSBytePtr("MyModule"));
                    var builder = LLVM.CreateBuilder();
                    
                    /*
                    -- References --
                    Declare function
                    LLVMOpaqueType*[] paramTypes = {LLVM.Int32Type()};
                    LLVMTypeRef doubleType;
                    fixed (LLVMOpaqueType** ptr = paramTypes)
                    {
                        doubleType = LLVM.FunctionType(LLVM.Int32Type(),ptr, 1,0);
                    }
                    var doubleFunction = LLVM.AddFunction(module, StringToSBytePtr("func"), doubleType);
                    var doubleEntry = LLVM.AppendBasicBlock(doubleFunction, StringToSBytePtr("entry"));
                    var doubleBuilder = LLVM.CreateBuilder();
                    LLVM.PositionBuilderAtEnd(doubleBuilder, doubleEntry);
                    var param = LLVM.GetParam(doubleFunction, 0);
                    var paramVal = LLVM.BuildLoad2(doubleBuilder, LLVM.Int32Type(), param, StringToSBytePtr("load"));
                    LLVM.BuildRet(doubleBuilder, paramVal);
                    */
                    
                    
                    var functionType = LLVM.FunctionType(LLVM.VoidType(), null, 0, 0);
                    var mainFunction = LLVM.AddFunction(module, StringToSBytePtr("main"), functionType);
                    var entryBlock = LLVM.AppendBasicBlock(mainFunction, StringToSBytePtr("entry"));
                    LLVM.PositionBuilderAtEnd(builder, entryBlock);
                    
                    /*
                    -- Refereces -- 
                    Call function and store value in new variable
                    LLVMOpaqueValue*[] paramValues = {LLVM.ConstInt(LLVM.Int32Type(), 1, 0)};
                    LLVMValueRef callDouble;
                    fixed (LLVMOpaqueValue** ptr = paramValues)
                    {
                        callDouble = LLVM.BuildCall2(builder, doubleType, doubleFunction, ptr, 1, StringToSBytePtr("call"));
                    }
                    var x = LLVM.BuildAlloca(builder, LLVM.Int32Type(), StringToSBytePtr("x"));
                    LLVM.BuildStore(builder, callDouble, x);
                
                    
                    int arraySize = 10;
                    var arrayType = LLVM.ArrayType(LLVM.Int32Type(), (uint)arraySize);
                    var arrayAlloca = LLVM.BuildAlloca(builder, arrayType,  StringToSBytePtr("myArray"));

                    var index = LLVM.ConstInt(LLVM.Int32Type(), 2, 0); // Index 2, change as needed
                    var lst = new LLVMOpaqueValue*[] { index };
                    LLVMOpaqueValue** indecies;
                    fixed (LLVMOpaqueValue** ptr=lst)
                    {
                        indecies = ptr;
                    } 
                    LLVMValueRef elementPtr = LLVM.BuildGEP2(builder, arrayType, arrayAlloca, indecies, (uint)1,StringToSBytePtr("load"));
                    LLVMValueRef valueToStore = LLVM.ConstInt(LLVM.Int32Type(), 42, 0); // Value to store, change as needed
                    LLVM.BuildStore(builder, valueToStore, elementPtr);

                    var load = LLVM.BuildLoad2(builder,LLVM.Int32Type(), elementPtr,StringToSBytePtr("zip"));
                    */
                    

                    var text = File.ReadAllText(path);
                    var syntaxTree = SyntaxTree.Parse(text);
                    syntaxTree.Root.WriteTo(_syntaxTreeWriter);
                    var compilation = new Compilation(syntaxTree);
                    compilation.WriteTree(_boundSyntaxTreeWriter);
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
                    return ;
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"{path} not found");
                }
                catch (Exception error)
                {
                    Console.ResetColor();
                    Console.WriteLine(error);
                }
            }
            _syntaxTreeWriter.Close();
            _boundSyntaxTreeWriter.Close();
        }
    }


}

