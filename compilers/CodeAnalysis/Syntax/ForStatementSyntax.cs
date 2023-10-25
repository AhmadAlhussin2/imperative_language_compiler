namespace compilers.CodeAnalysis
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax(SyntaxToken forKeyword, SyntaxToken identifier, SyntaxToken inKeyword, ExpressionSyntax lowerBound,
         SyntaxToken rangeToken, ExpressionSyntax upperBound, SyntaxToken loopKeyword, StatementSyntax body, SyntaxToken endKeyword)
        {
            ForKeyword = forKeyword;
            Identifier = identifier;
            InKeyword = inKeyword;
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
        public ExpressionSyntax LowerBound { get; }
        public SyntaxToken RangeToken { get; }
        public ExpressionSyntax UpperBound { get; }
        public SyntaxToken LoopKeyword { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken EndKeyword { get; }
    }

    public sealed class TypeDeclarationSyntax : StatementSyntax
    {
        public TypeDeclarationSyntax(SyntaxToken typeKeyword, SyntaxToken name, SyntaxToken isKeyword, SyntaxNode representedType)
        {
            TypeKeyword = typeKeyword;
            Name = name;
            IsKeyword = isKeyword;
            RepresentedType = representedType;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeStatement;

        public SyntaxToken TypeKeyword { get; }
        public SyntaxToken Name { get; }
        public SyntaxToken IsKeyword { get; }
        public SyntaxNode RepresentedType { get; }
    }

    
}