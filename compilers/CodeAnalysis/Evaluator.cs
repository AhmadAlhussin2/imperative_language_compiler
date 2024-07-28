using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using compilers.CodeAnalysis.Binding;
using compilers.CodeAnalysis.Symbols;
using compilers.CodeAnalysis.Syntax;
using LLVMSharp.Interop;

namespace compilers.CodeAnalysis;

internal sealed class Evaluator
{
    private readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> _functionBodies;
    private readonly Dictionary<FunctionSymbol, LLVMValueRef> _llvmFunctions;
    private readonly Dictionary<FunctionSymbol, LLVMTypeRef> _llvmFunctionTypes;
    private readonly Dictionary<VariableSymbol, LLVMTypeRef> _llvmArrayTypes;
    private readonly BoundBlockStatement _root;
    private readonly Dictionary<VariableSymbol, object> _globals = new Dictionary<VariableSymbol, object>();
    private readonly Dictionary<VariableSymbol, LLVMValueRef> _llvmGlobals = new Dictionary<VariableSymbol, LLVMValueRef>();
    public readonly Stack<Dictionary<VariableSymbol, object>> Locals = new Stack<Dictionary<VariableSymbol, object>>();
    public readonly Stack<Dictionary<VariableSymbol, LLVMValueRef>> LlvmLocals = new Stack<Dictionary<VariableSymbol, LLVMValueRef>>();
    private object? _lastValue;
    private readonly LLVMBuilderRef _builder;
    private readonly Stack<LLVMValueRef> _valueStack = new Stack<LLVMValueRef>();
    public Evaluator
    (
        LLVMBuilderRef builder, ImmutableDictionary<FunctionSymbol,
        BoundBlockStatement> functionBodies,
        Dictionary<FunctionSymbol, LLVMValueRef> llvmFunctions,
        Dictionary<FunctionSymbol, LLVMTypeRef> llvmFunctionTypes,
        BoundBlockStatement root
    )
    {
        _functionBodies = functionBodies;
        _root = root;
        _builder = builder;
        _llvmFunctions = llvmFunctions;
        _llvmFunctionTypes = llvmFunctionTypes;
        _llvmArrayTypes = new();
    }
    static unsafe sbyte* StringToSBytePtr(string str)
    {
        // Convert the string to a byte array using UTF-8 encoding
        var bytes = Encoding.UTF8.GetBytes(str + '\0');

        // Allocate unmanaged memory to hold the null-terminated string
        var ptr = Marshal.AllocHGlobal(bytes.Length);

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
        var llvmLabel = new Dictionary<BoundLabel, LLVMBasicBlockRef>();
        for (var i = 0; i < body.Statements.Length; i++)
        {
            if (body.Statements[i] is BoundLabelStatement l)
            {
                labelToIndex.Add(l.Label, i + 1);
            }
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
                            if (!llvmLabel.ContainsKey(gotoStatement.Label))
                            {
                                LLVMBasicBlockRef block = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                                llvmLabel.Add(gotoStatement.Label, block);
                            }

                            LLVM.BuildBr(_builder, llvmLabel[gotoStatement.Label]);
                        }
                        index++;
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        var conditionalGotoStatement = (BoundConditionalGotoStatement)s;
                        EvaluateExpression(conditionalGotoStatement.Condition, true);

                        if (conditionalGotoStatement.JumpIfTrue) unsafe
                        {
                            LLVMBasicBlockRef falseblock = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                            if (!llvmLabel.ContainsKey(conditionalGotoStatement.Label))
                            {
                                LLVMBasicBlockRef block = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                                llvmLabel.Add(conditionalGotoStatement.Label, block);
                            }
                            LLVM.BuildCondBr(_builder, _valueStack.Pop(), llvmLabel[conditionalGotoStatement.Label], falseblock);
                            LLVM.PositionBuilderAtEnd(_builder, falseblock);

                        }
                        else unsafe
                        {
                            LLVMBasicBlockRef trueBlock = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                            if (!llvmLabel.ContainsKey(conditionalGotoStatement.Label))
                            {
                                LLVMBasicBlockRef block = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                                llvmLabel.Add(conditionalGotoStatement.Label, block);
                            }
                            LLVM.BuildCondBr(_builder, _valueStack.Pop(), trueBlock, llvmLabel[conditionalGotoStatement.Label]);
                            LLVM.PositionBuilderAtEnd(_builder, trueBlock);
                        }
                        index++;
                        break;

                    case BoundNodeKind.LabelStatement:
                        if (s is BoundLabelStatement l) unsafe
                        {
                            if (!llvmLabel.ContainsKey(l.Label))
                            {
                                LLVMBasicBlockRef block2 = LLVM.AppendBasicBlock(function, StringToSBytePtr("label"));
                                llvmLabel.Add(l.Label, block2);
                            }
                            var block = llvmLabel[l.Label];
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
    LLVMTypeRef Conv(TypeSymbol s)
    {
        if (s == TypeSymbol.Int) unsafe
        {
            return LLVM.Int32Type();
        }
        if (s == TypeSymbol.Real) unsafe
        {
            return LLVM.DoubleType();
        }
        if (s == TypeSymbol.Bool) unsafe
        {
            return LLVM.Int1Type();
        }
             
        throw new Exception("Unexpected Type");
            
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

            if (node.Variable.Type == TypeSymbol.Int)
            {
                var n = LLVM.BuildAlloca(_builder, LLVM.Int32Type(), StringToSBytePtr(node.Variable.Name));
                LLVM.BuildStore(_builder, lst, n);
                _valueStack.Push(n);
            }
            else if (node.Variable.Type == TypeSymbol.Real)
            {
                var n = LLVM.BuildAlloca(_builder, LLVM.DoubleType(), StringToSBytePtr(node.Variable.Name));
                LLVM.BuildStore(_builder, LLVM.ConstReal(LLVM.DoubleType(), Convert.ToDouble(value)), n);
                _valueStack.Push(n);
            }
            else if (node.Variable.Type == TypeSymbol.Bool)
            {
                LLVMValueRef b = LLVM.BuildAlloca(_builder, LLVM.Int1Type(), StringToSBytePtr(node.Variable.Name));
                LLVM.BuildStore(_builder, LLVM.ConstInt(LLVM.Int1Type(), Convert.ToUInt32(value), 0), b);
                _valueStack.Push(b);
            }
            else if (node.Variable.Type.Name == "array")
            {
                int arraySize = 0;
                var type = TypeSymbol.Int;
                if (node.TypeClause is ArrayType a)
                {
                    arraySize = a.FlatenArray();

                    var x = a.GetPrimitive();
                    if (x is PrimitiveType y)
                    {
                        if (y.Identifier.Kind == SyntaxKind.RealKeyword)
                        {
                            type = TypeSymbol.Real;
                        }
                        else if (y.Identifier.Kind == SyntaxKind.BooleanKeyword)
                        {
                            type = TypeSymbol.Bool;
                        }
                    }
                }
                if (arraySize == 0)
                {
                    throw new Exception("Can not declare empty array");
                }
                var arrayType = LLVM.ArrayType(Conv(type), (uint)arraySize);
                _llvmArrayTypes[node.Variable] = arrayType;
                var arrayAlloca = LLVM.BuildAlloca(_builder, arrayType, StringToSBytePtr(node.Variable.Name));
                _valueStack.Push(arrayAlloca);



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
        if (node.Type == TypeSymbol.Int)
            return Convert.ToInt32(value);
        return value;
    }
    private object? EvaluateCallExpression(BoundCallExpression node, bool generateCode)
    {
        if (node.Function == BuiltinFunctions.PrintInt)
        {
            var message = EvaluateExpression(node.Arguments[0], generateCode).ToString();
            Console.WriteLine(message);
            return null;
        }
        unsafe
        {
            if (generateCode)
            {
                var locals = new Dictionary<VariableSymbol, object>();
                var paramValues = new LLVMOpaqueValue*[node.Arguments.Length];
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
                    callFunction = LLVM.BuildCall2(_builder, _llvmFunctionTypes[node.Function], _llvmFunctions[node.Function], ptr, 1, StringToSBytePtr("call"));
                }
                _valueStack.Push(callFunction);

                Locals.Push(locals);
                var statement = _functionBodies[node.Function];
                // var result = Evaluate(statement);
                var result = Evaluate(statement, _llvmFunctions[node.Function], false);
                Locals.Pop();
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

                Locals.Push(locals);
                var statement = _functionBodies[node.Function];
                // var result = Evaluate(statement);
                var result = Evaluate(statement, null, false);
                Locals.Pop();
                return result;
            }


        }
    }
    private object EvaluateBinaryExpression(BoundBinaryExpression b, bool generateCode)
    {
        var left = EvaluateExpression(b.Left, generateCode);
        var right = EvaluateExpression(b.Right, generateCode);
        LLVMValueRef rightLlvm = null;
        LLVMValueRef leftLlvm = null;
        if (generateCode)
        {
            rightLlvm = _valueStack.Pop();
            leftLlvm = _valueStack.Pop();
        }
        switch (b.Op.Kind)
        {
            case BoundBinaryOperatorKind.Addition:
                if (left is double || right is double)
                {
                    if (generateCode) unsafe
                    {

                        var ret = LLVM.BuildFAdd(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempAdd"));
                        _valueStack.Push(ret);
                    }
                    return (double)left + (double)right;
                }
                if (generateCode) unsafe
                {
                    var ret = LLVM.BuildAdd(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempAdd"));
                    _valueStack.Push(ret);
                }
                return (int)left + (int)right;
            case BoundBinaryOperatorKind.Subtraction:
                if (left is double || right is double)
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildFSub(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempSub"));
                        _valueStack.Push(ret);

                    }
                    return (double)left - (double)right;
                }
                else
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildSub(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempSub"));
                        _valueStack.Push(ret);
                    }
                    return (int)left - (int)right;
                }
            case BoundBinaryOperatorKind.Multiplication:
                if (left is double || right is double)
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildFMul(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempMul"));
                        _valueStack.Push(ret);
                    }
                    return (double)left * (double)right;
                }
                else
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildMul(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempMul"));
                        _valueStack.Push(ret);
                    }
                    return (int)left * (int)right;
                }
            case BoundBinaryOperatorKind.Division:
                if (left is double || right is double)
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildFDiv(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempDiv"));
                        _valueStack.Push(ret);
                    }
                    return (double)left / (double)right;
                }
                else
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildSDiv(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempDiv"));
                        _valueStack.Push(ret);
                    }
                    return (int)left / (int)right;
                }
            case BoundBinaryOperatorKind.LogicalAnd:
                if (generateCode) unsafe
                {
                    var ret = LLVM.BuildAnd(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempAnd"));
                    _valueStack.Push(ret);
                }
                return (bool)left & (bool)right;
            case BoundBinaryOperatorKind.LogicalXor:
                if (generateCode) unsafe
                {
                    var ret = LLVM.BuildXor(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempXor"));
                    _valueStack.Push(ret);
                }
                return (bool)left ^ (bool)right;
            case BoundBinaryOperatorKind.LogicalOr:
                if (generateCode) unsafe
                {
                    var ret = LLVM.BuildOr(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempOr"));
                    _valueStack.Push(ret);
                }
                return (bool)left | (bool)right;
            case BoundBinaryOperatorKind.Equal:
                if (generateCode) unsafe
                {
                    LLVMValueRef ret;
                    if (left is double && right is double)
                    {
                        ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOEQ, leftLlvm, rightLlvm, StringToSBytePtr("tempEq"));
                    }
                    else
                    {
                        ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntEQ, leftLlvm, rightLlvm, StringToSBytePtr("tempEq"));
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
                        ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealONE, leftLlvm, rightLlvm, StringToSBytePtr("tempNotEq"));
                    }
                    else
                    {
                        ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntNE, leftLlvm, rightLlvm, StringToSBytePtr("tempNotEq"));
                    }
                    _valueStack.Push(ret);
                }
                return !Equals(left, right);
            case BoundBinaryOperatorKind.LessThan:
                if (left is double || right is double)
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOLT, leftLlvm, rightLlvm, StringToSBytePtr("tempLess"));
                        _valueStack.Push(ret);
                    }
                    return (double)left < (double)right;
                }
                else
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSLT, leftLlvm, rightLlvm, StringToSBytePtr("tempLess"));
                        _valueStack.Push(ret);
                    }
                    return (int)left < (int)right;
                }
            case BoundBinaryOperatorKind.LessThanOrEqual:
                if (left is double || right is double)
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOLE, leftLlvm, rightLlvm, StringToSBytePtr("tempLess"));
                        _valueStack.Push(ret);
                    }
                    return (double)left <= (double)right;
                }
                else
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSLE, leftLlvm, rightLlvm, StringToSBytePtr("tempLess"));
                        _valueStack.Push(ret);
                    }
                    return (int)left <= (int)right;
                }
            case BoundBinaryOperatorKind.GreaterThan:
                if (left is double || right is double)
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOGT, leftLlvm, rightLlvm, StringToSBytePtr("tempLess"));
                        _valueStack.Push(ret);
                    }
                    return (double)left > (double)right;
                }
                else
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSGT, leftLlvm, rightLlvm, StringToSBytePtr("tempLess"));
                        _valueStack.Push(ret);
                    }
                    return (int)left > (int)right;
                }
            case BoundBinaryOperatorKind.GreaterThanOrEqual:
                if (left is double || right is double)
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildFCmp(_builder, LLVMRealPredicate.LLVMRealOGE, leftLlvm, rightLlvm, StringToSBytePtr("tempLess"));
                        _valueStack.Push(ret);
                    }
                    return (double)left >= (double)right;
                }
                else
                {
                    if (generateCode) unsafe
                    {
                        var ret = LLVM.BuildICmp(_builder, LLVMIntPredicate.LLVMIntSGE, leftLlvm, rightLlvm, StringToSBytePtr("tempLess"));
                        _valueStack.Push(ret);
                    }
                    return (int)left >= (int)right;
                }
            case BoundBinaryOperatorKind.Modulo:
                if (generateCode) unsafe
                {
                    var ret = LLVM.BuildSRem(_builder, leftLlvm, rightLlvm, StringToSBytePtr("tempRem"));
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
        var operandLlvm = _valueStack.Pop();
        switch (u.Op.Kind)
        {
            case BoundUnaryOperatorKind.Negation:
                if (u.Operand.Type == TypeSymbol.Int) unsafe
                {
                    if (generateCode)
                    {
                        var ret = LLVM.BuildSub(_builder, LLVM.ConstInt(LLVM.Int32Type(), 0, 0), operandLlvm, StringToSBytePtr("tempNegation"));
                        _valueStack.Push(ret);
                    }
                    return -(int)operand;
                }
                if (u.Operand.Type == TypeSymbol.Real) unsafe
                {
                    if (generateCode)
                    {
                        var ret = LLVM.BuildFSub(_builder, LLVM.ConstReal(LLVM.DoubleType(), 0), operandLlvm, StringToSBytePtr("tempNegation"));
                        _valueStack.Push(ret);
                    }
                    return -(double)operand;
                }
                throw new Exception($"Operator - is not defined for {u.Op}");
            case BoundUnaryOperatorKind.Identity:
                if (u.Operand.Type == TypeSymbol.Int) unsafe
                {
                    if (generateCode)
                    {
                        var ret = LLVM.BuildFAdd(_builder, LLVM.ConstInt(LLVM.Int32Type(), 0, 0), operandLlvm, StringToSBytePtr("tempNegation"));
                        _valueStack.Push(ret);
                    }
                    return -(int)operand;
                }
                if (u.Operand.Type == TypeSymbol.Real) unsafe
                {
                    if (generateCode)
                    {
                        var ret = LLVM.BuildAdd(_builder, LLVM.ConstReal(LLVM.DoubleType(), 0), operandLlvm, StringToSBytePtr("tempNegation"));
                        _valueStack.Push(ret);
                    }
                    return -(double)operand;
                }
                return (int)operand;
            case BoundUnaryOperatorKind.LogicalNegation:
                if (generateCode) unsafe
                {
                    var ret = LLVM.BuildXor(_builder, LLVM.ConstInt(LLVM.Int1Type(), 0, 0), operandLlvm, StringToSBytePtr("tempNegation"));
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
                var myVar = _llvmGlobals[v.Variable];
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
                else if (v.Indicies != null && v.Type.Dimensions != null)
                {
                    Console.WriteLine(v.Variable.Type.Leaf());
                    var indexes = new List<LLVMValueRef>();
                    foreach (var expr in v.Indicies)
                    {
                        EvaluateExpression(expr, true);
                        var reff = _valueStack.Pop();
                        indexes.Add(reff);
                    }

                    int len = v.Type.Dimensions.Count;
                    if (len != indexes.Count)
                    {
                        throw new Exception("invalid dimensions count");
                    }
                    var prod = LLVM.BuildMul(_builder, LLVM.ConstInt(LLVM.Int32Type(), 1, 0), LLVM.ConstInt(LLVM.Int32Type(), 1, 0), StringToSBytePtr("prod"));
                    var hash = LLVM.BuildMul(_builder, LLVM.ConstInt(LLVM.Int32Type(), 0, 0), LLVM.ConstInt(LLVM.Int32Type(), 0, 0), StringToSBytePtr("prod"));
                    for (int i = len - 1; i >= 0; i--)
                    {
                        var index = indexes[i];
                        var mul = LLVM.BuildMul(_builder, prod, index, StringToSBytePtr("dxi"));
                        hash = LLVM.BuildAdd(_builder, mul, hash, StringToSBytePtr("nextSum"));
                        prod = LLVM.BuildMul(_builder, prod, LLVM.ConstInt(LLVM.Int32Type(), (ulong)v.Type.Dimensions[i], 0), StringToSBytePtr("dxi"));
                    }
                    var lst = new [] { LLVM.ConstInt(LLVM.Int32Type(), 0, 0), hash };
                    LLVMOpaqueValue** indexesLst;
                    fixed (LLVMOpaqueValue** ptr = lst)
                    {
                        indexesLst = ptr;
                    }
                    var elementPtr = LLVM.BuildGEP2(_builder, _llvmArrayTypes[v.Variable], myVar, indexesLst, 2, StringToSBytePtr("ep"));


                    var d = LLVM.BuildLoad2(_builder, Conv(v.Variable.Type.Leaf()), elementPtr, StringToSBytePtr("load"));
                    _valueStack.Push(d);
                }
            }
            return _globals[v.Variable];
        }
        var locals = Locals.Peek();
        if (generateCode)
        {
            var llvmLocals = LlvmLocals.Peek();
            var myVar = llvmLocals[v.Variable];
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
    private object EvaluateAssignmentExpression(BoundAssignmentExpression a, bool generateCode)
    {
        var value = EvaluateExpression(a.Expression, generateCode);
        if (generateCode) unsafe
        {
            LLVMValueRef ret;
            if (a.Variable.Kind == SymbolKind.GlobalVariable)
            {

                ret = _llvmGlobals[a.Variable];
            }
            else
            {
                var llvmlocals = LlvmLocals.Peek();
                ret = llvmlocals[a.Variable];
            }

            if (a.ExactVar == null || a.ExactVar.Indicies == null || a.ExactVar.Type.Dimensions == null)
            {
                var last = _valueStack.Pop();
                LLVM.BuildStore(_builder, last, ret);
                _valueStack.Push(ret);
            }
            else
            {
                var last = _valueStack.Pop();
                var indexes = new List<LLVMValueRef>();

                foreach (var expr in a.ExactVar.Indicies)
                {
                    EvaluateExpression(expr, true);
                    var reff = _valueStack.Pop();
                    indexes.Add(reff);
                }

                var len = a.ExactVar.Type.Dimensions.Count;
                if (len != indexes.Count)
                {
                    throw new Exception("invalid dimensions count");
                }
                var prod = LLVM.BuildMul(_builder, LLVM.ConstInt(LLVM.Int32Type(), 1, 0), LLVM.ConstInt(LLVM.Int32Type(), 1, 0), StringToSBytePtr("prod"));
                var hash = LLVM.BuildMul(_builder, LLVM.ConstInt(LLVM.Int32Type(), 0, 0), LLVM.ConstInt(LLVM.Int32Type(), 0, 0), StringToSBytePtr("prod"));
                for (var i = len - 1; i >= 0; i--)
                {
                    var mul = LLVM.BuildMul(_builder, prod, indexes[i], StringToSBytePtr("dxi"));
                    hash = LLVM.BuildAdd(_builder, mul, hash, StringToSBytePtr("nextSum"));
                    prod = LLVM.BuildMul(_builder, prod, LLVM.ConstInt(LLVM.Int32Type(), (ulong)a.ExactVar.Type.Dimensions[i], 0), StringToSBytePtr("dxi"));
                }
                var lst = new[] { LLVM.ConstInt(LLVM.Int32Type(), 0, 0), hash };
                LLVMOpaqueValue** indexesLst;
                fixed (LLVMOpaqueValue** ptr = lst)
                {
                    indexesLst = ptr;
                }
                var elementPtr = LLVM.BuildGEP2(_builder, _llvmArrayTypes[a.Variable], ret, indexesLst, 2, StringToSBytePtr("ep"));
                LLVM.BuildStore(_builder, last, elementPtr);

                _valueStack.Push(ret);
            }
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
                var sign = (int)n.Value < 0 ? 1 : 0;
                var value = (int)n.Value;
                if (sign == 1)
                {
                    value *= -1;
                    _valueStack.Push(LLVM.ConstInt(LLVM.Int32Type(), (ulong)-Convert.ToUInt32(value), sign));
                }
                else
                {
                    LLVM.ConstInt(LLVM.Int32Type(), Convert.ToUInt32(value), 0);
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
                _llvmGlobals[variable] = _valueStack.Pop();
            }
        }
        else
        {
            var locals = Locals.Peek();
            locals[variable] = value;
            if (generateCode)
            {
                var llvmLocals = this.LlvmLocals.Peek();
                llvmLocals[variable] = _valueStack.Pop();
            }
        }
    }
}