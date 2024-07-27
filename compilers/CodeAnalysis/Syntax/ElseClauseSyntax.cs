namespace compilers.CodeAnalysis.Syntax
{
    public class ElseClauseSyntax : SyntaxNode
    {
        public ElseClauseSyntax(SyntaxTree syntaxTree, SyntaxToken elseKeyWord, StatementSyntax elseStatement)
        : base(syntaxTree)
        {
            ElseKeyWord = elseKeyWord;
            ElseStatement = elseStatement;
        }

        public override SyntaxKind Kind => SyntaxKind.ElseClause;

        public SyntaxToken ElseKeyWord { get; }
        public StatementSyntax ElseStatement { get; }
    }
}