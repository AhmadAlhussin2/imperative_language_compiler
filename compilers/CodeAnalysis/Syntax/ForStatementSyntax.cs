namespace compilers.CodeAnalysis
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax(SyntaxTree syntaxTree, SyntaxToken forKeyword, SyntaxToken identifier, SyntaxToken inKeyword,SyntaxToken? reverseKeyword,
         ExpressionSyntax lowerBound,SyntaxToken rangeToken, ExpressionSyntax upperBound, SyntaxToken loopKeyword, StatementSyntax body,
          SyntaxToken endKeyword)
         : base(syntaxTree)
        {
            ForKeyword = forKeyword;
            Identifier = identifier;
            InKeyword = inKeyword;
            ReverseKeyword = reverseKeyword;
            LowerBound = lowerBound;
            RangeToken = rangeToken;
            UpperBound = upperBound;
            LoopKeyword = loopKeyword;
            Body = body;
            EndKeyword = endKeyword;
        }

        public override SyntaxKind Kind => SyntaxKind.ForStatement;

        public SyntaxToken ForKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken InKeyword { get; }
        public SyntaxToken? ReverseKeyword { get; private set; }
        public ExpressionSyntax LowerBound { get; }
        public SyntaxToken RangeToken { get; }
        public ExpressionSyntax UpperBound { get; }
        public SyntaxToken LoopKeyword { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken EndKeyword { get; }
    }


}