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
        public void WriteTo(TextWriter writer)
        {
            SymbolPrinter.WriteTo(this, writer);
        }
    }
}