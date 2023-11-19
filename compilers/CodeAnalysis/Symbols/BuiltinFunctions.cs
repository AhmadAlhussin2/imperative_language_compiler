using System.Collections.Immutable;
using System.Reflection;

namespace compilers.CodeAnalysis.Symbol
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol PrintInt = new FunctionSymbol 
        (
            "printInt", 
            ImmutableArray.Create(new ParameterSymbol("value", TypeSymbol.Int)), 
            TypeSymbol.Void
        );
        public static readonly FunctionSymbol ReadInt = new FunctionSymbol 
        (
            "readInt", 
            ImmutableArray<ParameterSymbol>.Empty, 
            TypeSymbol.Int
        );

        internal static IEnumerable<FunctionSymbol> GetAll() 
            => typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                                       .Where(f => f.FieldType == typeof(FunctionSymbol))
                                       .Select(f => (FunctionSymbol)f.GetValue(null)!)!;
    }
}