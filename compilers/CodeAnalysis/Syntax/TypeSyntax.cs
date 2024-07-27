namespace compilers.CodeAnalysis.Syntax
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