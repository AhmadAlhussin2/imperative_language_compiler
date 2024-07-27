namespace compilers.CodeAnalysis.Syntax
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
        public static string? GetText(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.NegationToken:
                    return "not";
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
                case SyntaxKind.ArrayKeyword:
                    return "array";
                case SyntaxKind.IfKeyword:
                    return "if";
                case SyntaxKind.InKeyword:
                    return "in";
                case SyntaxKind.IsKeyword:
                    return "is";
                case SyntaxKind.EndKeyword:
                    return "end";
                case SyntaxKind.ForKeyword:
                    return "for";
                case SyntaxKind.NotKeyword:
                    return "not";
                case SyntaxKind.VarKeyword:
                    return "var";
                case SyntaxKind.ElseKeyword:
                    return "else";
                case SyntaxKind.LoopKeyword:
                    return "loop";
                case SyntaxKind.RealKeyword:
                    return "real";
                case SyntaxKind.ThenKeyword:
                    return "then";
                case SyntaxKind.TrueKeyword:
                    return "true";
                case SyntaxKind.TypeKeyword:
                    return "type";
                case SyntaxKind.FalseKeyword:
                    return "false";
                case SyntaxKind.WhileKeyword:
                    return "while";
                case SyntaxKind.RecordKeyword:
                    return "record";
                case SyntaxKind.ReturnKeyword:
                    return "return";
                case SyntaxKind.BooleanKeyword:
                    return "boolean";
                case SyntaxKind.IntegerKeyword:
                    return "integer";
                case SyntaxKind.ReverseKeyword:
                    return "reverse";
                case SyntaxKind.RoutineKeyword:
                    return "routine";
                default:
                    return null;
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