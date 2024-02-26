using System.Reflection;

namespace Yearl.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }

        public IEnumerable<BoundNode> GetChildren()
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
                {
                    BoundNode child = (BoundNode)property.GetValue(this);
                    if (child != null)
                        yield return child;
                }
                else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                {
                    IEnumerable<BoundNode> children = (IEnumerable<BoundNode>)property.GetValue(this);
                    foreach (BoundNode child in children)
                    {
                        if (child != null)
                            yield return child;
                    }
                }
            }
        }

        private IEnumerable<(string Name, object Value)> GetProperties()
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (property.Name is (nameof(Kind)) or
                    (nameof(BoundBinaryExpression.Operator)))
                    continue;

                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType) ||
                    typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                    continue;

                object? value = property.GetValue(this);
                if (value != null)
                    yield return (property.Name, value);
            }
        }

        public void WriteTo(TextWriter writer)
        {
            PrettyPrint(writer, this);
        }

        private static void PrettyPrint(TextWriter writer, BoundNode node, string indent = "", bool isLast = true)
        {
            bool isToConsole = writer == Console.Out;
            string marker = isLast ? "└──" : "├──";

            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            writer.Write(indent);
            writer.Write(marker);

            if (isToConsole)
                Console.ForegroundColor = GetColor(node);

            string text = GetText(node);
            writer.Write(text);

            bool isFirstProperty = true;

            foreach ((string Name, object Value) p in node.GetProperties())
            {
                if (isFirstProperty)
                    isFirstProperty = false;
                else
                {
                    if (isToConsole)
                        Console.ForegroundColor = ConsoleColor.DarkGray;

                    writer.Write(",");
                }

                writer.Write(" ");

                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                writer.Write(p.Name);

                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                writer.Write(": ");

                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;

                writer.Write(p.Value);
            }

            if (isToConsole)
                Console.ResetColor();

            writer.WriteLine();

            indent += isLast ? "   " : "│  ";

            BoundNode? lastChild = node.GetChildren().LastOrDefault();

            foreach (BoundNode child in node.GetChildren())
                PrettyPrint(writer, child, indent, child == lastChild);
        }

        private static string GetText(BoundNode node)
        {
            if (node is BoundBinaryExpression b)
                return b.Operator.Kind.ToString() + "Expression";

            if (node is BoundUnaryExpression u)
                return u.Operator.Kind.ToString() + "Expression";

            return node.Kind.ToString();
        }

        private static ConsoleColor GetColor(BoundNode node)
        {
            if (node is BoundExpression)
                return ConsoleColor.Blue;

            if (node is BoundStatement)
                return ConsoleColor.Cyan;

            return ConsoleColor.Yellow;
        }

        public override string ToString()
        {
            using (StringWriter writer = new())
            {
                WriteTo(writer);
                return writer.ToString();
            }
        }
    }
}