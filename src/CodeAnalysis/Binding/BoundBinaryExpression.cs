using ImperativeCompiler.CodeAnalysis.Symbols;
namespace ImperativeCompiler.CodeAnalysis.Binding;

internal sealed class BoundBinaryExpression : BoundExpression
{
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
    {
        Op = op;
        Left = left;
        Right = right;
    }
    public BoundExpression Left { get; }
    public BoundBinaryOperator Op { get; }
    public BoundExpression Right { get; }

    public override TypeSymbol Type => Op.Type;

    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
}