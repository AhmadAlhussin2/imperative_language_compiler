using System.CodeDom.Compiler;
using System.Collections.Immutable;
using compilers.CodeAnalysis;

namespace compilers.IO
{
    public static class TextWriterExtensions
    {
        public static bool IsConsoleOut(this TextWriter writer)
        {
            if (writer == Console.Out)
                return true;
            if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsoleOut())
                return true;
            return false;
        }
        public static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsoleOut())
                Console.ForegroundColor = color;
        }
        public static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsoleOut())
                Console.ResetColor();
        }
        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Blue);
            writer.Write(text);
            writer.ResetColor();
        }
        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Cyan);
            writer.Write(text);
            writer.ResetColor();
        }
        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Magenta);
            writer.Write(text);
            writer.ResetColor();
        }
        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.DarkGray);
            writer.Write(text);
            writer.ResetColor();
        }
        public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostic> diagnostics, SyntaxTree syntaxTree)
        {
            foreach (var diagnostic in diagnostics)
            {
                var lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                var line = syntaxTree.Text.Lines[lineIndex];
                var lineNumber = lineIndex + 1;
                var character = diagnostic.Span.Start - line.Start + 1;

                Console.WriteLine();


                Console.WriteLine($"({lineNumber},{character})");
                Console.WriteLine(diagnostic);

                var prefixSpan = TextSpan.FromBounds(line.Span.Start, diagnostic.Span.Start);
                var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);
                var prefix = syntaxTree.Text.ToString(prefixSpan);
                var error = syntaxTree.Text.ToString(diagnostic.Span);
                var suffix = syntaxTree.Text.ToString(suffixSpan);

                Console.Write("    ");
                Console.Write(prefix);
                Console.ForegroundColor = ConsoleColor.DarkRed;

                Console.Write(error);
                Console.ResetColor();

                Console.WriteLine(suffix);

                Console.WriteLine();
            }
        }
    }
}
