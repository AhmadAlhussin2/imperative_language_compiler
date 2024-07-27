using compilers.CodeAnalysis.Symbols;

namespace compilers.CodeAnalysis.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract TypeSymbol Type { get; }
    }
}