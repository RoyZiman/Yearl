using Mono.Options;
using Yearl.CodeAnalysis;
using Yearl.CodeAnalysis.Syntax;
using Yearl.IO;

namespace msc
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            string? outputPath = null;
            string? moduleName = null;
            var referencePaths = new List<string>();
            var sourcePaths = new List<string>();
            bool helpRequested = false;

            var options = new OptionSet
            {
                "usage: msc <source-paths> [options]",
                { "r=", "The {path} of an assembly to reference", v => referencePaths.Add(v) },
                { "o=", "The output {path} of the assembly to create", v => outputPath = v },
                { "m=", "The {name} of the module", v => moduleName = v },
                { "?|h|help", "Prints help", v => helpRequested = true },
                { "<>", v => sourcePaths.Add(v) }
            };

            options.Parse(args);

            if (helpRequested)
            {
                options.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            if (sourcePaths.Count == 0)
            {
                Console.Error.WriteLine("error: need at least one source file");
                return 1;
            }
            outputPath ??= Path.ChangeExtension(sourcePaths[0], ".exe");

            moduleName ??= Path.GetFileNameWithoutExtension(outputPath);

            var syntaxTrees = new List<SyntaxTree>();
            bool hasErrors = false;

            foreach (string path in sourcePaths)
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

            foreach (string path in referencePaths)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"error: file '{path}' doesn't exist");
                    hasErrors = true;
                    continue;
                }
            }

            if (hasErrors)
                return 1;

            var compilation = Compilation.Create([.. syntaxTrees]);
            var errors = compilation.Emit(moduleName, [.. referencePaths], outputPath);

            if (errors.Any())
            {
                Console.Error.WriteErrors(errors);
                return 1;
            }

            return 0;
        }
    }
}
