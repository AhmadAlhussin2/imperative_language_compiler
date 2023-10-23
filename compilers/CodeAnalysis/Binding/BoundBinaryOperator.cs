namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type type)
        : this(syntaxKind, kind, type, type, type)
        {

        }
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type type, Type resultType)
        : this(syntaxKind, kind, type, type, resultType)
        {

        }
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type leftType, Type rightType, Type resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            Type = resultType;
        }

        public SyntaxKind SyntaxKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public Type LeftType { get; }
        public Type RightType { get; }
        public Type Type { get; }

        private static BoundBinaryOperator[] _operators = {
            new BoundBinaryOperator(SyntaxKind.PlusToken,BoundBinaryOperatorKind.Addition,typeof(int)),
            new BoundBinaryOperator(SyntaxKind.MinusToken,BoundBinaryOperatorKind.Subtraction,typeof(int)),
            new BoundBinaryOperator(SyntaxKind.StarToken,BoundBinaryOperatorKind.Multiplication,typeof(int)),
            new BoundBinaryOperator(SyntaxKind.SlashToken,BoundBinaryOperatorKind.Division,typeof(int)),

            new BoundBinaryOperator(SyntaxKind.EqualToken,BoundBinaryOperatorKind.Equal,typeof(int),typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.NotEqualToken,BoundBinaryOperatorKind.NotEqual,typeof(int),typeof(bool)),

            new BoundBinaryOperator(SyntaxKind.LessThanToken,BoundBinaryOperatorKind.LessThan,typeof(int),typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.LessThanOrEqualToken,BoundBinaryOperatorKind.LessThanOrEqual,typeof(int),typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.GreaterThanToken,BoundBinaryOperatorKind.GreaterThan,typeof(int),typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.GreaterThanOrEqualToken,BoundBinaryOperatorKind.GreaterThanOrEqual,typeof(int),typeof(bool)),

            new BoundBinaryOperator(SyntaxKind.AndKeyword,BoundBinaryOperatorKind.LogicalAnd,typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.OrKeyword,BoundBinaryOperatorKind.LogicalOr,typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.XorKeyword,BoundBinaryOperatorKind.LogicalXor,typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.EqualToken,BoundBinaryOperatorKind.Equal,typeof(bool)),
            new BoundBinaryOperator(SyntaxKind.NotEqualToken,BoundBinaryOperatorKind.NotEqual,typeof(bool)),

        };

        public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, Type leftType, Type rightType)
        {
            foreach (var op in _operators)
            {
                if (op.SyntaxKind == syntaxKind && op.LeftType == leftType && op.RightType == rightType)
                {
                    return op;
                }
            }
            return null;
        }
    }
}