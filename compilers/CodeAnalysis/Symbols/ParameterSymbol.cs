namespace compilers.CodeAnalysis.Symbol
{
    public sealed class ParameterSymbol : LocalVariableSymbol
    {
        public ParameterSymbol(string name, TypeSymbol type) 
            : base(name, type)
        {

        }
        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}