using compilers.CodeAnalysis.Syntax;
namespace compilers.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Int = new("integer");
        public static readonly TypeSymbol Bool = new("boolean");
        public static readonly TypeSymbol Real = new("real");
        public static readonly TypeSymbol Error = new("?");
        public static readonly TypeSymbol Void = new("void");
        public static readonly TypeSymbol Array = new("array");

        internal TypeSymbol(string name, ExpressionSyntax? size = null, TypeSymbol? type = null, List<int>? dimensions=null) : base(name)
        {
            Size = size;
            Type = type;
            Dimensions = dimensions;
        }

        public override SymbolKind Kind => SymbolKind.Type;
        public TypeSymbol Leaf()
        {
            if(Type==null)return this;
            return Type;
        }
        public ExpressionSyntax? Size { get; }
        public TypeSymbol? Type { get; }
        public List<int>? Dimensions { get; }

        public override string ToString()
        {
            int size = 0;
            if (Size is LiteralExpressionSyntax s)
                size = (int)s.Value;
            return Name + (Size != null ? $"[{size}] " : "") + (Type != null ? Type.ToString() : "");
        }
    }
}