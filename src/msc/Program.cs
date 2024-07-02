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
                Console.Error.WriteLine("usage: msc <source-paths>");
                return;
            }

            if (args.Length > 1)
            {
                Console.WriteLine("error: only one path supported right now");
                return;
            }

            string path = args.Single();

            if (!File.Exists(path))
            {
                Console.WriteLine($"error: file '{path}' doesn't exist");
                return;
            }

            var syntaxTree = SyntaxTree.Load(path);

            Compilation compilation = new(syntaxTree);
            EvaluationResult result = compilation.Evaluate([]);

            if (!result.Errors.Any())
            {
                if (result.Value != null)
                    Console.WriteLine(result.Value);
            }
            else
            {
                Console.Error.WriteErrors(result.Errors);
            }
        }
    }
}
