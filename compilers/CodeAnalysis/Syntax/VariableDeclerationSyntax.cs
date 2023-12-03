namespace compilers.CodeAnalysis
{
    public sealed class VariableDeclerationSyntax : StatementSyntax
    {
        public VariableDeclerationSyntax(SyntaxTree syntaxTree, SyntaxToken varKeyword, SyntaxToken identifier, TypeSyntax? typeClause, SyntaxToken isToken, ExpressionSyntax initializer)
        : base(syntaxTree)
        {
            VarKeyword = varKeyword;
            Identifier = identifier;
            TypeClause = typeClause;
            IsToken = isToken;
            Initializer = initializer;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableDecleration;

        public SyntaxToken VarKeyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeSyntax? TypeClause { get; }
        public SyntaxToken IsToken { get; }
        public ExpressionSyntax Initializer { get; }
    }
}