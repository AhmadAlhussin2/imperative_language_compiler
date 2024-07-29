using FluentAssertions;
using ImperativeCompiler.CodeAnalysis.Syntax;

namespace ImperativeCompiler.Tests;

public class LexerTests
{
    [Theory]
    [MemberData(nameof(GetTokensData))]
    public void Lexed_lex_identifier_token(SyntaxKind kind, string text)
    {
        var tokens = SyntaxTree.ParseTokens(text);

        tokens.Should().ContainSingle();
        tokens.Single().Kind.Should().Be(kind);
        tokens.Single().Text.Should().Be(text);
    }

    public static IEnumerable<object[]> GetTokensData()
    {
        foreach (var token in GetTokens())
        {
            yield return [token.kind, token.text];
        }
    }

    private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
    {
        return new[]
        {
            (SyntaxKind.IdentifierToken, "a"),
            (SyntaxKind.IdentifierToken, "abc")
        };
    }
}