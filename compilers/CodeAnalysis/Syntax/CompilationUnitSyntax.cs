using System.Collections.Immutable;

namespace compilers.CodeAnalysis
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {

        public CompilationUnitSyntax(SyntaxTree syntaxTree, ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken)
        : base(syntaxTree)
        {
            EndOfFileToken = endOfFileToken;
            Members = members;

        }
        public ImmutableArray<MemberSyntax> Members { get; }
        public SyntaxToken EndOfFileToken { get; }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    }
}