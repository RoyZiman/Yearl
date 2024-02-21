using System.Collections.Immutable;

namespace Yearl.Language
{
    public sealed class EvaluationResult
    {
        public EvaluationResult(ImmutableArray<Error> errors, object value)
        {
            Errors = errors;
            Value = value;
        }

        public ImmutableArray<Error> Errors { get; }
        public object Value { get; }
    }
}