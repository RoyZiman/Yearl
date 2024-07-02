using Yearl.CodeAnalysis;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;
using Yearl.IO;

namespace msc
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("usage: msc <source-paths>");
                return 1;
            }

            IEnumerable<string> paths = GetFilePaths(args);
            var syntaxTrees = new List<SyntaxTree>();
            bool hasErrors = false;

            foreach (string path in paths)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"error: file '{path}' doesn't exist");
                    hasErrors = true;
                    continue;
                }
                var syntaxTree = SyntaxTree.Load(path);
                syntaxTrees.Add(syntaxTree);
            }

            if (hasErrors)
                return 1;

            var compilation = new Compilation(syntaxTrees.ToArray());
            EvaluationResult result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            if (!result.Errors.Any())
            {
                if (result.Value != null)
                    Console.WriteLine(result.Value);
            }
            else
            {
                Console.Error.WriteErrors(result.Errors);
                return 1;
            }

            return 0;
        }

        private static IEnumerable<string> GetFilePaths(IEnumerable<string> paths)
        {
            var result = new SortedSet<string>();

            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    result.UnionWith(Directory.EnumerateFiles(path, "*.yearl", SearchOption.AllDirectories));
                }
                else
                {
                    result.Add(path);
                }
            }

            return result;
        }
    }
}
