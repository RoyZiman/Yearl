using System.Collections.Immutable;
using Yearl.CodeAnalysis.Errors;

namespace Yearl.CodeAnalysis
{
    public sealed class EvaluationResult(ImmutableArray<Error> errors, object? value)
    {
        public ImmutableArray<Error> Errors { get; } = errors;
        public object? Value { get; } = value;
    }
}