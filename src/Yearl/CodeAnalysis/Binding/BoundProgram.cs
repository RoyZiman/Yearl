using System.Collections.Immutable;
using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundProgram(BoundProgram previous, ImmutableArray<Error> errors, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, BoundBlockStatement statement)
    {
        public BoundProgram Previous { get; } = previous;
        public ImmutableArray<Error> Errors { get; } = errors;
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; } = functions;
        public BoundBlockStatement Statement { get; } = statement;
    }
}