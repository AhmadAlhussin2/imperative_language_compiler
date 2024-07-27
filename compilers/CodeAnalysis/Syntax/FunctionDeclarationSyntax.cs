namespace compilers.CodeAnalysis.Syntax
{
    public sealed class FunctionDeclarationSyntax : MemberSyntax
    {
        public FunctionDeclarationSyntax(
            SyntaxTree syntaxTree,
            SyntaxToken routineKeyword, SyntaxToken identifier,
            SyntaxToken openParenthesisToken, SeparatedSyntaxList<ParameterSyntax> parameters,
            SyntaxToken closeParenthesisToken, TypeSyntax? typeClause, SyntaxToken isKeyword,
            BlockStatementSyntax body, SyntaxToken endKeyword)
            : base(syntaxTree)
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
        public TypeSyntax? TypeClause { get; }
        public SyntaxToken IsKeyword { get; }
        public BlockStatementSyntax Body { get; }
        public SyntaxToken EndKeyword { get; }
    }



}