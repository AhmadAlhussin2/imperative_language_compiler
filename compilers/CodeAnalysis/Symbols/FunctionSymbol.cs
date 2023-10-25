using System.Collections.Immutable;

namespace compilers.CodeAnalysis.Symbol
{
    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type, FunctionDeclerationSyntax? decleration = null)
            : base(name)
        {
            Parameters = parameters;
            Type = type;
            Decleration = decleration;
        }
        public override SymbolKind Kind => SymbolKind.Function;

        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol Type { get; }
        public FunctionDeclerationSyntax? Decleration { get; }
    }
}