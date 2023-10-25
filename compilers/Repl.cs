using System.Text;
using compilers.CodeAnalysis;

namespace compilers
{
    internal abstract class Repl
    {
        private readonly StringBuilder _textBuilder = new();
        private readonly StreamReader _reader = new("source.txt");
        private readonly StreamWriter _writer = new("output.txt");
        private readonly StreamWriter _syntaxTreeWriter = new("AST.txt");
        private readonly StreamWriter _boundSyntaxTreeWriter = new("B_AST.txt");
        private readonly StreamWriter _errorWriter = new("Errors.txt");

        public void Run()
        {
            while (true)
            {
                var line = _reader.ReadLine();
                if (line == null)
                    break;
                _textBuilder.AppendLine(line);
                var text = _textBuilder.ToString();
                if (!IsCompleteInstruciton(text))
                    continue;
                EvaluateCommand(text, _reader, _writer, _syntaxTreeWriter, _boundSyntaxTreeWriter, _errorWriter);
                _textBuilder.Clear();
            }
            if (!string.IsNullOrEmpty(_textBuilder.ToString()))
                PrintErrors(_textBuilder.ToString());
            _reader.Close();
            _writer.Close();
            _syntaxTreeWriter.Close();
            _boundSyntaxTreeWriter.Close();
            _errorWriter.Close();
        }

        protected abstract void EvaluateCommand(string text, StreamReader reader, StreamWriter writer, StreamWriter syntaxTreeWriter, StreamWriter boundSyntaxTreeWriter, StreamWriter errorWriter);

        protected void PrintErrors(string text)
        {
            EvaluateCommand(text, _reader, _writer, _syntaxTreeWriter, _boundSyntaxTreeWriter, _errorWriter);
        }
        protected bool IsCompleteInstruciton(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            var syntaxTree = SyntaxTree.Parse(text);
            return !syntaxTree.Root.Members.Last().GetLastToken().IsMissing;
        }
    }


}

