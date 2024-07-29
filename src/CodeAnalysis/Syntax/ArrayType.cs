namespace ImperativeCompiler.CodeAnalysis.Syntax;

public sealed class ArrayType : TypeSyntax
{
    public ArrayType(SyntaxTree syntaxTree, SyntaxToken arrayKeyword, SyntaxToken openSquare, ExpressionSyntax size, SyntaxToken closeSquare, TypeSyntax type)
    : base(syntaxTree)
    {
        ArrayKeyword = arrayKeyword;
        OpenSquare = openSquare;
        Size = size;
        CloseSquare = closeSquare;
        Type = type;
    }

    public int FlatenArray(){
        if(Size is LiteralExpressionSyntax se){
            var ret  = (int)se.Value;
            if(Type is ArrayType a){
                return ret * a.FlatenArray();
            }
            return ret;
        }
        throw new Exception("Only constants are allowed in array dimensions");
    }

    public TypeSyntax GetPrimitive()
    {
        var myType = Type;
        while(myType is ArrayType t)
        {
            myType = t.Type;
        }

        return myType;
    }

    private SyntaxToken ArrayKeyword { get; }
    public SyntaxToken OpenSquare { get; }
    public ExpressionSyntax Size { get; }
    public SyntaxToken CloseSquare { get; }
    public TypeSyntax Type { get; }

    public override SyntaxKind Kind => SyntaxKind.ArrayType;

    public override SyntaxToken Identifier => ArrayKeyword;
}