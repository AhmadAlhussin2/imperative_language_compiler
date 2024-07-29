namespace ImperativeCompiler.CodeAnalysis.Syntax;

public sealed class AssignmentExpressionSyntax : ExpressionSyntax
{
    public AssignmentExpressionSyntax(SyntaxTree syntaxTree, Variable variable, SyntaxToken assignmentToken, ExpressionSyntax expression)
    : base(syntaxTree)
    {
        Variable = variable;
        AssignmentToken = assignmentToken;
        Expression = expression;
    }
    public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
    public Variable Variable { get; }
    public SyntaxToken AssignmentToken { get; }
    public ExpressionSyntax Expression { get; }

}