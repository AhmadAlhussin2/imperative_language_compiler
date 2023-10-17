namespace compilers.CodeAnalysis
{
    public sealed class SyntaxTree
    {
        public SyntaxTree(IEnumerable<Diagnostic> diagnostics, ExpressionSyntax root, SyntaxToken endOfFileToken)
        {
            Diagnostics = diagnostics.ToArray();
            EndOfFileToken = endOfFileToken;
            Root = root;
        }

        public ExpressionSyntax Root { get; }
        public SyntaxToken EndOfFileToken { get; }
        public IReadOnlyList<Diagnostic> Diagnostics { get; }
        public static SyntaxTree Parse(String text)
        {
            var parser = new Parser(text);
            return parser.Parse();
        }
    }
}