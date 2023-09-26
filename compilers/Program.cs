using System;
using compilers.CodeAnalysis;

namespace compilers
{
    class Program
    {
        static void Main(String[] args)
        {
            while (true)
            {
                var line = Console.ReadLine();

                // var syntaxTree = SyntaxTree.Parse(line!);

                // Console.ForegroundColor = ConsoleColor.Green;

                // PrintTree(syntaxTree.Root);

                // Console.ResetColor();

                // if (syntaxTree.Errors.Any())
                // {
                //     Console.ForegroundColor = ConsoleColor.Red;
                //     foreach (var error in syntaxTree.Errors)
                //     {
                //         Console.WriteLine(error);
                //     }
                //     Console.ResetColor();
                // }
                // else
                // {
                //     var e = new Evaluator(syntaxTree.Root);
                //     var res = e.Evaluate();
                //     Console.WriteLine(res);
                // }

                var lexer = new Lexer(line!);
                while (true)
                {
                    var token = lexer.NextToken();
                    if (token.Kind == SyntaxKind.EOFToken)
                        break;
                    Console.Write($"{token.Kind}: '{token.Text}'");
                    if (token.Value != null)
                        Console.Write($" value : '{token.Value}'");

                    Console.WriteLine();
                }
            }
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

