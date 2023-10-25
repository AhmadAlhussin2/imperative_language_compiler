using System.Collections.Immutable;
using compilers.CodeAnalysis.Binding;
using compilers.CodeAnalysis.Symbol;

namespace compilers.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> _functionBodies;
        private readonly BoundBlockStatement _root;
        private readonly Dictionary<VariableSymbol, object> _globals;
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new();
        private object? _lastValue;

        public Evaluator(System.Collections.Immutable.ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies, BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _functionBodies = functionBodies;
            _root = root;
            _globals = variables;
        }


        public object? Evaluate()
        {
            return Evaluate(_root);
        }

        private object? Evaluate(BoundBlockStatement body)
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();
            for (var i = 0; i < body.Statements.Length; i++)
            {
                if (body.Statements[i] is BoundLabelStatement l)
                {
                    labelToIndex.Add(l.Label, i + 1);
                }
            }
            var index = 0;
            while (index < body.Statements.Length)
            {
                var s = body.Statements[index];
                switch (s.Kind)
                {
                    case BoundNodeKind.VariableDeclaration:
                        EvaluateVariableDeclaration((BoundVariableDeclaration)s);
                        index++;
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement)s);
                        index++;
                        break;
                    case BoundNodeKind.GotoStatement:
                        var gotoStatement = (BoundGoToStatement)s;
                        index = labelToIndex[gotoStatement.Label];
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        var conditionalGotoStatement = (BoundConditionalGotoStatement)s;
                        var condition = (bool)EvaluateExpression(conditionalGotoStatement.Condition);

                        if (condition == conditionalGotoStatement.JumpIfTrue)
                            index = labelToIndex[conditionalGotoStatement.Label];
                        else
                            index++;
                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;
                    default:
                        throw new Exception($"Unexpected node {s.Kind}");
                }
            }
            return _lastValue;
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            _globals[node.Variable] = value;
            _lastValue = value;
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    return EvaluateLiteralExpression((BoundLiteralExpression)node);
                case BoundNodeKind.VariableExpression:
                    return EvaluateVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.AssignmentExpression:
                    return EvaluateAssignmentExpression((BoundAssignmentExpression)node);
                case BoundNodeKind.UnaryExpression:
                    return EvaluateUnaryExpression((BoundUnaryExpression)node);
                case BoundNodeKind.BinaryExpression:
                    return EvaluateBinaryExpression((BoundBinaryExpression)node);
                case BoundNodeKind.CallExpression:
                    return EvaluateCallExpression((BoundCallExpression)node);
                case BoundNodeKind.ConversionExpression:
                    return EvaluateConversionExpression((BoundConversionExpression)node);
                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }
        }

        private object EvaluateConversionExpression(BoundConversionExpression node)
        {
            var value = EvaluateExpression(node.Expression);
            if (node.Type == TypeSymbol.Bool)
                return Convert.ToBoolean(value);
            else if (node.Type == TypeSymbol.Int)
                return Convert.ToInt32(value);
            else if (node.Type == TypeSymbol.Real)
                return Convert.ToDouble(value);
            else
                throw new Exception($"Unexpected casting");
        }

        private object EvaluateCallExpression(BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.PrintInt)
            {
                var message = EvaluateExpression(node.Arguments[0]).ToString();
                Console.WriteLine(message);
                return null;
            }
            else
            {
                var locals = new Dictionary<VariableSymbol, object>();
                for (int i = 0; i < node.Arguments.Length; i++)
                {
                    var parameter = node.Function.Parameters[i];
                    var value = EvaluateExpression(node.Arguments[i]);
                    locals.Add(parameter, value);
                }
                _locals.Push(locals);
                var statement = _functionBodies[node.Function];
                return Evaluate(statement);
            }
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression b)
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
                case BoundBinaryOperatorKind.LessThan:
                    return (int)left < (int)right;
                case BoundBinaryOperatorKind.LessThanOrEqual:
                    return (int)left <= (int)right;
                case BoundBinaryOperatorKind.GreaterThan:
                    return (int)left > (int)right;
                case BoundBinaryOperatorKind.GreaterThanOrEqual:
                    return (int)left >= (int)right;
                default:
                    throw new Exception($"Unexpected vinary operator {b.Op.Kind}");
            }
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression u)
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

        private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            _globals[a.Variable] = value;
            return value;
        }

        private object EvaluateVariableExpression(BoundVariableExpression v)
        {
            if (_locals.Count > 0)
            {
                var locals = _locals.Peek();
                if (locals.TryGetValue(v.Variable, out var value))
                {
                    return value;
                }
            }
            return _globals[v.Variable];
        }

        private static object EvaluateLiteralExpression(BoundLiteralExpression n)
        {
            return n.Value;
        }
    }
}