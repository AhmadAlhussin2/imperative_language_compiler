namespace compilers.CodeAnalysis.Syntax
{
    public sealed class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, TypeSyntax type)
        : base(syntaxTree)
        {
            Identifier = identifier;
            Type = type;
        }

        public override SyntaxKind Kind => SyntaxKind.Parameter;

        public SyntaxToken Identifier { get; }
        public TypeSyntax Type { get; }
    }
}