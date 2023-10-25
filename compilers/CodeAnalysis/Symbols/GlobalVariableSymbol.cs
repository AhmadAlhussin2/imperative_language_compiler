namespace compilers.CodeAnalysis.Symbol
{
    public sealed class GlobalVariableSymbol : VariableSymbol
    {
        internal GlobalVariableSymbol(string name, TypeSymbol type) 
            : base(name, type)
        {
            
        }

        public override SymbolKind Kind => SymbolKind.LocalVariable;
    }
}