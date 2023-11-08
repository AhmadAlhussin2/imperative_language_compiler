namespace compilers.CodeAnalysis
{
    public sealed class NameExpressionSyntax : ExpressionSyntax
    {
        public NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken, SyntaxToken? dotToken, ExpressionSyntax? nxt)
        : base(syntaxTree)
        {
            IdentifierToken = identifierToken;
            DotToken = dotToken;
            Nxt = nxt;
        }
        public override SyntaxKind Kind => SyntaxKind.NameExpression;

        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken? DotToken { get; }
        public ExpressionSyntax? Nxt { get; }

    }
}