namespace compilers.CodeAnalysis
{
    public sealed class NameExpressionSyntax : ExpressionSyntax
    {
        public NameExpressionSyntax(SyntaxToken identifierToken, SyntaxToken? dotToken, ExpressionSyntax? nxt)
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