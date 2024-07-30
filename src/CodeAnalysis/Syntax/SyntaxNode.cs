using System.Reflection;
using ImperativeCompiler.CodeAnalysis.Text;
namespace ImperativeCompiler.CodeAnalysis.Syntax;

public abstract class SyntaxNode
{
    protected SyntaxNode(SyntaxTree syntaxTree)
    {
        SyntaxTree = syntaxTree;
    }
    public SyntaxTree SyntaxTree { get; }
    public abstract SyntaxKind Kind { get; }
    public virtual TextSpan Span
    {
        get
        {
            var first = GetChildren().First().Span;
            var last = GetChildren().Last().Span;
            return TextSpan.FromBounds(first.Start, last.End);
        }
    }
    public SyntaxToken GetLastToken()
    {
        if (this is SyntaxToken token)
            return token;
        return GetChildren().Last().GetLastToken();
    }
    public IEnumerable<SyntaxNode> GetChildren()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
            {
                var child = property.GetValue(this);
                if (child != null)
                    yield return (SyntaxNode)child;
            }
            else if (typeof(SeparatedSyntaxList).IsAssignableFrom(property.PropertyType))
            {
                var seperatedSyntaxList = property.GetValue(this);
                if (seperatedSyntaxList != null)
                    foreach (var child in ((SeparatedSyntaxList)seperatedSyntaxList).GetWithSeparators())
                        yield return child;

            }
            else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
            {
                var children = property.GetValue(this);
                if (children != null)
                    foreach (var child in (IEnumerable<SyntaxNode>)children)
                    {
                        yield return child;
                    }
            }
        }
    }
    public void WriteTo(TextWriter writer)
    {
        PrintNode(writer, this);
    }
    private static void PrintNode(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = false)
    {
        // bool isToConsole = writer == Console.Out;
        var marker = isLast ? "└──" : "├──";
        writer.Write(indent);
        writer.Write(marker);
        writer.Write(node.Kind);
        if (node is SyntaxToken t && t.Value != null)
        {
            writer.Write($" {t.Value}");
        }
        writer.WriteLine();
        indent += isLast ? "   " : "│  ";
        var lastChild = node.GetChildren().LastOrDefault();
        foreach (var child in node.GetChildren())
        {
            PrintNode(writer, child, indent, child == lastChild);
        }
    }
    public override string ToString()
    {
        using var writer = new StringWriter();
        WriteTo(writer);
        return writer.ToString();
    }
}