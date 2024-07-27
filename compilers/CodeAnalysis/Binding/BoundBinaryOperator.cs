using compilers.CodeAnalysis.Symbols;
using compilers.CodeAnalysis.Syntax;

namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryOperator
    {
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol type)
        : this(syntaxKind, kind, type, type, type)
        {

        }
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol type, TypeSymbol resultType)
        : this(syntaxKind, kind, type, type, resultType)
        {

        }
        private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
        {
            SyntaxKind = syntaxKind;
            Kind = kind;
            LeftType = leftType;
            RightType = rightType;
            Type = resultType;
        }

        public SyntaxKind SyntaxKind { get; }
        public BoundBinaryOperatorKind Kind { get; }
        public TypeSymbol LeftType { get; }
        public TypeSymbol RightType { get; }
        public TypeSymbol Type { get; }

        private static readonly BoundBinaryOperator[] Operators = {
            new BoundBinaryOperator(SyntaxKind.PlusToken,BoundBinaryOperatorKind.Addition,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.PlusToken,BoundBinaryOperatorKind.Addition,TypeSymbol.Real),

            new BoundBinaryOperator(SyntaxKind.MinusToken,BoundBinaryOperatorKind.Subtraction,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.MinusToken,BoundBinaryOperatorKind.Subtraction,TypeSymbol.Real),

            new BoundBinaryOperator(SyntaxKind.StarToken,BoundBinaryOperatorKind.Multiplication,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.StarToken,BoundBinaryOperatorKind.Multiplication,TypeSymbol.Real),

            new BoundBinaryOperator(SyntaxKind.SlashToken,BoundBinaryOperatorKind.Division,TypeSymbol.Int),
            new BoundBinaryOperator(SyntaxKind.SlashToken,BoundBinaryOperatorKind.Division,TypeSymbol.Real),

            new BoundBinaryOperator(SyntaxKind.ModuloToken,BoundBinaryOperatorKind.Modulo,TypeSymbol.Int),

            new BoundBinaryOperator(SyntaxKind.EqualToken,BoundBinaryOperatorKind.Equal,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.EqualToken,BoundBinaryOperatorKind.Equal,TypeSymbol.Real,TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.NotEqualToken,BoundBinaryOperatorKind.NotEqual,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.NotEqualToken,BoundBinaryOperatorKind.NotEqual,TypeSymbol.Real,TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.LessThanToken,BoundBinaryOperatorKind.LessThan,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.LessThanToken,BoundBinaryOperatorKind.LessThan,TypeSymbol.Real,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.LessThanOrEqualToken,BoundBinaryOperatorKind.LessThanOrEqual,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.LessThanOrEqualToken,BoundBinaryOperatorKind.LessThanOrEqual,TypeSymbol.Real,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.GreaterThanToken,BoundBinaryOperatorKind.GreaterThan,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.GreaterThanToken,BoundBinaryOperatorKind.GreaterThan,TypeSymbol.Real,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.GreaterThanOrEqualToken,BoundBinaryOperatorKind.GreaterThanOrEqual,TypeSymbol.Int,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.GreaterThanOrEqualToken,BoundBinaryOperatorKind.GreaterThanOrEqual,TypeSymbol.Real,TypeSymbol.Bool),

            new BoundBinaryOperator(SyntaxKind.AndKeyword,BoundBinaryOperatorKind.LogicalAnd,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.OrKeyword,BoundBinaryOperatorKind.LogicalOr,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.XorKeyword,BoundBinaryOperatorKind.LogicalXor,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.EqualToken,BoundBinaryOperatorKind.Equal,TypeSymbol.Bool),
            new BoundBinaryOperator(SyntaxKind.NotEqualToken,BoundBinaryOperatorKind.NotEqual,TypeSymbol.Bool),

        };

        public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
        {
            foreach (var op in Operators)
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