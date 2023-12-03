namespace compilers.CodeAnalysis
{
    public abstract class TypeSyntax : SyntaxNode
    {
        public TypeSyntax(SyntaxTree syntaxTree)
        : base(syntaxTree)
        {
        }

        public abstract SyntaxToken Identifier { get; }

    }

}