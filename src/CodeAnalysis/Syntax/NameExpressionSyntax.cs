namespace ImperativeCompiler.CodeAnalysis.Syntax;

public sealed class NameExpressionSyntax : ExpressionSyntax
{
    public NameExpressionSyntax(SyntaxTree syntaxTree, Variable variable)
    : base(syntaxTree)
    {
        Variable = variable;
    }
    public override SyntaxKind Kind => SyntaxKind.NameExpression;

    public Variable Variable { get; }

}