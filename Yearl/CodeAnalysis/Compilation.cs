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
            BoundNode boundExpression = binder.BindNode(SyntaxTree.Root);

            Error[] errors = SyntaxTree.Errors.Concat(binder.Errors).ToArray();
            if (errors.Any())
                return new EvaluationResult(errors, null);

            Evaluator evaluator = new Evaluator(boundExpression, variables);
            object value = evaluator.Evaluate();
            return new EvaluationResult([], value);
        }
    }
}