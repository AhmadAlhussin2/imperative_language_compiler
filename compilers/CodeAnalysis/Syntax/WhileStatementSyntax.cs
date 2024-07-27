namespace compilers.CodeAnalysis.Syntax
{
    public sealed class WhileStatementSyntax : StatementSyntax
    {

        public WhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken whileKeyword, ExpressionSyntax condition, SyntaxToken loopKeyword, StatementSyntax body, SyntaxToken endKeyword)
        : base(syntaxTree)
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