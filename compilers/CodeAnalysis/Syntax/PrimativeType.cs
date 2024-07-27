namespace compilers.CodeAnalysis.Syntax
{
    public sealed class PrimativeType : TypeSyntax
    {

        public PrimativeType(SyntaxTree syntaxTree, SyntaxToken identifier) : base(syntaxTree)
        {
            Identifier = identifier;
        }

        public override SyntaxToken Identifier { get; }

        public override SyntaxKind Kind => SyntaxKind.PrimitiveType;

    }

}