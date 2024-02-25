namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class BoundForStatement : BoundStatement
    {
        public BoundForStatement(VariableSymbol variable, BoundExpression firstBoundary, BoundExpression secondBoundary, BoundExpression step, BoundStatement body)
        {
            Variable = variable;
            FirstBoundary = firstBoundary;
            SecondBoundary = secondBoundary;
            Step = step;
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
        public VariableSymbol Variable { get; }
        public BoundExpression FirstBoundary { get; }
        public BoundExpression SecondBoundary { get; }
        public BoundExpression Step { get; }
        public BoundStatement Body { get; }
    }
}