using compilers.CodeAnalysis;
using compilers.CodeAnalysis.Symbol;
using compilers.IO;

namespace compilers
{

    internal class Program
    {
        static private readonly StreamWriter _syntaxTreeWriter = new("AST.txt");
        static private readonly StreamWriter _boundSyntaxTreeWriter = new("B_AST.txt");
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run `path to your program`");
            }
            else if (args.Length == 1)
            {
                var path = args.Single();
                try
                {
                    var text = File.ReadAllText(path);
                    var syntaxTree = SyntaxTree.Parse(text);
                    syntaxTree.Root.WriteTo(_syntaxTreeWriter);
                    var compilation = new Compilation(syntaxTree);
                    compilation.WriteTree(_boundSyntaxTreeWriter);
                    var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());
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

