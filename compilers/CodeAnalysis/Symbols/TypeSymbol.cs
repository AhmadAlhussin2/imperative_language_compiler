namespace compilers.CodeAnalysis.Symbol
{
    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Int = new("int");
        public static readonly TypeSymbol Bool = new("bool");
        public static readonly TypeSymbol Real = new("real");
        public static readonly TypeSymbol Error = new("?");
        public static readonly TypeSymbol Void = new("void");
        
        internal TypeSymbol(string name) : base(name)
        {

        }

        public override SymbolKind Kind => SymbolKind.Type;
    }
}