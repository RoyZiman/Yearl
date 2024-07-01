using Yearl.CodeAnalysis;
using Yearl.CodeAnalysis.Syntax;
using Yearl.IO;

namespace msc
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: mc <source-paths>");
                return;
            }

            if (args.Length > 1)
            {
                Console.WriteLine("error: only one path supported right now");
                return;
            }

            string path = args.Single();

            string text = File.ReadAllText(path);
            SyntaxTree syntaxTree = SyntaxTree.Parse(text);

            Compilation compilation = new(syntaxTree);
            EvaluationResult result = compilation.Evaluate([]);

            if (!result.Errors.Any())
            {
                if (result.Value != null)
                    Console.WriteLine(result.Value);
            }
            else
            {
                Console.Error.WriteErrors(result.Errors, syntaxTree);
            }
        }
    }
}
