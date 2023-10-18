namespace compilers.CodeAnalysis
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {

        public CompilationUnitSyntax(StatementSyntax statement, SyntaxToken endOfFileToken)
        {
            EndOfFileToken = endOfFileToken;
            Statement = statement;

        }
        public StatementSyntax Statement { get; }
        public SyntaxToken EndOfFileToken { get; }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    }
}