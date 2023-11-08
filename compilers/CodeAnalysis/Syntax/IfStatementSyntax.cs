namespace compilers.CodeAnalysis
{
    public sealed class IfStatementSyntax : StatementSyntax
    {
        public IfStatementSyntax(SyntaxTree syntaxTree, SyntaxToken ifKeyword, ExpressionSyntax condition, SyntaxToken thenKeyword, StatementSyntax thenStatement, ElseClauseSyntax? elseClause, SyntaxToken endKeyword)
        : base(syntaxTree)
        {
            IfKeyword = ifKeyword;
            Condition = condition;
            ThenKeyword = thenKeyword;
            ThenStatement = thenStatement;
            ElseClause = elseClause;
            EndKeyword = endKeyword;
        }

        public override SyntaxKind Kind => SyntaxKind.IfStatement;

        public SyntaxToken IfKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public SyntaxToken ThenKeyword { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseClauseSyntax? ElseClause { get; }
        public SyntaxToken EndKeyword { get; }
    }
}