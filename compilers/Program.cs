using System;
using compilers.CodeAnalysis;
using compilers.CodeAnalysis.Binding;

namespace compilers
{
    class Program
    {
        static void Main(String[] args)
        {


            StreamReader reader = new StreamReader("source.txt");
            StreamWriter writer = new StreamWriter("output.txt");

            List<string> errors = new List<string>();
            int LineCounter = 0;
            var variables = new Dictionary<VariableSymbol, object>();
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                LineCounter = LineCounter + 1;

                var syntaxTree = SyntaxTree.Parse(line!);

                Console.ForegroundColor = ConsoleColor.Green;

                PrintTree(syntaxTree.Root);

                Console.ResetColor();

                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate(variables);
                var diagnostics = result.Diagnostics;
                if (diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach (var diagnostic in diagnostics)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(diagnostic);
                        Console.ResetColor();
                        var prefix = line.Substring(0, diagnostic.Span.Start);
                        var error = line.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                        var suffix = line.Substring(diagnostic.Span.Start+ diagnostic.Span.Length);
                        Console.Write("    "); 
                        Console.Write(prefix);

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(error);
                        Console.ResetColor();

                        Console.WriteLine(suffix);
                        Console.WriteLine();
                    }
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(result.Value);
                }
            
                // var lexer = new Lexer(line!);
                // while (true)
                // {
                //     var token = lexer.NextToken();
                //     if (token.Kind == SyntaxKind.EOFToken)
                //         break;
                //     writer.Write($"{token.Kind}: '{token.Text}'");
                //     if (token.Value != null)
                //         writer.Write($" value : '{token.Value}'");

                //     writer.WriteLine();
                //     foreach (string error in lexer.ViewErrors())
                //     {
                //         errors.Add($"Erorr in line {LineCounter}: " + error);
                //     }
                // }


            } 
            if (errors.Count > 0)
            {
                writer.WriteLine("Erros:");
                foreach (string error in errors)
                {
                    writer.WriteLine(error);
                }
            }

            reader.Close();
            writer.Close();
        }
        static void PrintTree(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";
            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);
            if (node is SyntaxToken t && t.Value != null)
            {
                Console.Write($" {t.Value}");
            }
            Console.WriteLine();
            indent += isLast ? "   " : "│  ";
            var lastChild = node.getChildren().LastOrDefault();
            foreach (var child in node.getChildren())
            {
                PrintTree(child, indent, child == lastChild);
            }
        }
    }


}

