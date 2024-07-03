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

        public Compilation(params SyntaxTree[] syntaxTrees)
            : this(null, syntaxTrees) { }

        private Compilation(Compilation previous, params SyntaxTree[] syntaxTrees)
        {
            Previous = previous;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }


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
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
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
                foreach (var builtin in builtinFunctions)
                    if (seenSymbolNames.Add(builtin.Name))
                        yield return builtin;

                foreach (var function in submission.Functions)
                    if (seenSymbolNames.Add(function.Name))
                        yield return function;

                foreach (var variable in submission.Variables)
                    if (seenSymbolNames.Add(variable.Name))
                        yield return variable;

                submission = submission.Previous;
            }
        }

        public Compilation ContinueWith(SyntaxTree syntaxTree) => new(this, syntaxTree);

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var parseErrors = SyntaxTrees.SelectMany(st => st.Errors);

            var errors = parseErrors.Concat(GlobalScope.Errors).ToImmutableArray();
            if (errors.Any())
                return new EvaluationResult(errors, null);

            var program = Binder.BindProgram(GlobalScope);
            if (program.Errors.Any())
                return new EvaluationResult(program.Errors, null);

            Evaluator evaluator = new(program, variables);
            object? value = evaluator.Evaluate();
            return new EvaluationResult([], value);
        }

        public void EmitTree(TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope);
            if (program.Statement.Statements.Any())
            {
                program.Statement.WriteTo(writer);
            }
            else
            {
                foreach (var functionBody in program.Functions)
                {
                    if (!GlobalScope.Functions.Contains(functionBody.Key))
                        continue;

                    functionBody.Key.WriteTo(writer);
                    writer.WriteLine();
                    functionBody.Value.WriteTo(writer);
                }
            }
        }

        public void EmitTree(FunctionSymbol symbol, TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope);
            symbol.WriteTo(writer);
            writer.WriteLine();

            if (!program.Functions.TryGetValue(symbol, out var body))
                return;
            body.WriteTo(writer);
        }

    }
}