using ImperativeCompiler.CodeAnalysis.Symbols;
namespace ImperativeCompiler.CodeAnalysis.Binding;

internal abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get; }
}