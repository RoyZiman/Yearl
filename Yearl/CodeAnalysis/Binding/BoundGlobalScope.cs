using System.Collections.Immutable;
using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope(BoundGlobalScope previous, ImmutableArray<Error> errors, ImmutableArray<VariableSymbol> variables, BoundStatement statement)
    {
        public BoundGlobalScope Previous { get; } = previous;
        public ImmutableArray<Error> Errors { get; } = errors;
        public ImmutableArray<VariableSymbol> Variables { get; } = variables;
        public BoundStatement Statement { get; } = statement;
    }
}