namespace compilers.CodeAnalysis.Symbol
{
    public abstract class Symbol
    {
        private protected Symbol(string name)
        {
            Name = name;
        }
        public abstract SymbolKind Kind { get; }
        public string Name { get; }
        public override string ToString()
        {
            return Name;
        }
    }
}