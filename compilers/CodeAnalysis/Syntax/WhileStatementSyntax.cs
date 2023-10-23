namespace compilers.CodeAnalysis
{
    public sealed class WhileStatementSyntax : StatementSyntax
    {

        public WhileStatementSyntax(SyntaxToken whileKeyword, ExpressionSyntax condition, SyntaxToken loopKeyword, StatementSyntax body, SyntaxToken endKeyword)
        {
            WhileKeyword = whileKeyword;
            Condition = condition;
            LoopKeyword = loopKeyword;
            Body = body;
            EndKeyword = endKeyword;
        }

        public override SyntaxKind Kind => SyntaxKind.WhileStatement;

        public SyntaxToken WhileKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public SyntaxToken LoopKeyword { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken EndKeyword { get; }
    }
}