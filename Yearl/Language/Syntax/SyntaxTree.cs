using Yearl.Language.Syntax;

namespace Yearl.Language.Syntax
{
    public sealed class SyntaxTree(IEnumerable<Error> errors, SyntaxNode root, SyntaxToken endOfFileToken) : SyntaxNode
    {
        public IReadOnlyList<Error> Errors { get; } = errors.ToArray();
        public SyntaxNode Root { get; } = root;
        public SyntaxToken EndOfFileToken { get; } = endOfFileToken;

        public override SyntaxKind Kind => SyntaxKind.TreeStatement;

        public static SyntaxTree Parse(string path, string code)
        {
            Parser parser = new Parser(path, code);
            return parser.Parse();
        }


        public override string ToString()
        {
            return "\n\nSyntaxTree\n" + PrettyPrint(Root);
        }
        static string PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            string output = "";

            output += indent + (isLast ? "└──" : "├──") + node.Kind;

            if (node is SyntaxToken t && t.Value != null)
                output += " " + t.Value;


            output += "\n";

            indent += isLast ? "    " : "│   ";

            SyntaxNode? lastChild = node.GetChildren().LastOrDefault();

            foreach (SyntaxNode child in node.GetChildren())
                output += PrettyPrint(child, indent, child == lastChild);

            return output;
        }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            throw new Exception("Inaccessible due to invalid accessibility call");
        }
    }
}
