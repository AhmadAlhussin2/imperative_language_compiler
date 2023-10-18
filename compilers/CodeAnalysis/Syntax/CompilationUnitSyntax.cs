namespace compilers.CodeAnalysis
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {

        public CompilationUnitSyntax(ExpressionSyntax expression, SyntaxToken endOfFileToken)
        {
            EndOfFileToken = endOfFileToken;
            Expression = expression;

        }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken EndOfFileToken { get; }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    }
}