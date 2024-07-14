using System.Collections.Immutable;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type, SyntaxStatementFunctionDeclaration declaration = null) : Symbol(name)
    {
        public override SymbolKind Kind => SymbolKind.Function;
        public SyntaxStatementFunctionDeclaration Declaration { get; } = declaration;
        public ImmutableArray<ParameterSymbol> Parameters { get; } = parameters;
        public TypeSymbol Type { get; } = type;
    }
}

