using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace compilers.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        LiteralExpression,
        UnaryExpression,
        BinaryExpression
    }
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
    }
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }
    internal enum BoundUnaryExpressionKind
    {
        Idendity,
        Negation
    }
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value)
        {
            Value = value;
        }
        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override Type Type => Value.GetType();
        public object Value { get; }
    }
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(BoundUnaryExpressionKind operatorKind, BoundExpression operand)
        {
            OperatorKind = operatorKind;
            Operand = operand;
        }
        public BoundUnaryExpressionKind OperatorKind { get; }
        public BoundExpression Operand { get; }

        public override Type Type => Operand.Type;

        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    }
    internal enum BoundBinaryOperatorKind
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression(BoundExpression left, BoundBinaryOperatorKind operatorKind, BoundExpression right)
        {
            OperatorKind = operatorKind;
            Left = left;
            Right = right;
        }
        public BoundBinaryOperatorKind OperatorKind { get; }
        public BoundExpression Right { get; }
        public BoundExpression Left { get; }

        public override Type Type => Right.Type;

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    }

    internal sealed class Binder
    {
        public BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            switch (syntax.Kind){
                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression((BinaryExpressionSyntax)syntax);
                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression((UnaryExpressionSyntax)syntax);
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression((LiteralExpressionSyntax)syntax);
                default:
                    throw new Exception($"Unexpected syntax {syntax.Kind}");
            }
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            int value = syntax.LiteralToken.Value as int? ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var operatorKind = BindUnaryExpressionKind(syntax.OperatorToken.Kind);
            var boundOperand = BindExpression(syntax.Operand);
            return new BoundUnaryExpression(operatorKind, boundOperand);
        }

        private BoundUnaryExpressionKind BindUnaryExpressionKind(SyntaxKind kind)
        {
            throw new NotImplementedException();
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var left = BindExpression(syntax.Left);
            var operatorKind = BindBinaryOperatorKind(syntax.OperatorToken.Kind);
            var right = BindExpression(syntax.Right);
            return new BoundBinaryExpression(left,operatorKind, right);
        }

        private BoundBinaryOperatorKind BindBinaryOperatorKind(SyntaxKind kind)
        {
            throw new NotImplementedException();
        }
    }
}