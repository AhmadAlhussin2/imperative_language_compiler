namespace compilers.CodeAnalysis
{
    public sealed class FunctionDeclerationSyntax : MemberSyntax
    {
        public FunctionDeclerationSyntax(
            SyntaxToken routineKeyword, SyntaxToken identifier,
            SyntaxToken openParenthesisToken, SeparatedSyntaxList<ParameterSyntax> parameters,
            SyntaxToken closeParenthesisToken, TypeClauseSyntax? typeClause, SyntaxToken isKeyword,
            BlockStatementSyntax body, SyntaxToken endKeyword)
        {
            RoutineKeyword = routineKeyword;
            Identifier = identifier;
            OpenParenthesisToken = openParenthesisToken;
            Parameters = parameters;
            CloseParenthesisToken = closeParenthesisToken;
            TypeClause = typeClause;
            IsKeyword = isKeyword;
            Body = body;
            EndKeyword = endKeyword;
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionDecleration;

        public SyntaxToken RoutineKeyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public TypeClauseSyntax? TypeClause { get; }
        public SyntaxToken IsKeyword { get; }
        public BlockStatementSyntax Body { get; }
        public SyntaxToken EndKeyword { get; }
    }


    
}