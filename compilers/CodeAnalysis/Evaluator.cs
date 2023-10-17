using compilers.CodeAnalysis.Binding;

namespace compilers.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundExpression _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }


        public object Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
            {
                return n.Value!;
            }
            if (node is BoundVariableExpression v)
            {
                var value = _variables[v.Variable];
                return value;
            }
            if (node is BoundAssignmentExpression a)
            {
                var value = EvaluateExpression(a.Expression);
                _variables[a.Variable] = value;
                return value;
            }
            if (node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);
                switch (u.Op.Kind)
                {
                    case BoundUnaryOperatorKind.Negation:
                        return -(int)operand;
                    case BoundUnaryOperatorKind.Identity:
                        return (int)operand;
                    case BoundUnaryOperatorKind.LogicalNegation:
                        return !(bool)operand;
                    default:
                        throw new Exception($"Unexpected unary operator {u.Op.Kind}");
                }
            }
            if (node is BoundBinaryExpression b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);
                switch (b.Op.Kind)
                {
                    case BoundBinaryOperatorKind.Addition:
                        return (int)left + (int)right;
                    case BoundBinaryOperatorKind.Subtraction:
                        return (int)left - (int)right;
                    case BoundBinaryOperatorKind.Multiplication:
                        return (int)left * (int)right;
                    case BoundBinaryOperatorKind.Division:
                        return (int)left / (int)right;
                    case BoundBinaryOperatorKind.LogicalAnd:
                        return (bool)left & (bool)right;
                    case BoundBinaryOperatorKind.LogicalXor:
                        return (bool)left ^ (bool)right;
                    case BoundBinaryOperatorKind.LogicalOr:
                        return (bool)left | (bool)right;
                    case BoundBinaryOperatorKind.Equal:
                        return Equals(left, right);
                    case BoundBinaryOperatorKind.NotEqual:
                        return !Equals(left, right);
                    default:
                        throw new Exception($"Unexpected vinary operator {b.Op.Kind}");
                }
            }
            throw new Exception($"Unexpected node {node.Kind}");
        }
    }
}