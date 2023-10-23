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
                return TextSpan.FromBound(first.Start, last.Length);
            }
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
    }
}