namespace ImperativeCompiler.CodeAnalysis.Syntax;

public class SyntaxFacts
{
    public static IEnumerable<SyntaxKind> GetBinaryOperatorsKinds()
    {
        var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var kind in kinds)
        {
            if (kind.GetBinaryOperatorPriority() > 0)
            {
                yield return kind;
            }
        }
    }

    public static IEnumerable<SyntaxKind> GetUnaryOperatorsKinds()
    {
        var kinds = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var kind in kinds)
        {
            if (kind.GetUnaryOperatorPriority() > 0)
            {
                yield return kind;
            }
        }
    }
}