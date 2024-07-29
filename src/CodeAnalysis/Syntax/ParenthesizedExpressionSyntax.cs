namespace ImperativeCompiler.CodeAnalysis.Syntax;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken OpenParenthesisToken { get; }
    public ExpressionSyntax Expression { get; }
    public SyntaxToken CloseParenthesisToken { get; }
    public ParenthesizedExpressionSyntax(SyntaxTree syntaxTree,SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closeParenthesisToken)
    :base(syntaxTree)
    {
        CloseParenthesisToken = closeParenthesisToken;
        Expression = expression;
        OpenParenthesisToken = openParenthesisToken;

    }
    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;


}