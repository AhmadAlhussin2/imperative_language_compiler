namespace compilers.CodeAnalysis
{
    public abstract class MemberSyntax : SyntaxNode
    {
        protected MemberSyntax(SyntaxTree syntaxTree) : base(syntaxTree)
        {
        }
    }
}