using System.Collections.Immutable;

namespace Yearl.CodeAnalysis.Binding
{
    internal abstract class BoundTreeRewriter
    {
        public virtual BoundStatement RewriteStatement(BoundStatement node)
        {
            return node.Kind switch
            {
                BoundNodeKind.BlockStatement => RewriteBlockStatement((BoundBlockStatement)node),
                BoundNodeKind.VariableDeclarationStatement => RewriteVariableDeclaration((BoundVariableDeclarationStatement)node),
                BoundNodeKind.IfStatement => RewriteIfStatement((BoundIfStatement)node),
                BoundNodeKind.WhileStatement => RewriteWhileStatement((BoundWhileStatement)node),
                BoundNodeKind.ForStatement => RewriteForStatement((BoundForStatement)node),
                BoundNodeKind.LabelStatement => RewriteLabelStatement((BoundLabelStatement)node),
                BoundNodeKind.GotoStatement => RewriteGotoStatement((BoundGotoStatement)node),
                BoundNodeKind.ConditionalGotoStatement => RewriteConditionalGotoStatement((BoundConditionalGotoStatement)node),
                BoundNodeKind.ExpressionStatement => RewriteExpressionStatement((BoundExpressionStatement)node),
                _ => throw new Exception($"Unexpected node: {node.Kind}"),
            };
        }

        protected virtual BoundStatement RewriteBlockStatement(BoundBlockStatement node)
        {
            ImmutableArray<BoundStatement>.Builder? builder = null;

            for (int i = 0; i < node.Statements.Length; i++)
            {
                BoundStatement oldStatement = node.Statements[i];
                BoundStatement newStatement = RewriteStatement(oldStatement);
                if (newStatement != oldStatement)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);

                        for (int j = 0; j < i; j++)
                            builder.Add(node.Statements[j]);
                    }
                }

                builder?.Add(newStatement);
            }

            if (builder == null)
                return node;

            return new BoundBlockStatement(builder.MoveToImmutable());
        }

        protected virtual BoundStatement RewriteVariableDeclaration(BoundVariableDeclarationStatement node)
        {
            BoundExpression initializer = RewriteExpression(node.Initializer);
            if (initializer == node.Initializer)
                return node;

            return new BoundVariableDeclarationStatement(node.Variable, initializer);
        }

        protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            BoundExpression condition = RewriteExpression(node.Condition);
            BoundStatement bodyStatement = RewriteStatement(node.BodyStatement);
            BoundStatement? elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);
            if (condition == node.Condition && bodyStatement == node.BodyStatement && elseStatement == node.ElseStatement)
                return node;

            return new BoundIfStatement(condition, bodyStatement, elseStatement);
        }

        protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            BoundExpression condition = RewriteExpression(node.Condition);
            BoundStatement body = RewriteStatement(node.Body);
            if (condition == node.Condition && body == node.Body)
                return node;

            return new BoundWhileStatement(condition, body);
        }

        protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
        {
            BoundExpression firstBound = RewriteExpression(node.FirstBoundary);
            BoundExpression secondBound = RewriteExpression(node.SecondBoundary);
            BoundExpression stepExpression = RewriteExpression(node.Step);
            BoundStatement body = RewriteStatement(node.Body);
            if (firstBound == node.FirstBoundary && secondBound == node.SecondBoundary && stepExpression == node.Step && body == node.Body)
                return node;

            return new BoundForStatement(node.Variable, firstBound, secondBound, stepExpression, body);
        }

        protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
        {
            BoundExpression expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;

            return new BoundExpressionStatement(expression);
        }

        protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node)
        {
            return node;
        }

        protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node)
        {
            return node;
        }

        protected virtual BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            BoundExpression condition = RewriteExpression(node.Condition);
            if (condition == node.Condition)
                return node;

            return new BoundConditionalGotoStatement(node.Label, condition, node.JumpIfTrue);
        }

        protected virtual BoundExpression RewriteExpression(BoundExpression node)
        {
            return node.Kind switch
            {
                BoundNodeKind.ErrorExpression => RewriteErrorExpression((BoundErrorExpression)node),
                BoundNodeKind.LiteralExpression => RewriteLiteralExpression((BoundLiteralExpression)node),
                BoundNodeKind.VariableExpression => RewriteVariableExpression((BoundVariableExpression)node),
                BoundNodeKind.VariableAssignmentExpression => RewriteAssignmentExpression((BoundVariableAssignmentExpression)node),
                BoundNodeKind.UnaryExpression => RewriteUnaryExpression((BoundUnaryExpression)node),
                BoundNodeKind.BinaryExpression => RewriteBinaryExpression((BoundBinaryExpression)node),
                _ => throw new Exception($"Unexpected node: {node.Kind}"),
            };
        }

        protected virtual BoundExpression RewriteErrorExpression(BoundErrorExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteAssignmentExpression(BoundVariableAssignmentExpression node)
        {
            BoundExpression expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;

            return new BoundVariableAssignmentExpression(node.Variable, expression);
        }

        protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
        {
            BoundExpression expression = RewriteExpression(node.Expression);
            if (expression == node.Expression)
                return node;

            return new BoundUnaryExpression(node.Operator, expression);
        }

        protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
        {
            BoundExpression left = RewriteExpression(node.Left);
            BoundExpression right = RewriteExpression(node.Right);
            if (left == node.Left && right == node.Right)
                return node;

            return new BoundBinaryExpression(left, node.Operator, right);
        }
    }
}
