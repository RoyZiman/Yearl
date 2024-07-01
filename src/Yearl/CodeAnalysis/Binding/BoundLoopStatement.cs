namespace Yearl.CodeAnalysis.Binding
{
    internal abstract class BoundLoopStatement(BoundLabel breakLabel, BoundLabel continueLabel) : BoundStatement
    {
        public BoundLabel BreakLabel { get; } = breakLabel;
        public BoundLabel ContinueLabel { get; } = continueLabel;
    }

}