namespace compilers.CodeAnalysis
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxToken literalTiken)
        {
            LiteralToken = literalTiken;
        }
        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
        public SyntaxToken LiteralToken { get; }

        public override IEnumerable<SyntaxNode> getChildren()
        {
            yield return LiteralToken;
        }
    }
}