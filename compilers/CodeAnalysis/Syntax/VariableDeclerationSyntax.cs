namespace compilers.CodeAnalysis
{
    public sealed class VariableDeclerationSyntax : StatementSyntax
    {
        public VariableDeclerationSyntax(SyntaxToken varKeyword, SyntaxToken identifier, SyntaxToken isToken, ExpressionSyntax initializer)
        {
            VarKeyword = varKeyword;
            Identifier = identifier;
            IsToken = isToken;
            Initializer = initializer;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableDecleration;

        public SyntaxToken VarKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken IsToken { get; }
        public ExpressionSyntax Initializer { get; }
    }
}