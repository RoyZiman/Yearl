using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression initializer) : BoundStatement
    {
        public override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;
        public VariableSymbol Variable { get; } = variable;
        public BoundExpression Initializer { get; } = initializer;
    }
}