namespace compilers.CodeAnalysis.Symbol
{
    public class VariableSymbol : Symbol
    {
        internal VariableSymbol(string name, TypeSymbol type) : base(name)
        {
            Type = type;
        }

        public TypeSymbol Type { get; }

        public override SymbolKind Kind => SymbolKind.Variable;
    }
}