namespace ImperativeCompiler.CodeAnalysis.Syntax;

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    public BinaryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
    : base(syntaxTree)
    {
        Right = right;
        OperatorToken = operatorToken;
        Left = left;
    }
    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Right { get; }
    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

}