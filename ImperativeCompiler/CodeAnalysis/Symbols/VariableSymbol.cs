namespace compilers.CodeAnalysis.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        internal VariableSymbol(string name, TypeSymbol type) : base(name)
        {
            Type = type;
        }

        public TypeSymbol Type { get; }
    }
}