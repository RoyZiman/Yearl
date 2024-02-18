namespace Yearl.Language
{
    public sealed class EvaluationResult
    {
        public EvaluationResult(IEnumerable<Error> errors, object value)
        {
            Errors = errors.ToArray();
            Value = value;
        }

        public IReadOnlyList<Error> Errors { get; }
        public object Value { get; }
    }
}