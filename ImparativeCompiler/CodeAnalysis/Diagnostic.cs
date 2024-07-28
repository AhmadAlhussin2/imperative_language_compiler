using compilers.CodeAnalysis.Text;
namespace compilers.CodeAnalysis;

public sealed class Diagnostic
{
    public Diagnostic(TextSpan span, string message)
    {
        Span = span;
        Message = message;
    }
    public TextSpan Span { get; }
    private string Message { get; }

    public override string ToString() => Message;

}