using System.Collections.Immutable;
using Yearl.CodeAnalysis.Errors;
using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope(BoundGlobalScope previous,
                                           ImmutableArray<Error> errors,
                                           FunctionSymbol mainFunction,
                                           FunctionSymbol scriptFunction,
                                           ImmutableArray<FunctionSymbol> functions,
                                           ImmutableArray<VariableSymbol> variables,
                                           ImmutableArray<BoundStatement> statements)
    {
        public BoundGlobalScope Previous { get; } = previous;
        public ImmutableArray<Error> Errors { get; } = errors;
        public FunctionSymbol MainFunction { get; } = mainFunction;
        public FunctionSymbol ScriptFunction { get; } = scriptFunction;
        public ImmutableArray<FunctionSymbol> Functions { get; } = functions;
        public ImmutableArray<VariableSymbol> Variables { get; } = variables;
        public ImmutableArray<BoundStatement> Statements { get; } = statements;
    }
}