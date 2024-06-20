using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundForStatement(VariableSymbol variable, BoundExpression firstBoundary, BoundExpression secondBoundary, BoundExpression step, BoundStatement body
                                            , BoundLabel breakLabel, BoundLabel continueLabel) : BoundLoopStatement(breakLabel, continueLabel)
    {
        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
        public VariableSymbol Variable { get; } = variable;
        public BoundExpression FirstBoundary { get; } = firstBoundary;
        public BoundExpression SecondBoundary { get; } = secondBoundary;
        public BoundExpression Step { get; } = step;
        public BoundStatement Body { get; } = body;
    }
}