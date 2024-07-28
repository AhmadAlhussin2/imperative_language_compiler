using System.Collections.Immutable;
using ImperativeCompiler.CodeAnalysis.Syntax;
namespace ImperativeCompiler.CodeAnalysis.Symbols;

public sealed class FunctionSymbol : Symbol
{
    public FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type, FunctionDeclarationSyntax? decleration = null)
    : base(name)
    {
        Parameters = parameters;
        Type = type;
        Decleration = decleration;
    }
    public override SymbolKind Kind => SymbolKind.Function;

    public ImmutableArray<ParameterSymbol> Parameters { get; }
    public TypeSymbol Type { get; }
    public FunctionDeclarationSyntax? Decleration { get; }
}