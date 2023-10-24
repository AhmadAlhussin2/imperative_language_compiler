using System.Reflection;
using System.Runtime.CompilerServices;

namespace compilers.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
        public IEnumerable<BoundNode> getChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
                {
                    var child = property.GetValue(this);
                    if (child != null)
                        yield return (BoundNode)child;
                }
                else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                {
                    var children = property.GetValue(this);

                    if (children != null)
                        foreach (var child in (IEnumerable<BoundNode>)children)
                        {
                            if (child != null)
                                yield return child;
                        }
                }
            }
        }
        private IEnumerable<(string Name, object Value)> GetProperties()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.Name == nameof(Kind) || property.Name == nameof(BoundBinaryExpression.Op))
                    continue;

                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType)
                    || typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                    continue;
                var value = property.GetValue(this);
                if (value != null)
                {
                    yield return (property.Name, value);
                }
            }
        }
        private static void printNode(TextWriter writer, BoundNode node, string indent = "", bool isLast = false)
        {
            bool toConsole = writer == Console.Out;
            var marker = isLast ? "└──" : "├──";
            writer.Write(indent);
            writer.Write(marker);
            if (toConsole)
                Console.ForegroundColor = GetColor(node);
            var text = GetText(node);
            writer.Write(text);
            if (toConsole)
                Console.ResetColor();

            var isFirstProperty = true;
            foreach (var p in node.GetProperties())
            {
                if (isFirstProperty)
                    isFirstProperty = false;
                else{
                    if (toConsole)
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    writer.Write(",");
                }
                writer.Write(" ");
                if (toConsole)
                        Console.ForegroundColor = ConsoleColor.Green;
                writer.Write(p.Name);
                if (toConsole)
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                writer.Write(" = ");
                if (toConsole)
                        Console.ForegroundColor = ConsoleColor.Magenta;
                writer.Write(p.Value);
            }
            if (toConsole)
                Console.ResetColor();

            writer.WriteLine();
            indent += isLast ? "   " : "│  ";
            var lastChild = node.getChildren().LastOrDefault();
            foreach (var child in node.getChildren())
            {
                printNode(writer, child, indent, child == lastChild);
            }
        }


        private static ConsoleColor GetColor(BoundNode node)
        {
            if (node is BoundExpression)
                return ConsoleColor.Blue;
            if (node is BoundStatement)
                return ConsoleColor.Cyan;
            return ConsoleColor.Yellow;
        }

        private static string GetText(BoundNode node)
        {
            if (node is BoundBinaryExpression b)
                return b.Op.Kind.ToString() + "Expression";
            if (node is BoundUnaryExpression u)
                return u.Op.Kind.ToString() + " Expression";
            return node.Kind.ToString();
        }

        public void WriteTo(TextWriter writer)
        {
            printNode(writer, this);
        }
    }
}