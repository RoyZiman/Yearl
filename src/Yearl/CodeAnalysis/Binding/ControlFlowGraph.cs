using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis.Binding
{
    internal sealed class ControlFlowGraph
    {
        private ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks, List<BasicBlockBranch> branches)
        {
            Start = start;
            End = end;
            Blocks = blocks;
            Branches = branches;
        }

        public BasicBlock Start { get; }
        public BasicBlock End { get; }
        public List<BasicBlock> Blocks { get; }
        public List<BasicBlockBranch> Branches { get; }

        public sealed class BasicBlock
        {
            public BasicBlock()
            {
            }

            public BasicBlock(bool isStart)
            {
                IsStart = isStart;
                IsEnd = !isStart;
            }

            public bool IsStart { get; }
            public bool IsEnd { get; }
            public List<BoundStatement> Statements { get; } = [];
            public List<BasicBlockBranch> Incoming { get; } = [];
            public List<BasicBlockBranch> Outgoing { get; } = [];

        }

        public sealed class BasicBlockBranch(BasicBlock from, BasicBlock to, BoundExpression condition)
        {
            public BasicBlock From { get; } = from;
            public BasicBlock To { get; } = to;
            public BoundExpression Condition { get; } = condition;
        }

        public sealed class BasicBlockBuilder
        {
            private List<BoundStatement> _statements = [];
            private List<BasicBlock> _blocks = [];

            public List<BasicBlock> Build(BoundBlockStatement block)
            {
                foreach (var statement in block.Statements)
                {
                    switch (statement.Kind)
                    {
                        case BoundNodeKind.LabelStatement:
                            StartBlock();
                            _statements.Add(statement);
                            break;
                        case BoundNodeKind.GotoStatement:
                        case BoundNodeKind.ConditionalGotoStatement:
                        case BoundNodeKind.ReturnStatement:
                            _statements.Add(statement);
                            StartBlock();
                            break;
                        case BoundNodeKind.NopStatement:
                        case BoundNodeKind.VariableDeclarationStatement:
                        case BoundNodeKind.ExpressionStatement:
                            _statements.Add(statement);
                            break;
                        default:
                            throw new Exception($"Unexpected statement: {statement.Kind}");
                    }
                }

                EndBlock();

                return _blocks.ToList();
            }

            private void StartBlock() => EndBlock();

            private void EndBlock()
            {
                if (_statements.Count > 0)
                {
                    BasicBlock block = new();
                    block.Statements.AddRange(_statements);
                    _blocks.Add(block);
                    _statements.Clear();
                }
            }
        }

        public sealed class GraphBuilder
        {
            private Dictionary<BoundStatement, BasicBlock> _blockFromStatement = [];
            private Dictionary<BoundLabel, BasicBlock> _blockFromLabel = [];
            private List<BasicBlockBranch> _branches = [];
            private BasicBlock _start = new(isStart: true);
            private BasicBlock _end = new(isStart: false);

            public ControlFlowGraph Build(List<BasicBlock> blocks)
            {
                if (blocks.Count == 0)
                    Connect(_start, _end);
                else
                    Connect(_start, blocks.First());

                foreach (var block in blocks)
                {
                    foreach (var statement in block.Statements)
                    {
                        _blockFromStatement.Add(statement, block);
                        if (statement is BoundLabelStatement labelStatement)
                            _blockFromLabel.Add(labelStatement.Label, block);
                    }
                }

                for (int i = 0; i < blocks.Count; i++)
                {
                    var current = blocks[i];
                    var next = i == blocks.Count - 1 ? _end : blocks[i + 1];

                    foreach (var statement in current.Statements)
                    {
                        bool isLastStatementInBlock = statement == current.Statements.Last();
                        switch (statement.Kind)
                        {
                            case BoundNodeKind.GotoStatement:
                                var gs = (BoundGotoStatement)statement;
                                var toBlock = _blockFromLabel[gs.Label];
                                Connect(current, toBlock);
                                break;
                            case BoundNodeKind.ConditionalGotoStatement:
                                var cgs = (BoundConditionalGotoStatement)statement;
                                var thenBlock = _blockFromLabel[cgs.Label];
                                var elseBlock = next;
                                var negatedCondition = Negate(cgs.Condition); ;
                                var thenCondition = cgs.JumpIfTrue ? cgs.Condition : negatedCondition;
                                var elseCondition = cgs.JumpIfTrue ? negatedCondition : cgs.Condition;
                                Connect(current, thenBlock, thenCondition);
                                Connect(current, elseBlock, elseCondition);
                                break;
                            case BoundNodeKind.ReturnStatement:
                                Connect(current, _end);
                                break;
                            case BoundNodeKind.NopStatement:
                            case BoundNodeKind.VariableDeclarationStatement:
                            case BoundNodeKind.LabelStatement:
                            case BoundNodeKind.ExpressionStatement:
                                if (isLastStatementInBlock)
                                    Connect(current, next);
                                break;
                            default:
                                throw new Exception($"Unexpected statement: {statement.Kind}");
                        }
                    }
                }

            ScanAgain:
                foreach (var block in blocks)
                {
                    if (block.Incoming.Count == 0)
                    {
                        RemoveBlock(blocks, block);
                        goto ScanAgain;
                    }
                }

                blocks.Insert(0, _start);
                blocks.Add(_end);

                return new ControlFlowGraph(_start, _end, blocks, _branches);
            }

            private void Connect(BasicBlock from, BasicBlock to, BoundExpression condition = null)
            {
                if (condition is BoundLiteralExpression l)
                {
                    bool value = (bool)l.Value;
                    if (value)
                        condition = null;
                    else
                        return;
                }

                BasicBlockBranch branch = new(from, to, condition);
                from.Outgoing.Add(branch);
                to.Incoming.Add(branch);
                _branches.Add(branch);
            }

            private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
            {
                foreach (var branch in block.Incoming)
                {
                    branch.From.Outgoing.Remove(branch);
                    _branches.Remove(branch);
                }

                foreach (var branch in block.Outgoing)
                {
                    branch.To.Incoming.Remove(branch);
                    _branches.Remove(branch);
                }

                blocks.Remove(block);
            }

            private BoundExpression Negate(BoundExpression condition)
            {
                if (condition is BoundLiteralExpression literal)
                {
                    bool value = (bool)literal.Value;
                    return new BoundLiteralExpression(!value);
                }

                var op = BoundUnaryOperator.Bind(SyntaxKind.NotToken, TypeSymbol.Bool);
                return new BoundUnaryExpression(op, condition);
            }
        }

        public static ControlFlowGraph Create(BoundBlockStatement body)
        {
            BasicBlockBuilder basicBlockBuilder = new();
            var blocks = basicBlockBuilder.Build(body);

            GraphBuilder graphBuilder = new();
            return graphBuilder.Build(blocks);
        }

        public static bool AllPathsReturn(BoundBlockStatement body)
        {
            var graph = Create(body);

            foreach (var branch in graph.End.Incoming)
            {
                var lastStatement = branch.From.Statements.LastOrDefault();
                if (lastStatement == null || lastStatement.Kind != BoundNodeKind.ReturnStatement)
                    return false;
            }

            return true;
        }
    }
}