using compilers.CodeAnalysis.Symbols;

namespace compilers.CodeAnalysis.Binding
{
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
}