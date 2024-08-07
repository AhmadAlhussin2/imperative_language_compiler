using ImperativeCompiler.CodeAnalysis.Symbols;
using ImperativeCompiler.CodeAnalysis.Syntax;
namespace ImperativeCompiler.CodeAnalysis.Binding;

internal sealed class BoundVariableDeclaration : BoundStatement
{
    public BoundVariableDeclaration(VariableSymbol variable, BoundExpression initializer, TypeSyntax? typeClause=null, TypeSymbol? type=null)
    {
        Variable = variable;
        Initializer = initializer;
        TypeClause = typeClause;
        Type = type;
    }
 
    public VariableSymbol Variable { get; }
    public BoundExpression Initializer { get; }
    public TypeSyntax? TypeClause { get; }
    public TypeSymbol? Type { get; }

    public override BoundNodeKind Kind => BoundNodeKind.VariableDeclaration;
}