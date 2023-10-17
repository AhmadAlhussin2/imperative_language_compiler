namespace compilers.CodeAnalysis
{
    public static class OperatorsPriority
    {
        public static int GetUnaryOperatorPriority(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.NegationToken:
                    return 7;
                default:
                    return 0;
            }
        }
        public static int GetBinaryOperatorPriority(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 6;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 5;
                case SyntaxKind.EqualToken:
                case SyntaxKind.NotEqualToken:
                    return 4;
                case SyntaxKind.AndKeyword:
                    return 3;
                case SyntaxKind.OrKeyword:
                    return 2;
                case SyntaxKind.XorKeyword:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}