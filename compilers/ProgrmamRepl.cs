using compilers.CodeAnalysis;
using compilers.CodeAnalysis.Symbol;
using compilers.IO;

namespace compilers
{
    internal sealed class ProgrmamRepl : Repl
    {

        private Compilation? _previous;
        private readonly Dictionary<VariableSymbol, object> _variables = new();
        protected override void EvaluateCommand(string text, StreamReader reader, StreamWriter writer, StreamWriter syntaxTreeWriter, StreamWriter boundSyntaxTreeWriter, StreamWriter errorWriter)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            syntaxTree.Root.WriteTo(syntaxTreeWriter);

            //var compilation = _previous == null ? new Compilation(syntaxTree) : _previous.ContinueWith(syntaxTree);

            if (syntaxTree.Diagnostics.Any())
            {
                foreach (var error in syntaxTree.Diagnostics)
                {
                    errorWriter.WriteLine(error);
                }
            }
            
            var compilation = _previous == null ? new Compilation(syntaxTree) : _previous.ContinueWith(syntaxTree);


            Console.ForegroundColor = ConsoleColor.Green;

            //syntaxTree.Root.WriteTo(Console.Out);
            compilation.WriteTree(Console.Out);

            compilation.WriteTree(boundSyntaxTreeWriter);

            Console.ResetColor();


            var result = compilation.Evaluate(_variables);
            var diagnostics = result.Diagnostics;
            if (diagnostics.Any())
            {
                Console.Error.WriteDiagnostics(result.Diagnostics, syntaxTree);
            }
            else
            {
                _previous = compilation;
                if (result.Value != null)
                {
                    Console.WriteLine(result.Value);
                }
            }

            

        }
    }
}

