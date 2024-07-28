using System.Collections.Immutable;
using ImperativeCompiler.CodeAnalysis.Text;
namespace ImperativeCompiler.CodeAnalysis.Syntax;

public sealed class SyntaxTree
{
    private delegate void ParseHandler(SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics);
    private SyntaxTree(SourceText text, ParseHandler handler)
    {
        Text = text;
        handler(this, out var root, out var diagnostics);
        Diagnostics = diagnostics;
        Root = root;
    }

    public SourceText Text { get; }
    public CompilationUnitSyntax Root { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }
    private static void Parse(SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> diagnostics)
    {
        var parser = new Parser(syntaxTree);
        root = parser.ParseCompilationUnit();
        diagnostics = parser.Diagnostics.ToImmutableArray();
    }
    public static SyntaxTree Parse(string text)
    {
        var sourceText = SourceText.From(text);
        return Parse(sourceText);
    }
    public static SyntaxTree Parse(SourceText text)
    {
        return new SyntaxTree(text, Parse);
    }
    public static ImmutableArray<SyntaxToken> ParseTokens(string text)
    {
        var sourceText = SourceText.From(text);
        return ParseTokens(sourceText);
    }
    public static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics)
    {
        var sourceText = SourceText.From(text);
        return ParseTokens(sourceText, out diagnostics);
    }
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text)
    {
        return ParseTokens(text, out _);
    }
    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text, out ImmutableArray<Diagnostic> diagnostics)
    {
        var tokens = new List<SyntaxToken>();
        void ParseTokens(SyntaxTree syntaxTree, out CompilationUnitSyntax root, out ImmutableArray<Diagnostic> d)
        {
            var l = new Lexer(syntaxTree);
            while (true)
            {
                var token = l.NextToken();
                if (token.Kind == SyntaxKind.EofToken)
                {
                    root = new CompilationUnitSyntax(syntaxTree, ImmutableArray<MemberSyntax>.Empty, token);
                    break;
                }
                tokens.Add(token);
            }
            d = [..l.Diagnostics];
        }
        var syntaxTree = new SyntaxTree(text, ParseTokens);
        diagnostics = [..syntaxTree.Diagnostics];
        return [..tokens];
    }
}