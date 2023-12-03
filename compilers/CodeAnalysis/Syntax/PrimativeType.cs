using compilers.CodeAnalysis.Symbol;

namespace compilers.CodeAnalysis
{
    public sealed class PrimativeType : TypeSyntax
    {
        private SyntaxToken _identifier;

        public PrimativeType(SyntaxTree syntaxTree, SyntaxToken identifier) : base(syntaxTree)
        {
            _identifier = identifier;
        }

        public override SyntaxToken Identifier => _identifier;

        public override SyntaxKind Kind => SyntaxKind.PrimativeType;

    }

}