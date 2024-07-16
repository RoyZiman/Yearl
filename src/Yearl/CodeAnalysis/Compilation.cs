using System.Collections.Immutable;
using Yearl.CodeAnalysis.Binding;
using Yearl.CodeAnalysis.Errors;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope? _globalScope;

        private Compilation(bool isScript, Compilation? previous, params SyntaxTree[] syntaxTrees)
        {
            IsScript = isScript;
            Previous = previous;
            SyntaxTrees = [.. syntaxTrees];
        }

        public static Compilation Create(params SyntaxTree[] syntaxTrees) => new(isScript: false, previous: null, syntaxTrees);

        public static Compilation CreateScript(Compilation? previous, params SyntaxTree[] syntaxTrees) => new(isScript: true, previous, syntaxTrees);

        public bool IsScript { get; }
        public Compilation? Previous { get; }
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
        public FunctionSymbol? MainFunction => GlobalScope.MainFunction;
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

            var builtinFunctions = BuiltinFunctions.GetAll().ToList();

            while (submission != null)
            {
                
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
            if (GlobalScope.Errors.Any())
                return new EvaluationResult(GlobalScope.Errors, null);

            var program = GetProgram();
            if (program.Errors.Any())
                return new EvaluationResult(program.Errors, null);

            Evaluator evaluator = new(program, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult([], value);
        }

        public void EmitTree(TextWriter writer)
        {
            if (MainFunction != null)
                EmitTree(MainFunction, writer);
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

        public ImmutableArray<Error> Emit(string moduleName, string[] references, string outputPath)
        {
            var parseErrors = SyntaxTrees.SelectMany(st => st.Errors);

            var errors = parseErrors.Concat(GlobalScope.Errors).ToImmutableArray();
            if (errors.Any())
                return errors;

            var program = GetProgram();
            return Emitter.Emit(program, moduleName, references, outputPath);
        }
    }
}