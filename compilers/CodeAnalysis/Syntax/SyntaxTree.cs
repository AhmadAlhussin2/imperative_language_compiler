using compilers.CodeAnalysis.Text;

namespace compilers.CodeAnalysis
{
    public sealed class SyntaxTree
    {
        private SyntaxTree(SourceText text)
        {
            var parser = new Parser(text);
            var root = parser.ParseCompilationUnit();
            Diagnostics = parser.Diagnostics.ToArray();
            Text = text;
            Root = root;
        }

        public SourceText Text { get; }
        public CompilationUnitSyntax Root { get; }
        public IReadOnlyList<Diagnostic> Diagnostics { get; }
        public static SyntaxTree Parse(string text)
        {
            var sourceText = SourceText.From(text);
            return Parse(sourceText);
        }
        public static SyntaxTree Parse(SourceText text)
        {
            return new SyntaxTree(text);
        }
        public static IEnumerable<SyntaxToken> ParseTokens(String text)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }
        public static IEnumerable<SyntaxToken> ParseTokens(SourceText text)
        {
            var lexer = new Lexer(text);
            while (true)
            {
                var token = lexer.NextToken();
                if (token.Kind == SyntaxKind.EOFToken)
                    break;
                yield return token;
            }
        }
    }
}