using System.Collections.Immutable;

namespace compilers.CodeAnalysis
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {

        public CompilationUnitSyntax(ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken)
        {
            EndOfFileToken = endOfFileToken;
            Members = members;

        }
        public ImmutableArray<MemberSyntax> Members { get; }
        public SyntaxToken EndOfFileToken { get; }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    }
}