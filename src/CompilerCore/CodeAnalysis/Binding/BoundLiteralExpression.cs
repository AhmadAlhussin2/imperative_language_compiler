using ImperativeCompiler.CodeAnalysis.Symbols;
namespace ImperativeCompiler.CodeAnalysis.Binding;

internal sealed class BoundLiteralExpression : BoundExpression
{
    public BoundLiteralExpression(object value)
    {
        Value = value;
        switch (value)
        {
            case bool:
                Type = TypeSymbol.Bool;
                break;
            case int:
                Type = TypeSymbol.Int;
                break;
            case double:
                Type = TypeSymbol.Real;
                break;
            default:
                throw new Exception($"Unexpected literal {value} of type {value.GetType()}");
        }
    }
    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    public override TypeSymbol Type { get; }
    public object Value { get; }
}