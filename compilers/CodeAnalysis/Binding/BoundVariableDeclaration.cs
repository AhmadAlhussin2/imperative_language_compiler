using compilers.CodeAnalysis.Symbol;

namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration(VariableSymbol variable, BoundExpression initializer, TypeSymbol? type=null)
        {
            Variable = variable;
            Initializer = initializer;
            Type = type;
        }

        public VariableSymbol Variable { get; }
        public BoundExpression Initializer { get; }
        public TypeSymbol? Type { get; }

        public override BoundNodeKind Kind => BoundNodeKind.VariableDeclaration;
    }
}