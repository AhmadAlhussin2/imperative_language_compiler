namespace compilers.CodeAnalysis
{
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

        public SyntaxToken ArrayKeyword { get; }
        public SyntaxToken OpenSquare { get; }
        public ExpressionSyntax Size { get; }
        public SyntaxToken CloseSquare { get; }
        public TypeSyntax Type { get; }

        public override SyntaxKind Kind => SyntaxKind.ArrayType;

        public override SyntaxToken Identifier => ArrayKeyword;
    }

}