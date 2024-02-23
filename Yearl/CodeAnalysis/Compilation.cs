using System.Collections.Immutable;
using Yearl.CodeAnalysis.Binding;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope? _globalScope;

        public Compilation(SyntaxTree syntaxTree)
            : this(null, syntaxTree) { }

        private Compilation(Compilation previous, SyntaxTree syntaxTree)
        {
            Previous = previous;
            SyntaxTree = syntaxTree;
        }

        public Compilation Previous { get; }
        public SyntaxTree SyntaxTree { get; }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    BoundGlobalScope globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTree.Root);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public Compilation ContinueWith(SyntaxTree syntaxTree)
        {
            return new Compilation(this, syntaxTree);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            ImmutableArray<Error> diagnostics = SyntaxTree.Errors.Concat(GlobalScope.Errors).ToImmutableArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            Evaluator evaluator = new(GlobalScope.Statement, variables);
            object? value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Error>.Empty, value);
        }
    }
}