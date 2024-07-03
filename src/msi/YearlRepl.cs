using Yearl.CodeAnalysis;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;
using Yearl.IO;

namespace msi;

internal class YearlRepl : Repl
{
    private bool _loadingSubmission;
    private static readonly Compilation _emptyCompilation = Compilation.CreateScript(null);
    private Compilation? _previous;
    private bool _showTree;
    private bool _showProgram;
    private readonly Dictionary<VariableSymbol, object> _variables = [];

    public YearlRepl()
    {
        LoadSubmissions();
    }

    protected override void RenderLine(string line)
    {
        var tokens = SyntaxTree.ParseTokens(line);
        foreach (var token in tokens)
        {
            bool isKeyword = token.Kind.ToString().EndsWith("Keyword");
            bool isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
            bool isNumber = token.Kind == SyntaxKind.NumberToken;
            bool isString = token.Kind == SyntaxKind.StringToken;

            Console.ForegroundColor = isKeyword
                ? ConsoleColor.Blue
                : isIdentifier
                ? ConsoleColor.DarkYellow
                : isNumber ? ConsoleColor.Cyan : isString ? ConsoleColor.Magenta : ConsoleColor.DarkGray;

            Console.Write(token.Text);

            Console.ResetColor();
        }
    }

    [MetaCommand("cls", "Clears the screen")]
    private void EvaluateCls() => Console.Clear();

    [MetaCommand("reset", "Clears all previous submissions")]
    private void EvaluateReset()
    {
        _previous = null;
        _variables.Clear();
        ClearSubmissions();
    }

    [MetaCommand("showTree", "Shows the parse tree")]
    private void EvaluateShowTree()
    {
        _showTree = !_showTree;
        Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
    }

    [MetaCommand("showProgram", "Shows the bound tree")]
    private void EvaluateShowProgram()
    {
        _showProgram = !_showProgram;
        Console.WriteLine(_showProgram ? "Showing bound tree." : "Not showing bound tree.");
    }

    [MetaCommand("load", "Loads a script file")]
    private void EvaluateLoad(string path)
    {
        path = Path.GetFullPath(path);

        if (!File.Exists(path))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"error: file does not exist '{path}'");
            Console.ResetColor();
            return;
        }

        Console.WriteLine($"Successfully '{path}'");

        string text = File.ReadAllText(path);
        EvaluateSubmission(text);
    }

    [MetaCommand("ls", "Lists all symbols")]
    private void EvaluateLs()
    {
        var compilation = _previous ?? _emptyCompilation;
        var symbols = compilation.GetSymbols().OrderBy(s => s.Kind).ThenBy(s => s.Name);

        foreach (var symbol in symbols)
        {
            symbol.WriteTo(Console.Out);
            Console.WriteLine();
        }
    }

    [MetaCommand("dump", "Shows bound tree of a given function")]
    private void EvaluateDump(string functionName)
    {
        var compilation = _previous ?? _emptyCompilation;
        var symbol = compilation.GetSymbols().OfType<FunctionSymbol>().SingleOrDefault(f => f.Name == functionName);
        if (symbol == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"error: function '{functionName}' does not exist");
            Console.ResetColor();
            return;
        }

        compilation.EmitTree(symbol, Console.Out);
    }

    protected override bool IsCompleteSubmission(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        var syntaxTree = SyntaxTree.Parse(text);

        // Use Members because we need to exclude the EndOfFileToken.
        return !syntaxTree.Root.Members.Last().GetLastToken().IsMissing;
    }

    private static SyntaxToken GetLastToken(SyntaxNode node)
    {
        if (node is SyntaxToken token)
            return token;

        // A syntax node should always contain at least 1 token.
        return GetLastToken(node.GetChildren().Last());
    }

    protected override void EvaluateSubmission(string text)
    {
        var syntaxTree = SyntaxTree.Parse(text);

        var compilation = Compilation.CreateScript(_previous, syntaxTree);

        if (_showTree)
            syntaxTree.Root.WriteTo(Console.Out);

        if (_showProgram)
            compilation.EmitTree(Console.Out);

        var result = compilation.Evaluate(_variables);

        if (!result.Errors.Any())
        {
            if (result.Value != null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(result.Value);
                Console.ResetColor();
            }
            _previous = compilation;

            SaveSubmission(text);
        }
        else
        {
            Console.Out.WriteErrors(result.Errors);
        }
    }

    private static string GetSubmissionsDirectory()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string submissionsDirectory = Path.Combine(localAppData, "Yearl", "Submissions");
        return submissionsDirectory;
    }

    private void LoadSubmissions()
    {
        string submissionsDirectory = GetSubmissionsDirectory();
        if (!Directory.Exists(submissionsDirectory))
            return;

        string[] files = Directory.GetFiles(submissionsDirectory).OrderBy(f => f).ToArray();
        if (files.Length == 0)
            return;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Loaded {files.Length} submission(s)");
        Console.ResetColor();

        _loadingSubmission = true;

        foreach (string? file in files)
        {
            string text = File.ReadAllText(file);
            EvaluateSubmission(text);
        }

        _loadingSubmission = false;
    }

    private static void ClearSubmissions()
    {
        var dir = GetSubmissionsDirectory();
        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
    }
    private void SaveSubmission(string text)
    {
        if (_loadingSubmission)
            return;

        string submissionsDirectory = GetSubmissionsDirectory();
        Directory.CreateDirectory(submissionsDirectory);
        int count = Directory.GetFiles(submissionsDirectory).Length;
        string name = $"submission{count:0000}";
        string fileName = Path.Combine(submissionsDirectory, name);
        File.WriteAllText(fileName, text);
    }
}