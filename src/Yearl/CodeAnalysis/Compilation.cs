using System.Collections.Immutable;
using Yearl.CodeAnalysis.Binding;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;
using ReflectionBindingFlags = System.Reflection.BindingFlags;

namespace Yearl.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope? _globalScope;

        private Compilation(bool isScript, Compilation previous, params SyntaxTree[] syntaxTrees)
        {
            IsScript = isScript;
            Previous = previous;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public static Compilation Create(params SyntaxTree[] syntaxTrees) => new(isScript: false, previous: null, syntaxTrees);

        public static Compilation CreateScript(Compilation previous, params SyntaxTree[] syntaxTrees) => new(isScript: true, previous, syntaxTrees);

        public bool IsScript { get; }
        public Compilation Previous { get; }
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
        public ImmutableArray<FunctionSymbol> Functions => GlobalScope.Functions;
        public ImmutableArray<VariableSymbol> Variables => GlobalScope.Variables;

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(IsScript, Previous?.GlobalScope, SyntaxTrees);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public IEnumerable<Symbol> GetSymbols()
        {
            var submission = this;
            HashSet<string> seenSymbolNames = [];

            while (submission != null)
            {
                const ReflectionBindingFlags bindingFlags =
                   ReflectionBindingFlags.Static |
                   ReflectionBindingFlags.Public |
                   ReflectionBindingFlags.NonPublic;
                var builtinFunctions = typeof(BuiltinFunctions)
                    .GetFields(bindingFlags)
                    .Where(fi => fi.FieldType == typeof(FunctionSymbol))
                    .Select(fi => (FunctionSymbol)fi.GetValue(obj: null))
                    .ToList();

                foreach (var function in submission.Functions)
                    if (seenSymbolNames.Add(function.Name))
                        yield return function;

                foreach (var variable in submission.Variables)
                    if (seenSymbolNames.Add(variable.Name))
                        yield return variable;

                foreach (var builtin in builtinFunctions)
                    if (seenSymbolNames.Add(builtin.Name))
                        yield return builtin;

                submission = submission.Previous;
            }
        }

        private BoundProgram GetProgram()
        {
            var previous = Previous?.GetProgram();
            return Binder.BindProgram(IsScript, previous, GlobalScope);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var parseErrors = SyntaxTrees.SelectMany(st => st.Errors);

            var errors = parseErrors.Concat(GlobalScope.Errors).ToImmutableArray();
            if (errors.Any())
                return new EvaluationResult(errors, null);

            var program = GetProgram();
            if (program.Errors.Any())
                return new EvaluationResult(program.Errors, null);

            Evaluator evaluator = new(program, variables);
            object? value = evaluator.Evaluate();
            return new EvaluationResult([], value);
        }

        public void EmitTree(TextWriter writer)
        {
            var program = GetProgram();
            if (GlobalScope.MainFunction != null)
                EmitTree(GlobalScope.MainFunction, writer);
            else if (GlobalScope.ScriptFunction != null)
                EmitTree(GlobalScope.ScriptFunction, writer);
        }

        public void EmitTree(FunctionSymbol symbol, TextWriter writer)
        {
            var program = GetProgram();
            symbol.WriteTo(writer);
            writer.WriteLine();

            if (!program.Functions.TryGetValue(symbol, out var body))
                return;
            body.WriteTo(writer);
        }

    }
}