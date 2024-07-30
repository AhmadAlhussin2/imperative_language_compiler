using ImperativeCompiler.CodeAnalysis.Syntax;

namespace ImperativeCompiler.Tests.Syntax;

public class ParserTests
{
    [Theory]
    [MemberData(nameof(GetBinaryOperatorsData))]
    public void Parse_binary_expression_correctly_resolve_operators_priority(SyntaxKind op1, SyntaxKind op2)
    {
        var op1Priority = op1.GetBinaryOperatorPriority();
        var op2Priority = op2.GetBinaryOperatorPriority();
        var op1Text = op1.GetText();
        var op2Text = op2.GetText();
        var text = $"a {op1Text} b {op2Text} c";
        var expression = ParseExpression(text);

        if (op1Priority >= op2Priority)
        {
            // example: a + b + c / a * b + c

            using var e = new AssertingEnumerator(expression);

            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.Variable);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(op1, op1Text!);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.Variable);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
            e.AssertToken(op2, op2Text!);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.Variable);
            e.AssertToken(SyntaxKind.IdentifierToken, "c");
        }
        else
        {
            using var e = new AssertingEnumerator(expression);

            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.Variable);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(op1, op1Text!);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.Variable);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
            e.AssertToken(op2, op2Text!);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.Variable);
            e.AssertToken(SyntaxKind.IdentifierToken, "c");
        }
    }

    [Theory]
    [MemberData(nameof(GetUnaryOperatorsData))]
    public void Parse_unary_expression_correctly_resolve_operators_priority(SyntaxKind unaryKind, SyntaxKind binaryKind)
    {
        var unaryPriority = unaryKind.GetUnaryOperatorPriority();
        var binaryPriority = binaryKind.GetBinaryOperatorPriority();
        var unaryText = unaryKind.GetText();
        var binaryText = binaryKind.GetText();
        var text = $"{unaryText} a {binaryText} b";
        var expression = ParseExpression(text);

        if (unaryPriority >= binaryPriority)
        {
            using var e = new AssertingEnumerator(expression);

            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.UnaryExpression);
            e.AssertToken(unaryKind, unaryText!);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.Variable);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(binaryKind, binaryText!);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.Variable);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");
        }
        else
        {
            using var e = new AssertingEnumerator(expression);

            e.AssertNode(SyntaxKind.UnaryExpression);
            e.AssertToken(unaryKind, unaryText!);
            e.AssertNode(SyntaxKind.BinaryExpression);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.Variable);
            e.AssertToken(SyntaxKind.IdentifierToken, "a");
            e.AssertToken(binaryKind, binaryText!);
            e.AssertNode(SyntaxKind.NameExpression);
            e.AssertNode(SyntaxKind.Variable);
            e.AssertToken(SyntaxKind.IdentifierToken, "b");

        }
    }

    public static IEnumerable<object[]> GetBinaryOperatorsData()
    {
        foreach (var op1 in SyntaxFacts.GetBinaryOperatorsKinds())
        {
            foreach (var op2 in SyntaxFacts.GetBinaryOperatorsKinds())
            {
                yield return [op1, op2];
            }
        }
    }

    public static IEnumerable<object[]> GetUnaryOperatorsData()
    {
        foreach (var op1 in SyntaxFacts.GetUnaryOperatorsKinds())
        {
            foreach (var op2 in SyntaxFacts.GetBinaryOperatorsKinds())
            {
                yield return [op1, op2];
            }
        }
    }

    private static ExpressionSyntax ParseExpression(string text)
    {
        var syntaxTree = SyntaxTree.Parse(text);
        var root = syntaxTree.Root;
        var member = Assert.Single(root.Members);
        var globalStatement = Assert.IsType<GlobalStatementSyntax>(member);
        return Assert.IsType<ExpressionStatementSyntax>(globalStatement.Statement).Expression;
    }
}