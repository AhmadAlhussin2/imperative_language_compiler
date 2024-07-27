namespace compilers.CodeAnalysis.Syntax
{
    public sealed class RecordDeclarationSyntax : StatementSyntax
    {
        public RecordDeclarationSyntax(
            SyntaxTree syntaxTree,
            SyntaxToken recordKeyword,
            SeparatedSyntaxList<ParameterSyntax> parameters,
            SyntaxToken endKeyword) : base(syntaxTree)
        {
            RecordKeyword = recordKeyword;
            Parameters = parameters;
            EndKeyword = endKeyword;
        }

        public override SyntaxKind Kind => SyntaxKind.RecordDeclaration;

        public SyntaxToken RecordKeyword { get; }

        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

        public SyntaxToken EndKeyword { get; }
    }
    /*
    public sealed class ArrayDeclarationSyntax : StatementSyntax
    {
        /*
        public ArrayDeclarationSyntax(
            SyntaxToken arrayKeyword,
            SyntaxToken openBracket,
            ExpressionSyntax size,
            SyntaxToken closeBracket
            )
        {
            ArrayKeyword = arrayKeyword;
            OpenBracket = openBracket;
            Size = size;
            CloseBracket = closeBracket;
        }

        public override SyntaxKind Kind => SyntaxKind.ArrayDeclaration;

        public SyntaxToken ArrayKeyword { get; }
        public SyntaxToken OpenBracket { get; }
        public ExpressionSyntax Size { get; }
        public SyntaxToken CloseBracket { get; }
        
    }
    */



}