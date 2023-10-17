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
    internal enum BoundUnaryOperatorKind
    {
        Identity,
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
        public BoundUnaryExpression(BoundUnaryOperatorKind operatorKind, BoundExpression operand)
        {
            OperatorKind = operatorKind;
            Operand = operand;
        }
        public BoundUnaryOperatorKind OperatorKind { get; }
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
        private readonly List<string> _diagnostics = new List<string>();
        public  IEnumerable<string> Diagnostics => _diagnostics;
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
            var value = syntax.LiteralToken.Value as int? ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperatorKind = BindUnaryOperatorKind(syntax.OperatorToken.Kind, boundOperand.Type);
            if (boundOperatorKind == null)
            {
                _diagnostics.Add($"Unary Operator '{syntax.OperatorToken.Text}' is not definted for type {boundOperand.Type}");
                return boundOperand;
            }
            return new BoundUnaryExpression(boundOperatorKind.Value, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var left = BindExpression(syntax.Left);
            var right = BindExpression(syntax.Right);
            var operatorKind = BindBinaryOperatorKind(syntax.OperatorToken.Kind, left.Type, right.Type);
            if (operatorKind == null)
            {
                _diagnostics.Add($"Binary operator '{syntax.OperatorToken.Text}' is not defined for types {left.Type} and {right.Type}");
                return left;
            }
            return new BoundBinaryExpression(left,operatorKind.Value, right);
        }

        private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type operandType)
        {
            if (operandType != typeof(int))
                return null;
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return BoundUnaryOperatorKind.Identity;
                case SyntaxKind.MinusToken:
                    return BoundUnaryOperatorKind.Negation;
                default:
                    throw new Exception($"Unexpected unary operator {kind}");
            }
        }


        private BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind kind, Type leftType, Type rightType)
        {
            if (leftType != typeof(int) || rightType != typeof(int))
                return null;
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return BoundBinaryOperatorKind.Addition;
                case SyntaxKind.MinusToken:
                    return BoundBinaryOperatorKind.Subtraction;
                case SyntaxKind.StarToken:
                    return BoundBinaryOperatorKind.Multiplication;
                case SyntaxKind.SlashToken:
                    return BoundBinaryOperatorKind.Division;
                default:
                    throw new Exception($"Unexpected unary operator {kind}");
            }
        }
    }
}