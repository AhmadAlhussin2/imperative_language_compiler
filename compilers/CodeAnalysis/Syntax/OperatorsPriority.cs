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
                    return 8;
                default:
                    return 0;
            }
        }
        public static string GetOperatorText(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.NegationToken:
                    return "not ";
                case SyntaxKind.StarToken:
                    return "*";
                case SyntaxKind.SlashToken:
                    return "/";
                case SyntaxKind.ModuloToken:
                    return "%";
                case SyntaxKind.EqualToken:
                    return "=";
                case SyntaxKind.NotEqualToken:
                    return "/=";
                case SyntaxKind.LessThanToken:
                    return "<";
                case SyntaxKind.LessThanOrEqualToken:
                    return "<=";
                case SyntaxKind.GreaterThanToken:
                    return ">";
                case SyntaxKind.GreaterThanOrEqualToken:
                    return ">=";
                case SyntaxKind.AndKeyword:
                    return "and";
                case SyntaxKind.OrKeyword:
                    return "or";
                case SyntaxKind.XorKeyword:
                    return "xor";
                default:
                    throw new Exception("Unknown token");
            }
        }
        public static int GetBinaryOperatorPriority(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 7;
                case SyntaxKind.ModuloToken:
                    return 6;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 5;
                case SyntaxKind.EqualToken:
                case SyntaxKind.NotEqualToken:
                case SyntaxKind.LessThanToken:
                case SyntaxKind.LessThanOrEqualToken:
                case SyntaxKind.GreaterThanToken:
                case SyntaxKind.GreaterThanOrEqualToken:
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