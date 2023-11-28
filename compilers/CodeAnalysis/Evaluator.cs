using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using compilers.CodeAnalysis.Binding;
using compilers.CodeAnalysis.Symbol;
using LLVMSharp.Interop;

namespace compilers.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> _functionBodies;
        private readonly BoundBlockStatement _root;
        private readonly Dictionary<VariableSymbol, object> _globals;
        private Dictionary<VariableSymbol, LLVMValueRef> _LLVMglobals = new Dictionary<VariableSymbol, LLVMValueRef>();
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new();
        private Stack<Dictionary<VariableSymbol, LLVMValueRef>> _LLVMlocals = new();
        private object? _lastValue;
        private LLVMBuilderRef _builder;
        private Stack<LLVMValueRef> _valueStack = new Stack<LLVMValueRef>();
        public Evaluator(LLVMBuilderRef builder, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies, BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _functionBodies = functionBodies;
            _root = root;
            _globals = variables;
            _builder = builder;
        }
        static unsafe sbyte* StringToSBytePtr(string str)
        {
            // Convert the string to a byte array using UTF-8 encoding
            byte[] bytes = Encoding.UTF8.GetBytes(str + '\0');

            // Allocate unmanaged memory to hold the null-terminated string
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);

            // Copy the byte array to the allocated memory
            Marshal.Copy(bytes, 0, ptr, bytes.Length);

            // Return a pointer to the allocated memory
            return (sbyte*)ptr;
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
                    case BoundNodeKind.GoToStatement:
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
                    case BoundNodeKind.ReturnStatement:
                        var rs = (BoundReturnStatement)s;
                        _lastValue = EvaluateExpression(rs.Expression);
                        return _lastValue;
                    default:
                        throw new Exception($"Unexpected node {s.Kind}");
                }
            }
            return _lastValue;
        }
        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            var lst = _valueStack.Pop();
            if (node.Initializer.Type == TypeSymbol.Int) unsafe 
            {
                var n = LLVM.BuildAlloca(_builder, LLVM.Int32Type(), StringToSBytePtr(node.Variable.Name));
                LLVM.BuildStore(_builder, lst, n);
                _valueStack.Push(n);
            }
            else if (node.Initializer.Type == TypeSymbol.Real) unsafe
            {
                var n = LLVM.BuildAlloca(_builder, LLVM.DoubleType(), StringToSBytePtr(node.Variable.Name));
                LLVM.BuildStore(_builder, LLVM.ConstReal(LLVM.DoubleType(), Convert.ToDouble(value)), n);
                _valueStack.Push(n);
            }
            else if (node.Initializer.Type == TypeSymbol.Bool) unsafe
                {
                    LLVMValueRef b = LLVM.BuildAlloca(_builder, LLVM.Int1Type(), StringToSBytePtr(node.Variable.Name));
                    LLVM.BuildStore(_builder, LLVM.ConstInt(LLVM.Int1Type(), Convert.ToUInt32(value), 0), b);
                }
            _lastValue = value;
            Assign(node.Variable, value);
               
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
        private object? EvaluateCallExpression(BoundCallExpression node)
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
                var result = Evaluate(statement);
                _locals.Pop();
                return result;
            }
        }
        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            var left = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            var rightLLVM = _valueStack.Pop();
            var leftLLVM = _valueStack.Pop();

            switch (b.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (left is double || right is double) unsafe
                        {
                            var ret = LLVM.BuildFAdd(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempAdd"));
                            _valueStack.Push(ret);
                            return (double)left + (double)right;
                        }
                    else unsafe
                        {
                            var ret = LLVM.BuildAdd(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempAdd"));
                            _valueStack.Push(ret);
                            return (int)left + (int)right;
                        }
                case BoundBinaryOperatorKind.Subtraction:
                    if (left is double || right is double) unsafe
                        {
                            var ret = LLVM.BuildFSub(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempSub"));
                            _valueStack.Push(ret);
                            return (double)left - (double)right;
                        }
                    else unsafe
                        {
                            var ret = LLVM.BuildSub(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempSub"));
                            _valueStack.Push(ret);
                            return (int)left - (int)right;
                        }
                case BoundBinaryOperatorKind.Multiplication:
                    if (left is double || right is double) unsafe
                        {
                            var ret = LLVM.BuildFMul(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempMul"));
                            _valueStack.Push(ret);
                            return (double)left * (double)right;
                        }
                    else unsafe
                        {
                            var ret = LLVM.BuildMul(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempMul"));
                            _valueStack.Push(ret);
                            return (int)left * (int)right;
                        }
                case BoundBinaryOperatorKind.Division:
                    if (left is double || right is double) unsafe
                        {
                            var ret = LLVM.BuildFDiv(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempDiv"));
                            _valueStack.Push(ret);
                            return (double)left / (double)right;
                        }
                    else unsafe
                        {
                            var ret = LLVM.BuildSDiv(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempDiv"));
                            _valueStack.Push(ret);
                            return (int)left / (int)right;
                        }
                case BoundBinaryOperatorKind.LogicalAnd:
                    unsafe
                    {
                        var ret = LLVM.BuildAnd(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempAnd"));
                        _valueStack.Push(ret);
                    }
                    return (bool)left & (bool)right;
                case BoundBinaryOperatorKind.LogicalXor:
                    unsafe
                    {
                        var ret = LLVM.BuildXor(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempXor"));
                        _valueStack.Push(ret);
                    }
                    return (bool)left ^ (bool)right;
                case BoundBinaryOperatorKind.LogicalOr:
                    unsafe
                    {
                        var ret = LLVM.BuildOr(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempOr"));
                        _valueStack.Push(ret);
                    }
                    return (bool)left | (bool)right;
                case BoundBinaryOperatorKind.Equal:
                    unsafe
                    {
                        LLVMValueRef ret;
                        if (left is double && right is double)
                        {
                            ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOEQ, leftLLVM, rightLLVM, StringToSBytePtr("tempEq"));
                        }
                        else
                        {
                            ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntEQ, leftLLVM, rightLLVM, StringToSBytePtr("tempEq"));
                        }
                        _valueStack.Push(ret);
                    }
                    return Equals(left, right);
                case BoundBinaryOperatorKind.NotEqual:
                    unsafe
                    {
                        LLVMValueRef ret;
                        if (left is double && right is double)
                        {
                            ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealONE, leftLLVM, rightLLVM, StringToSBytePtr("tempNotEq"));
                        }
                        else
                        {
                            ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntNE, leftLLVM, rightLLVM, StringToSBytePtr("tempNotEq"));
                        }
                        _valueStack.Push(ret);
                    }
                    return !Equals(left, right);
                case BoundBinaryOperatorKind.LessThan:
                    if (left is double || right is double) unsafe
                        {
                            var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOLT, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                            _valueStack.Push(ret);
                            return (double)left < (double)right;
                        }
                    else unsafe
                        {
                            var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSLT, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                            _valueStack.Push(ret);
                            return (int)left < (int)right;
                        }
                case BoundBinaryOperatorKind.LessThanOrEqual:
                    if (left is double || right is double) unsafe
                        {
                            var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOLE, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                            _valueStack.Push(ret);
                            return (double)left <= (double)right;
                        }
                    else unsafe
                        {
                            var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSLE, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                            _valueStack.Push(ret);
                            return (int)left <= (int)right;
                        }
                case BoundBinaryOperatorKind.GreaterThan:
                    if (left is double || right is double) unsafe
                        {
                            var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOGT, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                            _valueStack.Push(ret);
                            return (double)left > (double)right;
                        }
                    else unsafe
                        {
                            var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSGT, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                            _valueStack.Push(ret);
                            return (int)left > (int)right;
                        }
                case BoundBinaryOperatorKind.GreaterThanOrEqual:
                    if (left is double || right is double) unsafe
                        {
                            var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOGE, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                            _valueStack.Push(ret);
                            return (double)left >= (double)right;
                        }
                    else unsafe
                        {
                            var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSGE, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                            _valueStack.Push(ret);
                            return (int)left >= (int)right;
                        }
                case BoundBinaryOperatorKind.Modulo:
                    unsafe
                    {
                        var ret = LLVM.BuildSRem(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempRem"));
                        _valueStack.Push(ret);
                    }
                    return (int)left % (int)right;
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
                    if (u.Operand.Type == TypeSymbol.Int)
                        return -(int)operand;
                    else if (u.Operand.Type == TypeSymbol.Real)
                        return -(double)operand;
                    throw new Exception($"Operator - is not defined for {u.Op.ToString()}");
                case BoundUnaryOperatorKind.Identity:
                    return (int)operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool)operand;
                default:
                    throw new Exception($"Unexpected unary operator {u.Op.Kind}");
            }
        }
        private object EvaluateVariableExpression(BoundVariableExpression v)
        {
            
            if (v.Variable.Kind == SymbolKind.GlobalVariable)
            {
                var myVar = _LLVMglobals[v.Variable];
                if(v.Variable.Type == TypeSymbol.Int) unsafe 
                {
                    var d = LLVM.BuildLoad2(_builder, LLVM.Int32Type(), myVar, StringToSBytePtr("load"));
                    _valueStack.Push(d);
                }
                else if(v.Variable.Type == TypeSymbol.Real) unsafe
                {
                    var d = LLVM.BuildLoad2(_builder, LLVM.DoubleType(), myVar, StringToSBytePtr("load"));
                    _valueStack.Push(d);
                }
                else if(v.Variable.Type == TypeSymbol.Bool) unsafe 
                {
                    var d = LLVM.BuildLoad2(_builder, LLVM.Int1Type(), myVar, StringToSBytePtr("load"));
                    _valueStack.Push(d);
                }
                return _globals[v.Variable];
            }
            else
            {
                var locals = _locals.Peek();
                var LLVMlocals = _LLVMlocals.Peek();
                var myVar = LLVMlocals[v.Variable];
                if(v.Variable.Type == TypeSymbol.Int) unsafe 
                {
                    var d = LLVM.BuildLoad2(_builder, LLVM.Int32Type(), myVar, StringToSBytePtr("load"));
                    _valueStack.Push(d);
                }
                else if(v.Variable.Type == TypeSymbol.Real) unsafe
                {
                    var d = LLVM.BuildLoad2(_builder, LLVM.DoubleType(), myVar, StringToSBytePtr("load"));
                    _valueStack.Push(d);
                }
                else if(v.Variable.Type == TypeSymbol.Bool) unsafe 
                {
                    var d = LLVM.BuildLoad2(_builder, LLVM.Int1Type(), myVar, StringToSBytePtr("load"));
                    _valueStack.Push(d);
                }
                return locals[v.Variable];
            }

        }
        private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            Assign(a.Variable, value);
            return value;
        }
        private object EvaluateLiteralExpression(BoundLiteralExpression n)
        {
            if (n.Type == TypeSymbol.Int) unsafe
                {
                    int sign = (int)n.Value < 0 ? 1 : 0;
                    int value = (int)n.Value;
                    if (sign == 1)
                    {
                        value *= -1;
                        _valueStack.Push(LLVM.ConstInt(LLVM.Int32Type(), (ulong)-Convert.ToUInt32(value), sign));
                    }
                    else
                    {
                        _valueStack.Push(LLVM.ConstInt(LLVM.Int32Type(), Convert.ToUInt32(value), sign));
                    }
                }
            else if (n.Type == TypeSymbol.Real) unsafe
                {
                    _valueStack.Push(LLVM.ConstReal(LLVM.DoubleType(), Convert.ToDouble(n.Value)));
                }
            else if (n.Type == TypeSymbol.Bool) unsafe
                {
                    _valueStack.Push(LLVM.ConstInt(LLVM.Int1Type(), Convert.ToUInt32(n.Value), 0));
                }
            return n.Value;
        }
        private void Assign(VariableSymbol variable, object value)
        {
            
            if (variable.Kind == SymbolKind.GlobalVariable)
            {
                _globals[variable] = value;
                _LLVMglobals[variable] = _valueStack.Pop();
            }
            else
            {
                var locals = _locals.Peek();
                locals[variable] = value;
                var LLVMlocals = _LLVMlocals.Peek();
                LLVMlocals[variable] = _valueStack.Pop();
            }
        }
    }
}