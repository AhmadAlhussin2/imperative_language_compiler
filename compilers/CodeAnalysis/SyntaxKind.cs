namespace compilers.CodeAnalysis
{
    public enum SyntaxKind
    {
        NumberToken,
        WhiteSpace,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        UnknowToken,
        EOFToken,
        LiteralExpression,
        BinaryExpression,
        ParenthesizedExpression,
        UnaryExpression
    }
}