using System.Collections.Immutable;
using Yearl.Language.Binding;
using Yearl.Language.Syntax;

namespace Yearl.Language
{
    public sealed class Compilation(SyntaxTree syntaxTree)
    {
        public SyntaxTree SyntaxTree { get; } = syntaxTree;

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            Binder binder = new Binder();
            BoundNode boundExpression = binder.BindExpression(SyntaxTree.Root.Expression);

            ImmutableArray<Error> errors = SyntaxTree.Errors.Concat(binder.Errors).ToImmutableArray();
            if (errors.Any())
                return new EvaluationResult(errors, null);

            Evaluator evaluator = new Evaluator(boundExpression, variables);
            object value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Error>.Empty, value);
        }
    }
}