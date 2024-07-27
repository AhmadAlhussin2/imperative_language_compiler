namespace compilers.CodeAnalysis.Symbols
{
    public class LocalVariableSymbol : VariableSymbol
    {
        internal LocalVariableSymbol(string name, TypeSymbol type) 
            : base(name, type)
        {
            
        }

        public override SymbolKind Kind => SymbolKind.GlobalVariable;
    }
}