using compilers.CodeAnalysis;
using compilers.CodeAnalysis.Symbol;

namespace compilers
{
    internal sealed class ProgrmamRepl : Repl
    {

        private Compilation? _previous;
        private readonly Dictionary<VariableSymbol, object> _variables = new();
        protected override void EvaluateCommand(string text, StreamReader reader, StreamWriter writer, StreamWriter syntaxTreeWriter, StreamWriter boundSyntaxTreeWriter, StreamWriter errorWriter)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            // syntaxTree.Root.WriteTo(syntaxTreeWriter);
            // if (syntaxTree.Diagnostics.Any())
            // {
            //     foreach (var error in syntaxTree.Diagnostics)
            //     {
            //         errorWriter.WriteLine(error);
            //     }
            // }
            
            var compilation = _previous == null ? new Compilation(syntaxTree) : _previous.continueWith(syntaxTree);

            Console.ForegroundColor = ConsoleColor.Green;

            // syntaxTree.Root.WriteTo(Console.Out);
            compilation.WriteTree(Console.Out);
            
            compilation.WriteTree(boundSyntaxTreeWriter);

            Console.ResetColor();

            
            var result = compilation.Evaluate(_variables);
            var diagnostics = result.Diagnostics;
            if (diagnostics.Any())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var diagnostic in diagnostics)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(diagnostic);
                    Console.ResetColor();
                    // var prefix = text.Substring(0, diagnostic.Span.Start);
                    // var error = text.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                    // var suffix = text.Substring(diagnostic.Span.Start + diagnostic.Span.Length);
                    Console.Write("    ");
                    // Console.Write(prefix);

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    // Console.Write(error);
                    Console.ResetColor();

                    // Console.WriteLine(suffix);
                    Console.WriteLine();
                }
                Console.ResetColor();
            }
            else
            {
                _previous = compilation;
                if(result.Value != null)
                {
                    Console.WriteLine(result.Value);
                }
                
            }
            
            
        }
    }


}

