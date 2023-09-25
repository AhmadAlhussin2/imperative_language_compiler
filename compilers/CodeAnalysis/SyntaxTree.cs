namespace compilers.CodeAnalysis
{
    public sealed class SyntaxTree
    {
        public ExpressionSyntax Root { get; }
        public SyntaxToken EndOfFileToken { get; }
        public IReadOnlyList<string> Errors { get; }
        public SyntaxTree(IEnumerable<string> errors, ExpressionSyntax root, SyntaxToken endOfFileToken)
        {
            Errors = errors.ToArray();
            EndOfFileToken = endOfFileToken;
            Root = root;
        }
        public static SyntaxTree Parse(String text)
        {
            var parser = new Parser(text);
            return parser.Parse();
        }
    }
}