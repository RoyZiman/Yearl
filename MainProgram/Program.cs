using Yearl.Language;
using Yearl.Language.Syntax;

namespace Yearl
{
    public class Program
    {
        public static void Main()
        {

            bool showTree = true;
            Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();


            while (true)
            {
                Console.Write("> ");
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;

                if (input == "quit") break;

                if (input == "cls")
                {
                    Console.Clear();
                    continue;
                }

                if (input == "#tree")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing parse trees." : "Hiding parse trees.");
                    continue;
                }

                SyntaxTree syntaxTree = SyntaxTree.Parse(input);
                Compilation compilation = new Compilation(syntaxTree);
                EvaluationResult result = compilation.Evaluate(variables);

                if (!result.Errors.Any())
                {
                    if (showTree)
                        Console.WriteLine(syntaxTree);
                    Console.WriteLine(result.Value);
                }
                else foreach (Error Error in result.Errors)
                        Console.WriteLine(Error);

            }
        }
    }
}
