using System.Data.Common;
using System.Linq.Expressions;

namespace compilers.CodeAnalysis
{
    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
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
}