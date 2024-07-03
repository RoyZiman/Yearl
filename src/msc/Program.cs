using Yearl.CodeAnalysis;
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

            var paths = GetFilePaths(args);
            List<SyntaxTree> syntaxTrees = [];
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

            Compilation compilation = new(syntaxTrees.ToArray());
            var result = compilation.Evaluate([]);

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
            SortedSet<string> result = [];

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
