using ImperativeCompiler.CodeAnalysis.Symbols;
namespace ImperativeCompiler.CodeAnalysis.Binding;

internal sealed class BoundVariableExpression : BoundExpression
{
    public BoundVariableExpression(VariableSymbol variable, List<BoundExpression>? indicies = null)
    {
        Variable = variable;
        Indicies = indicies;
    }

    public VariableSymbol Variable { get; }
    public List<BoundExpression>? Indicies { get; }

    public override TypeSymbol Type => Variable.Type;

    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
}