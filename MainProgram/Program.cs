using System.Text;
using Yearl.CodeAnalysis;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;
using Yearl.CodeAnalysis.Text;

namespace MainProgram
{
    public class Program
    {
        public static void Main()
        {
            bool showTree = false;
            bool showProgram = true;
            Dictionary<VariableSymbol, object> variables = [];
            StringBuilder textBuilder = new();
            Compilation? previous = null;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;

                if (textBuilder.Length == 0)
                    Console.Write("» ");
                else
                    Console.Write("· ");

                Console.ResetColor();

                string? input = Console.ReadLine();
                bool isBlank = string.IsNullOrWhiteSpace(input);

                if (textBuilder.Length == 0)
                {
                    if (isBlank) continue;
                    else if (input == "#quit") break;
                    else if (input == "#parseTree")
                    {
                        showTree = !showTree;
                        Console.WriteLine(showTree ? "Showing parse trees." : "Not showing parse trees.");
                        continue;
                    }
                    else if (input == "#boundTree")
                    {
                        showProgram = !showProgram;
                        Console.WriteLine(showTree ? "Showing bound tree." : "Not showing bound tree.");
                        continue;
                    }
                    else if (input == "#reset")
                    {
                        previous = null;
                        variables.Clear();
                        continue;
                    }
                    else if (input == "#cls")
                    {
                        Console.Clear();
                        continue;
                    }
                }

                textBuilder.AppendLine(input);
                string text = textBuilder.ToString();

                SyntaxTree syntaxTree = SyntaxTree.Parse(text);

                if (!isBlank && syntaxTree.Errors.Any())
                    continue;

                Compilation compilation = previous == null
                                   ? new Compilation(syntaxTree)
                                   : previous.ContinueWith(syntaxTree);

                if (showTree)
                {
                    Console.WriteLine();
                    syntaxTree.Root.WriteTo(Console.Out);
                }
                if (showProgram)
                {
                    Console.WriteLine();
                    compilation.EmitTree(Console.Out);
                }

                EvaluationResult result = compilation.Evaluate(variables);

                if (!result.Errors.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                    previous = compilation;
                }
                else foreach (Error diagnostic in result.Errors)
                    {
                        int lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                        TextLine line = syntaxTree.Text.Lines[lineIndex];
                        int lineNumber = lineIndex + 1;
                        int character = diagnostic.Span.Start - line.Start + 1;

                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write($"({lineNumber}, {character}): ");
                        Console.WriteLine(diagnostic);
                        Console.ResetColor();

                        TextSpan prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                        TextSpan suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                        string prefix = syntaxTree.Text.ToString(prefixSpan);
                        string error = syntaxTree.Text.ToString(diagnostic.Span);
                        string suffix = syntaxTree.Text.ToString(suffixSpan);

                        Console.Write("    ");
                        Console.Write(prefix);

                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(error);
                        Console.ResetColor();

                        Console.Write(suffix);

                        Console.WriteLine();
                    }

                textBuilder.Clear();
            }
        }
    }
}