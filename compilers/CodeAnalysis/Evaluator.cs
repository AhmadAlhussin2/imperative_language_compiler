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
        private readonly Dictionary<FunctionSymbol, LLVMValueRef> _LLVMfunctions;
        private readonly Dictionary<FunctionSymbol, LLVMTypeRef> _LLVMfunctiontypes;
        private readonly BoundBlockStatement _root;
        private readonly Dictionary<VariableSymbol, object> _globals = new();
        private Dictionary<VariableSymbol, LLVMValueRef> _LLVMglobals = new Dictionary<VariableSymbol, LLVMValueRef>();
        public readonly Stack<Dictionary<VariableSymbol, object>> _locals = new();
        public Stack<Dictionary<VariableSymbol, LLVMValueRef>> _LLVMlocals = new();
        private object? _lastValue;
        private LLVMBuilderRef _builder;
        private Stack<LLVMValueRef> _valueStack = new Stack<LLVMValueRef>();
        public Evaluator
            (
                LLVMBuilderRef builder, ImmutableDictionary<FunctionSymbol,
                BoundBlockStatement> functionBodies,
                Dictionary<FunctionSymbol, LLVMValueRef> LLVMfunctions,
                Dictionary<FunctionSymbol, LLVMTypeRef> LLVMfunctiontypes,
                BoundBlockStatement root
            )
        {
            _functionBodies = functionBodies;
            _root = root;
            _builder = builder;
            _LLVMfunctions = LLVMfunctions;
            _LLVMfunctiontypes = LLVMfunctiontypes;
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
        public object? Evaluate(LLVMValueRef function)
        {
            return Evaluate(_root, function, true);
        }
        private object? Evaluate(BoundBlockStatement body, LLVMValueRef function, bool genCode)
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();
            var LLVMLabel = new Dictionary<BoundLabel, LLVMBasicBlockRef>();
            var labelToBuilder = new Dictionary<BoundLabel, LLVMBuilderRef>();
            for (var i = 0; i < body.Statements.Length; i++)
                if (body.Statements[i] is BoundLabelStatement l) unsafe
                    {
                        labelToIndex.Add(l.Label, i + 1);
                    }
            var index = 0;
            if (genCode)
            {
                while (index < body.Statements.Length)
                {
                    var s = body.Statements[index];
                    switch (s.Kind)
                    {
                        case BoundNodeKind.VariableDeclaration:
                            EvaluateVariableDeclaration((BoundVariableDeclaration)s, true);
                            index++;
                            break;
                        case BoundNodeKind.ExpressionStatement:
                            EvaluateExpressionStatement((BoundExpressionStatement)s, true);
                            index++;
                            break;
                        case BoundNodeKind.GoToStatement:
                            var gotoStatement = (BoundGoToStatement)s;
                            unsafe
                            {
                                if (!LLVMLabel.ContainsKey(gotoStatement.Label))
                                {
                                    LLVMBasicBlockRef block = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                                    LLVMLabel.Add(gotoStatement.Label, block);
                                }

                                var rr = LLVM.BuildBr(_builder, LLVMLabel[gotoStatement.Label]);
                            }
                            index++;
                            break;
                        case BoundNodeKind.ConditionalGotoStatement:
                            var conditionalGotoStatement = (BoundConditionalGotoStatement)s;
                            var condition = (bool)EvaluateExpression(conditionalGotoStatement.Condition, true);

                            if (conditionalGotoStatement.JumpIfTrue) unsafe
                                {
                                    LLVMBasicBlockRef falseblock = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                                    if (!LLVMLabel.ContainsKey(conditionalGotoStatement.Label)) unsafe
                                        {
                                            LLVMBasicBlockRef block = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                                            LLVMLabel.Add(conditionalGotoStatement.Label, block);
                                        }
                                    LLVM.BuildCondBr(_builder, _valueStack.Pop(), LLVMLabel[conditionalGotoStatement.Label], falseblock);
                                    LLVM.PositionBuilderAtEnd(_builder, falseblock);

                                }
                            else unsafe
                                {
                                    LLVMBasicBlockRef trueblock = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                                    if (!LLVMLabel.ContainsKey(conditionalGotoStatement.Label)) unsafe
                                        {
                                            LLVMBasicBlockRef block = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                                            LLVMLabel.Add(conditionalGotoStatement.Label, block);
                                        }
                                    LLVM.BuildCondBr(_builder, _valueStack.Pop(), trueblock, LLVMLabel[conditionalGotoStatement.Label]);
                                    LLVM.PositionBuilderAtEnd(_builder, trueblock);
                                }
                            index++;
                            break;

                        case BoundNodeKind.LabelStatement:
                            if (s is BoundLabelStatement l) unsafe
                                {
                                    if (!LLVMLabel.ContainsKey(l.Label))
                                    {
                                        LLVMBasicBlockRef block2 = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                                        LLVMLabel.Add(l.Label, block2);
                                    }
                                    var block = LLVMLabel[l.Label];
                                    LLVM.BuildBr(_builder, block);
                                    LLVM.PositionBuilderAtEnd(_builder, block);

                                }
                            index++;
                            break;
                        case BoundNodeKind.ReturnStatement:
                            var rs = (BoundReturnStatement)s;
                            _lastValue = EvaluateExpression(rs.Expression, true);
                            unsafe
                            {
                                LLVM.BuildRet(_builder, _valueStack.Pop());
                            }
                            return _lastValue;
                        default:
                            throw new Exception($"Unexpected node {s.Kind}");

                    }
                }
            }

            index = 0;
            while (index < body.Statements.Length)
            {
                var s = body.Statements[index];
                switch (s.Kind)
                {
                    case BoundNodeKind.VariableDeclaration:
                        EvaluateVariableDeclaration((BoundVariableDeclaration)s, false);
                        index++;
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement)s, false);
                        index++;
                        break;
                    case BoundNodeKind.GoToStatement:
                        var gotoStatement = (BoundGoToStatement)s;
                        index = labelToIndex[gotoStatement.Label];
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        var conditionalGotoStatement = (BoundConditionalGotoStatement)s;
                        var condition = (bool)EvaluateExpression(conditionalGotoStatement.Condition, false);
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
                        _lastValue = EvaluateExpression(rs.Expression, false);
                        return _lastValue;
                    default:
                        throw new Exception($"Unexpected node {s.Kind}");
                }
            }
            return _lastValue;
        }
        private void EvaluateVariableDeclaration(BoundVariableDeclaration node, bool generateCode)
        {
            var value = EvaluateExpression(node.Initializer, generateCode);
            LLVMValueRef lst = null;
            if (generateCode)
            {
                lst = _valueStack.Pop();
            }
            if (generateCode) unsafe
                {
                    if (node.Initializer.Type == TypeSymbol.Int)
                    {
                        var n = LLVM.BuildAlloca(_builder, LLVM.Int32Type(), StringToSBytePtr(node.Variable.Name));
                        LLVM.BuildStore(_builder, lst, n);
                        _valueStack.Push(n);
                    }
                    else if (node.Initializer.Type == TypeSymbol.Real)
                    {
                        var n = LLVM.BuildAlloca(_builder, LLVM.DoubleType(), StringToSBytePtr(node.Variable.Name));
                        LLVM.BuildStore(_builder, LLVM.ConstReal(LLVM.DoubleType(), Convert.ToDouble(value)), n);
                        _valueStack.Push(n);
                    }
                    else if (node.Initializer.Type == TypeSymbol.Bool)
                    {
                        LLVMValueRef b = LLVM.BuildAlloca(_builder, LLVM.Int1Type(), StringToSBytePtr(node.Variable.Name));
                        LLVM.BuildStore(_builder, LLVM.ConstInt(LLVM.Int1Type(), Convert.ToUInt32(value), 0), b);
                        _valueStack.Push(b);
                    }
                }
            _lastValue = value;
            Assign(node.Variable, value, generateCode);

        }
        private void EvaluateExpressionStatement(BoundExpressionStatement node, bool generateCode)
        {
            _lastValue = EvaluateExpression(node.Expression, generateCode);
        }
        private object EvaluateExpression(BoundExpression node, bool generateCode)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    return EvaluateLiteralExpression((BoundLiteralExpression)node, generateCode);
                case BoundNodeKind.VariableExpression:
                    return EvaluateVariableExpression((BoundVariableExpression)node, generateCode);
                case BoundNodeKind.AssignmentExpression:
                    return EvaluateAssignmentExpression((BoundAssignmentExpression)node, generateCode);
                case BoundNodeKind.UnaryExpression:
                    return EvaluateUnaryExpression((BoundUnaryExpression)node, generateCode);
                case BoundNodeKind.BinaryExpression:
                    return EvaluateBinaryExpression((BoundBinaryExpression)node, generateCode);
                case BoundNodeKind.CallExpression:
                    return EvaluateCallExpression((BoundCallExpression)node, generateCode) ?? 0;
                case BoundNodeKind.ConversionExpression:
                    return EvaluateConversionExpression((BoundConversionExpression)node, generateCode);
                default:
                    throw new Exception($"Unexpected node {node.Kind}");
            }
        }
        private object EvaluateConversionExpression(BoundConversionExpression node, bool generateCode)
        {
            var value = EvaluateExpression(node.Expression, generateCode);
            if (node.Type == TypeSymbol.Bool)
                return Convert.ToBoolean(value);
            else if (node.Type == TypeSymbol.Int)
                return Convert.ToInt32(value);
            else if (node.Type == TypeSymbol.Real)
                return Convert.ToDouble(value);
            else
                throw new Exception($"Unexpected casting");
        }
        private object? EvaluateCallExpression(BoundCallExpression node, bool generateCode)
        {
            if (node.Function == BuiltinFunctions.PrintInt)
            {
                var message = EvaluateExpression(node.Arguments[0], generateCode).ToString();
                Console.WriteLine(message);
                return null;
            }
            else unsafe
                {
                    if (generateCode)
                    {
                        var locals = new Dictionary<VariableSymbol, object>();
                        LLVMOpaqueValue*[] paramValues = new LLVMOpaqueValue*[node.Arguments.Length];
                        for (int i = 0; i < node.Arguments.Length; i++)
                        {
                            var parameter = node.Function.Parameters[i];
                            var value = EvaluateExpression(node.Arguments[i], generateCode);

                            paramValues[i] = _valueStack.Pop();
                            locals.Add(parameter, value);
                        }
                        LLVMValueRef callFunction;
                        fixed (LLVMOpaqueValue** ptr = paramValues)
                        {
                            callFunction = LLVM.BuildCall2(_builder, _LLVMfunctiontypes[node.Function], _LLVMfunctions[node.Function], ptr, 1, StringToSBytePtr("call"));
                        }
                        _valueStack.Push(callFunction);

                        _locals.Push(locals);
                        var statement = _functionBodies[node.Function];
                        // var result = Evaluate(statement);
                        var result = Evaluate(statement, _LLVMfunctions[node.Function], false);
                        _locals.Pop();
                        return result;
                    }
                    else
                    {
                        var locals = new Dictionary<VariableSymbol, object>();
                        for (int i = 0; i < node.Arguments.Length; i++)
                        {
                            var parameter = node.Function.Parameters[i];
                            var value = EvaluateExpression(node.Arguments[i], generateCode);
                            locals.Add(parameter, value);
                        }

                        _locals.Push(locals);
                        var statement = _functionBodies[node.Function];
                        // var result = Evaluate(statement);
                        var result = Evaluate(statement, null, false);
                        _locals.Pop();
                        return result;
                    }


                }
        }
        private object EvaluateBinaryExpression(BoundBinaryExpression b, bool generateCode)
        {
            var left = EvaluateExpression(b.Left, generateCode);
            var right = EvaluateExpression(b.Right, generateCode);
            LLVMValueRef rightLLVM = null;
            LLVMValueRef leftLLVM = null;
            if (generateCode) unsafe
                {
                    rightLLVM = _valueStack.Pop();
                    leftLLVM = _valueStack.Pop();
                }
            switch (b.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (left is double || right is double)
                    {
                        if (generateCode) unsafe
                            {

                                var ret = LLVM.BuildFAdd(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempAdd"));
                                _valueStack.Push(ret);
                            }
                        return (double)left + (double)right;
                    }
                    else
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildAdd(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempAdd"));
                                _valueStack.Push(ret);
                            }
                        return (int)left + (int)right;
                    }
                case BoundBinaryOperatorKind.Subtraction:
                    if (left is double || right is double)
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildFSub(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempSub"));
                                _valueStack.Push(ret);

                            }
                        return (double)left - (double)right;
                    }
                    else
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildSub(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempSub"));
                                _valueStack.Push(ret);
                            }
                        return (int)left - (int)right;
                    }
                case BoundBinaryOperatorKind.Multiplication:
                    if (left is double || right is double)
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildFMul(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempMul"));
                                _valueStack.Push(ret);
                            }
                        return (double)left * (double)right;
                    }
                    else
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildMul(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempMul"));
                                _valueStack.Push(ret);
                            }
                        return (int)left * (int)right;
                    }
                case BoundBinaryOperatorKind.Division:
                    if (left is double || right is double)
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildFDiv(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempDiv"));
                                _valueStack.Push(ret);
                            }
                        return (double)left / (double)right;
                    }
                    else
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildSDiv(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempDiv"));
                                _valueStack.Push(ret);
                            }
                        return (int)left / (int)right;
                    }
                case BoundBinaryOperatorKind.LogicalAnd:
                    if (generateCode) unsafe
                        {
                            var ret = LLVM.BuildAnd(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempAnd"));
                            _valueStack.Push(ret);
                        }
                    return (bool)left & (bool)right;
                case BoundBinaryOperatorKind.LogicalXor:
                    if (generateCode) unsafe
                        {
                            var ret = LLVM.BuildXor(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempXor"));
                            _valueStack.Push(ret);
                        }
                    return (bool)left ^ (bool)right;
                case BoundBinaryOperatorKind.LogicalOr:
                    if (generateCode) unsafe
                        {
                            var ret = LLVM.BuildOr(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempOr"));
                            _valueStack.Push(ret);
                        }
                    return (bool)left | (bool)right;
                case BoundBinaryOperatorKind.Equal:
                    if (generateCode) unsafe
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
                    if (generateCode) unsafe
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
                    if (left is double || right is double)
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOLT, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                                _valueStack.Push(ret);
                            }
                        return (double)left < (double)right;
                    }
                    else
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSLT, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                                _valueStack.Push(ret);
                            }
                        return (int)left < (int)right;
                    }
                case BoundBinaryOperatorKind.LessThanOrEqual:
                    if (left is double || right is double)
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOLE, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                                _valueStack.Push(ret);
                            }
                        return (double)left <= (double)right;
                    }
                    else
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSLE, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                                _valueStack.Push(ret);
                            }
                        return (int)left <= (int)right;
                    }
                case BoundBinaryOperatorKind.GreaterThan:
                    if (left is double || right is double)
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOGT, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                                _valueStack.Push(ret);
                            }
                        return (double)left > (double)right;
                    }
                    else
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSGT, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                                _valueStack.Push(ret);
                            }
                        return (int)left > (int)right;
                    }
                case BoundBinaryOperatorKind.GreaterThanOrEqual:
                    if (left is double || right is double)
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOGE, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                                _valueStack.Push(ret);
                            }
                        return (double)left >= (double)right;
                    }
                    else
                    {
                        if (generateCode) unsafe
                            {
                                var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSGE, leftLLVM, rightLLVM, StringToSBytePtr("tempLess"));
                                _valueStack.Push(ret);
                            }
                        return (int)left >= (int)right;
                    }
                case BoundBinaryOperatorKind.Modulo:
                    if (generateCode) unsafe
                        {
                            var ret = LLVM.BuildSRem(_builder, leftLLVM, rightLLVM, StringToSBytePtr("tempRem"));
                            _valueStack.Push(ret);
                        }
                    return (int)left % (int)right;
                default:
                    throw new Exception($"Unexpected vinary operator {b.Op.Kind}");
            }
        }
        private object EvaluateUnaryExpression(BoundUnaryExpression u, bool generateCode)
        {
            var operand = EvaluateExpression(u.Operand, generateCode);
            var operandLLVM = _valueStack.Pop();
            switch (u.Op.Kind)
            {
                case BoundUnaryOperatorKind.Negation:
                    if (u.Operand.Type == TypeSymbol.Int) unsafe
                        {
                            if (generateCode)
                            {
                                var ret = LLVM.BuildSub(_builder, LLVM.ConstInt(LLVM.Int32Type(), 0, 0), operandLLVM, StringToSBytePtr("tempNegation"));
                                _valueStack.Push(ret);
                            }
                            return -(int)operand;
                        }
                    else if (u.Operand.Type == TypeSymbol.Real) unsafe
                        {
                            if (generateCode)
                            {
                                var ret = LLVM.BuildFSub(_builder, LLVM.ConstReal(LLVM.DoubleType(), 0), operandLLVM, StringToSBytePtr("tempNegation"));
                                _valueStack.Push(ret);
                            }
                            return -(double)operand;
                        }
                    throw new Exception($"Operator - is not defined for {u.Op.ToString()}");
                case BoundUnaryOperatorKind.Identity:
                    if (u.Operand.Type == TypeSymbol.Int) unsafe
                        {
                            if (generateCode)
                            {
                                var ret = LLVM.BuildFAdd(_builder, LLVM.ConstInt(LLVM.Int32Type(), 0, 0), operandLLVM, StringToSBytePtr("tempNegation"));
                                _valueStack.Push(ret);
                            }
                            return -(int)operand;
                        }
                    else if (u.Operand.Type == TypeSymbol.Real) unsafe
                        {
                            if (generateCode)
                            {
                                var ret = LLVM.BuildAdd(_builder, LLVM.ConstReal(LLVM.DoubleType(), 0), operandLLVM, StringToSBytePtr("tempNegation"));
                                _valueStack.Push(ret);
                            }
                            return -(double)operand;
                        }
                    return (int)operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    if (generateCode) unsafe
                        {
                            var ret = LLVM.BuildXor(_builder, LLVM.ConstInt(LLVM.Int1Type(), 0, 0), operandLLVM, StringToSBytePtr("tempNegation"));
                            _valueStack.Push(ret);
                        }
                    return !(bool)operand;
                default:
                    throw new Exception($"Unexpected unary operator {u.Op.Kind}");
            }
        }
        private object EvaluateVariableExpression(BoundVariableExpression v, bool generateCode)
        {

            if (v.Variable.Kind == SymbolKind.GlobalVariable)
            {
                if (generateCode) unsafe
                    {
                        var myVar = _LLVMglobals[v.Variable];
                        if (v.Variable.Type == TypeSymbol.Int)
                        {
                            var d = LLVM.BuildLoad2(_builder, LLVM.Int32Type(), myVar, StringToSBytePtr("load"));
                            _valueStack.Push(d);
                        }
                        else if (v.Variable.Type == TypeSymbol.Real)
                        {
                            var d = LLVM.BuildLoad2(_builder, LLVM.DoubleType(), myVar, StringToSBytePtr("load"));
                            _valueStack.Push(d);
                        }
                        else if (v.Variable.Type == TypeSymbol.Bool)
                        {
                            var d = LLVM.BuildLoad2(_builder, LLVM.Int1Type(), myVar, StringToSBytePtr("load"));
                            _valueStack.Push(d);
                        }
                    }
                return _globals[v.Variable];
            }
            else
            {
                var locals = _locals.Peek();
                if (generateCode) unsafe
                    {
                        var LLVMlocals = _LLVMlocals.Peek();
                        var myVar = LLVMlocals[v.Variable];
                        _valueStack.Push(myVar);
                        /*
                        if (v.Variable.Type == TypeSymbol.Int)
                        {
                            var d = LLVM.BuildLoad2(_builder, LLVM.Int32Type(), myVar, StringToSBytePtr("load"));
                            _valueStack.Push(d);
                        }
                        else if (v.Variable.Type == TypeSymbol.Real)
                        {
                            var d = LLVM.BuildLoad2(_builder, LLVM.DoubleType(), myVar, StringToSBytePtr("load"));
                            _valueStack.Push(d);
                        }
                        else if (v.Variable.Type == TypeSymbol.Bool)
                        {
                            var d = LLVM.BuildLoad2(_builder, LLVM.Int1Type(), myVar, StringToSBytePtr("load"));
                            _valueStack.Push(d);
                        }
                        */
                    }
                return locals[v.Variable];
            }

        }
        private object EvaluateAssignmentExpression(BoundAssignmentExpression a, bool generateCode)
        {
            var value = EvaluateExpression(a.Expression, generateCode);
            LLVMValueRef ret;
            if (generateCode) unsafe
                {
                    if (a.Variable.Kind == SymbolKind.GlobalVariable)
                    {

                        ret = _LLVMglobals[a.Variable];
                    }
                    else
                    {
                        var LLVMlocals = _LLVMlocals.Peek();
                        ret = LLVMlocals[a.Variable];
                    }
                    var last = _valueStack.Pop();
                    LLVM.BuildStore(_builder, last, ret);
                    _valueStack.Push(ret);
                }
            Assign(a.Variable, value, generateCode);
            return value;
        }
        private object EvaluateLiteralExpression(BoundLiteralExpression n, bool generateCode)
        {
            if (generateCode) unsafe
                {
                    if (n.Type == TypeSymbol.Int)
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
                            var node = LLVM.ConstInt(LLVM.Int32Type(), Convert.ToUInt32(value), 0);
                            _valueStack.Push(LLVM.ConstInt(LLVM.Int32Type(), Convert.ToUInt32(value), sign));
                        }
                    }
                    else if (n.Type == TypeSymbol.Real)
                    {
                        _valueStack.Push(LLVM.ConstReal(LLVM.DoubleType(), Convert.ToDouble(n.Value)));
                    }
                    else if (n.Type == TypeSymbol.Bool)
                    {
                        _valueStack.Push(LLVM.ConstInt(LLVM.Int1Type(), Convert.ToUInt32(n.Value), 0));
                    }
                }
            return n.Value;
        }
        private void Assign(VariableSymbol variable, object value, bool generateCode)
        {

            if (variable.Kind == SymbolKind.GlobalVariable)
            {
                _globals[variable] = value;
                if (generateCode)
                {
                    _LLVMglobals[variable] = _valueStack.Pop();
                }
            }
            else
            {
                var locals = _locals.Peek();
                locals[variable] = value;
                if (generateCode)
                {
                    var LLVMlocals = _LLVMlocals.Peek();
                    LLVMlocals[variable] = _valueStack.Pop();
                }
            }
        }
    }
}