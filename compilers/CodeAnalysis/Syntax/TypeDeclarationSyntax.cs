namespace compilers.CodeAnalysis
{
    public sealed class TypeDeclarationSyntax : StatementSyntax
    {
        public TypeDeclarationSyntax(SyntaxToken typeKeyword, SyntaxToken name, SyntaxToken isKeyword, SyntaxNode representedType)
        {
            TypeKeyword = typeKeyword;
            Name = name;
            IsKeyword = isKeyword;
            RepresentedType = representedType;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeStatement;

        public SyntaxToken TypeKeyword { get; }
        public SyntaxToken Name { get; }
        public SyntaxToken IsKeyword { get; }
        public SyntaxNode RepresentedType { get; }
    }


}