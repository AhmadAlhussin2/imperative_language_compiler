using compilers.CodeAnalysis.Text;
namespace compilers.CodeAnalysis.Syntax;

public class SyntaxToken : SyntaxNode
{
    public SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, int position, string? text, object? value) : base(syntaxTree)
    {
        Kind = kind;
        Position = position;
        IsMissing = string.IsNullOrEmpty(text);
        Text = text ?? string.Empty;
        Value = value;
    }

    public override SyntaxKind Kind { get; }
    public int Position { get; }
    public bool IsMissing { get; }
    public string Text { get; }
    public object? Value { get; }
    public override TextSpan Span => new TextSpan(Position, Text.Length);
}