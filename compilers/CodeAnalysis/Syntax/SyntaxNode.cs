using System.Reflection;

namespace compilers.CodeAnalysis
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }
        public virtual TextSpan Span
        {
            get
            {
                var first = getChildren().First().Span;
                var last = getChildren().Last().Span;
                return TextSpan.FromBounds(first.Start, last.Length);
            }
        }
        public SyntaxToken GetLastToken(){
            if (this is SyntaxToken token)
                return token;
            return getChildren().Last().GetLastToken();
        } 
        public IEnumerable<SyntaxNode> getChildren()
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
                else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                {
                    var children = property.GetValue(this);
                    if (children != null)
                        foreach (var child in (IEnumerable<SyntaxNode>)children)
                        {
                            if (child != null)
                                yield return child;
                        }
                }
            }
        }
        public void WriteTo(TextWriter writer)
        {
            printNode(writer, this);
        }
        private static void printNode(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = false)
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
            var lastChild = node.getChildren().LastOrDefault();
            foreach (var child in node.getChildren())
            {
                printNode(writer, child, indent, child == lastChild);
            }
        }
        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                WriteTo(writer);
                return writer.ToString();
            }
        }
    }
}