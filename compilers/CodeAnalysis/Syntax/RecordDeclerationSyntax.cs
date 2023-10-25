namespace compilers.CodeAnalysis
{
    public sealed class RecordDeclerationSyntax : StatementSyntax
    {
        public RecordDeclerationSyntax(
            SyntaxToken recordKeyword,
            SeparatedSyntaxList<ParameterSyntax> parameters,
            SyntaxToken endKeyword)
        {
            RecordKeyword = recordKeyword;
            Parameters = parameters;
            EndKeyword = endKeyword;
        }

        public override SyntaxKind Kind => SyntaxKind.RecordDecleration;

        public SyntaxToken RecordKeyword { get; }
        
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
        
        public SyntaxToken EndKeyword { get; }
    }


    
}