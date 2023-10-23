namespace compilers.CodeAnalysis
{
    public class ElseClauseSyntax : SyntaxNode
    {
        public ElseClauseSyntax(SyntaxToken elseKeyWord, StatementSyntax elseStatement)
        {
            ElseKeyWord = elseKeyWord;
            ElseStatement = elseStatement;
        }

        public override SyntaxKind Kind => SyntaxKind.ElseClause;

        public SyntaxToken ElseKeyWord { get; }
        public StatementSyntax ElseStatement { get; }
    }
}