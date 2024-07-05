using System.Collections.Immutable;
using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundProgram(BoundProgram previous,
                                       ImmutableArray<Error> errors,
                                       FunctionSymbol mainFunction,
                                       FunctionSymbol scriptFunction,
                                       ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions)
    {
        public BoundProgram Previous { get; } = previous;
        public ImmutableArray<Error> Errors { get; } = errors;
        public FunctionSymbol MainFunction { get; } = mainFunction;
        public FunctionSymbol ScriptFunction { get; } = scriptFunction;
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; } = functions;
    }
}