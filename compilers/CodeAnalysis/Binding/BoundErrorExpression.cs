using compilers.CodeAnalysis.Symbols;

namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundErrorExpression : BoundExpression
    {
        public override TypeSymbol Type => TypeSymbol.Error;

        public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
    }
}