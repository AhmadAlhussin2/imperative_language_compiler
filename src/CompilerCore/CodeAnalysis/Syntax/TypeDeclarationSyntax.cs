namespace ImperativeCompiler.CodeAnalysis.Syntax;

public sealed class TypeDeclarationSyntax : StatementSyntax
{
    public TypeDeclarationSyntax(SyntaxTree syntaxTree,SyntaxToken typeKeyword, SyntaxToken name, SyntaxToken isKeyword, SyntaxNode representedType)
    :base(syntaxTree)
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