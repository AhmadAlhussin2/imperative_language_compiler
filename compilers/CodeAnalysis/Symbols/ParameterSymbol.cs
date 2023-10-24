namespace compilers.CodeAnalysis.Symbol
{
    public sealed class ParameterSymbol : VariableSymbol
    {
        public ParameterSymbol(string name, TypeSymbol type) 
            : base(name, type)
        {

        }
        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}