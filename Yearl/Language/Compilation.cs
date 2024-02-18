using Yearl.Language.Binding;
using Yearl.Language.Syntax;

namespace Yearl.Language
{
    public sealed class Compilation(string path, string code, SyntaxTree syntaxTree)
    {
        public SyntaxTree SyntaxTree { get; } = syntaxTree;
        private readonly string _path = path;
        private readonly string _code = code;

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            Binder binder = new Binder(_path, _code);
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