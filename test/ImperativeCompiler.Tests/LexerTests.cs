using FluentAssertions;
using ImperativeCompiler.CodeAnalysis.Syntax;

namespace ImperativeCompiler.Tests;

public class LexerTests
{
    private static readonly List<string> Separators =
    [
        " ",
        "\n\n",
        "\t\t\t",
        " \t\n"
    ];

    [Theory]
    [MemberData(nameof(GetTokensData))]
    public void Lexed_lex_single_token(SyntaxKind kind, string text)
    {
        // Act
        var tokens = SyntaxTree.ParseTokens(text);

        // Assert
        tokens.Should().ContainSingle();
        tokens.Single().Kind.Should().Be(kind);
        tokens.Single().Text.Should().Be(text);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairsData))]
    public void Lexed_lex_multiple_tokens(SyntaxKind kind1, string text1, SyntaxKind kind2, string text2)
    {
        // Act
        var text = text1 + text2;
        var tokens = SyntaxTree.ParseTokens(text).ToArray();

        // Assert
        tokens.Should().HaveCount(2);
        tokens[0].Kind.Should().Be(kind1);
        tokens[0].Text.Should().Be(text1);
        tokens[1].Kind.Should().Be(kind2);
        tokens[1].Text.Should().Be(text2);
    }

    [Theory]
    [MemberData(nameof(GetSpecialPairsData))]
    public void Lexed_lex_multiple_tokens_with_separator(SyntaxKind kind1, string text1, SyntaxKind separator,
        string separatorText, SyntaxKind kind2, string text2)
    {
        // Act
        var text = text1 + separatorText + text2;
        var tokens = SyntaxTree.ParseTokens(text).ToArray();

        // Assert
        tokens.Should().HaveCount(3);
        tokens[0].Kind.Should().Be(kind1);
        tokens[0].Text.Should().Be(text1);
        tokens[1].Kind.Should().Be(separator);
        tokens[1].Text.Should().Be(separatorText);
        tokens[2].Kind.Should().Be(kind2);
        tokens[2].Text.Should().Be(text2);
    }

    public static IEnumerable<object[]> GetTokensData()
    {
        foreach (var token in GetTokens())
        {
            yield return [token.kind, token.text];
        }
    }

    public static IEnumerable<object[]> GetTokenPairsData()
    {
        foreach (var token in GetTokenPairs())
        {
            yield return [token.kind1, token.text1, token.kind2, token.text2];
        }
    }

    public static IEnumerable<object[]> GetSpecialPairsData()
    {
        foreach (var token in GetSpecialTokenPairs())
        {
            foreach (var separator in Separators)
            {
                yield return [token.kind1, token.text1, SyntaxKind.WhiteSpace, separator, token.kind2, token.text2];
            }
        }
    }

    private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
    {
        return new[]
        {
            (SyntaxKind.IdentifierToken, "a"),
            (SyntaxKind.IdentifierToken, "abc"),
            (SyntaxKind.WhiteSpace, "    "),
            (SyntaxKind.WhiteSpace, "\n\n\n"),
            (SyntaxKind.WhiteSpace, "\n\n\n\t"),
            (SyntaxKind.NumberToken, "123"),
            (SyntaxKind.PlusToken, "+"),
            (SyntaxKind.MinusToken, "-"),
            (SyntaxKind.RealNumberToken, "12.3123"),
            (SyntaxKind.StarToken, "*"),
            (SyntaxKind.SlashToken, "/"),
            (SyntaxKind.OpenParenthesisToken, "("),
            (SyntaxKind.CloseParenthesisToken, ")"),
            (SyntaxKind.LessThanOrEqualToken, "<="),
            (SyntaxKind.LessThanToken, "<"),
            (SyntaxKind.GreaterThanOrEqualToken, ">="),
            (SyntaxKind.GreaterThanToken, ">"),
            (SyntaxKind.NotEqualToken, "/="),
            (SyntaxKind.NotKeyword, "not"),
            (SyntaxKind.ModuloToken, "%"),
            (SyntaxKind.AssignmentToken, ":="),
            (SyntaxKind.ColonToken, ":"),
            (SyntaxKind.EqualToken, "="),
            (SyntaxKind.AndKeyword, "and"),
            (SyntaxKind.OrKeyword, "or"),
            (SyntaxKind.XorKeyword, "xor"),
            (SyntaxKind.IfKeyword, "if"),
            (SyntaxKind.IsKeyword, "is"),
            (SyntaxKind.IntegerKeyword, "integer"),
            (SyntaxKind.RealKeyword, "real"),
            (SyntaxKind.TypeKeyword, "type"),
            (SyntaxKind.BooleanKeyword, "boolean"),
            (SyntaxKind.TrueKeyword, "true"),
            (SyntaxKind.FalseKeyword, "false"),
            (SyntaxKind.RecordKeyword, "record"),
            (SyntaxKind.EndKeyword, "end"),
            (SyntaxKind.ArrayKeyword, "array"),
            (SyntaxKind.WhileKeyword, "while"),
            (SyntaxKind.LoopKeyword, "loop"),
            (SyntaxKind.ForKeyword, "for"),
            (SyntaxKind.ReverseKeyword, "reverse"),
            (SyntaxKind.DotToken, "."),
            (SyntaxKind.CommaToken, ","),
            (SyntaxKind.RangeToken, ".."),
            (SyntaxKind.ThenKeyword, "then"),
            (SyntaxKind.ElseKeyword, "else"),
            (SyntaxKind.RoutineKeyword, "routine"),
            (SyntaxKind.ReturnKeyword, "return"),
            (SyntaxKind.OpenSquareBracketToken, "["),
            (SyntaxKind.CloseSquareBracketToken, "]")
        };
    }

    private static bool RequireSeparator(SyntaxKind kind1, SyntaxKind kind2)
    {
        var isKind1Keyword = kind1.ToString().EndsWith("Keyword");
        var isKind2Keyword = kind2.ToString().EndsWith("Keyword");

        if (kind1 is SyntaxKind.IdentifierToken &&
            kind2 is (SyntaxKind.RealNumberToken or SyntaxKind.IdentifierToken or SyntaxKind.NumberToken))
        {
            return true;
        }

        if (isKind1Keyword && isKind2Keyword)
        {
            return true;
        }

        if (isKind1Keyword && kind2 is SyntaxKind.IdentifierToken)
        {
            return true;
        }

        if (isKind2Keyword && kind1 is SyntaxKind.IdentifierToken)
        {
            return true;
        }

        if (isKind1Keyword && kind2 is (SyntaxKind.NumberToken or SyntaxKind.RealNumberToken))
        {
            return true;
        }

        if (kind1 is (SyntaxKind.NumberToken or SyntaxKind.RealNumberToken) &&
            kind2 is (SyntaxKind.NumberToken or SyntaxKind.RealNumberToken))
        {
            return true;
        }

        if (kind1 is (SyntaxKind.DotToken or SyntaxKind.RangeToken) &&
            kind2 is (SyntaxKind.DotToken or SyntaxKind.RangeToken))
        {
            return true;
        }

        if (kind1 is SyntaxKind.ColonToken && kind2 is SyntaxKind.EqualToken)
        {
            return true;
        }

        if (kind1 is (SyntaxKind.SlashToken or SyntaxKind.GreaterThanToken or SyntaxKind.LessThanToken) &&
            kind2 is SyntaxKind.EqualToken)
        {
            return true;
        }

        if (kind1 is (SyntaxKind.NumberToken or SyntaxKind.RealNumberToken) && kind2 is SyntaxKind.DotToken)
        {
            return true;
        }

        if (kind1 is SyntaxKind.WhiteSpace && kind2 is SyntaxKind.WhiteSpace)
        {
            return true;
        }

        return false;
    }

    private static IEnumerable<(SyntaxKind kind1, string text1, SyntaxKind kind2, string text2)> GetTokenPairs()
    {
        foreach (var token1 in GetTokens())
        {
            foreach (var token2 in GetTokens())
            {
                if (RequireSeparator(token1.kind, token2.kind))
                {
                    continue;
                }

                yield return (token1.kind, token1.text, token2.kind, token2.text);
            }
        }
    }

    private static IEnumerable<(SyntaxKind kind1, string text1, SyntaxKind kind2, string text2)> GetSpecialTokenPairs()
    {
        foreach (var token1 in GetTokens())
        {
            foreach (var token2 in GetTokens())
            {
                if (!RequireSeparator(token1.kind, token2.kind) || token1.kind is SyntaxKind.WhiteSpace ||
                    token2.kind is SyntaxKind.WhiteSpace)
                {
                    continue;
                }

                yield return (token1.kind, token1.text, token2.kind, token2.text);
            }
        }
    }
}