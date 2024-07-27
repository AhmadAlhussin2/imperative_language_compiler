using compilers.CodeAnalysis.Symbols;

namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand)
        {
            Op = op;
            Operand = operand;
        }
        public BoundUnaryOperator Op { get; }
        public BoundExpression Operand { get; }

        public override TypeSymbol Type => Op.Type;

        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    }
}