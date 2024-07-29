namespace ImperativeCompiler.CodeAnalysis.Syntax;

public sealed class Variable : SyntaxNode
{
    public Variable(SyntaxTree syntaxTree, SyntaxToken identifier, List<ExpressionSyntax>? indices)
    : base(syntaxTree)
    {
        Identifier = identifier;
        Indices = indices;
    }
    public override SyntaxKind Kind => SyntaxKind.Variable;

    public SyntaxToken Identifier { get; }
    public List<ExpressionSyntax>? Indices { get; }
}