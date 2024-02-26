using System.Collections.Immutable;
using Yearl.CodeAnalysis.Binding;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount = 0;
        private LabelSymbol GenerateLabel()
        {
            string name = $"Label{++_labelCount}";
            return new LabelSymbol(name);
        }
        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            Lowerer lowerer = new();
            BoundStatement result = lowerer.RewriteStatement(statement);
            return Flatten(result);
        }

        private static BoundBlockStatement Flatten(BoundStatement statement)
        {
            ImmutableArray<BoundStatement>.Builder builder = ImmutableArray.CreateBuilder<BoundStatement>();
            Stack<BoundStatement> stack = new();
            stack.Push(statement);

            while (stack.Count > 0)
            {
                BoundStatement current = stack.Pop();

                if (current is BoundBlockStatement block)
                {
                    foreach (BoundStatement? s in block.Statements.Reverse())
                        stack.Push(s);
                }
                else
                {
                    builder.Add(current);
                }
            }

            return new BoundBlockStatement(builder.ToImmutable());
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            // while <condition>
            //      <bode>
            //
            // ----->
            //
            // goto check
            // continue:
            // <body>
            // check:
            // gotoTrue <condition> continue
            // end:
            //

            LabelSymbol continueLabel = GenerateLabel();
            LabelSymbol checkLabel = GenerateLabel();
            LabelSymbol endLabel = GenerateLabel();

            BoundGotoStatement gotoCheck = new(checkLabel);
            BoundLabelStatement continueLabelStatement = new(continueLabel);
            BoundLabelStatement checkLabelStatement = new(checkLabel);
            BoundConditionalGotoStatement gotoTrue = new(continueLabel, node.Condition);
            BoundLabelStatement endLabelStatement = new(endLabel);

            BoundBlockStatement result = new([gotoCheck, continueLabelStatement, node.Body, checkLabelStatement, gotoTrue, endLabelStatement]);

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                // if <condition>
                //      <then>
                //
                // ---->
                //
                // gotoFalse <condition> end
                // <then>  
                // end:
                LabelSymbol endLabel = GenerateLabel();
                BoundConditionalGotoStatement gotoFalse = new(endLabel, node.Condition, false);
                BoundLabelStatement endLabelStatement = new(endLabel);
                BoundBlockStatement result = new([gotoFalse, node.BodyStatement, endLabelStatement]);
                return RewriteStatement(result);
            }
            else
            {
                // if <condition>
                //      <then>
                // else
                //      <else>
                //
                // ---->
                //
                // gotoFalse <condition> else
                // <then>
                // goto end
                // else:
                // <else>
                // end:

                LabelSymbol elseLabel = GenerateLabel();
                LabelSymbol endLabel = GenerateLabel();

                BoundConditionalGotoStatement gotoFalse = new(elseLabel, node.Condition, false);
                BoundGotoStatement gotoEndStatement = new(endLabel);
                BoundLabelStatement elseLabelStatement = new(elseLabel);
                BoundLabelStatement endLabelStatement = new(endLabel);
                BoundBlockStatement result = new(ImmutableArray.Create<BoundStatement>(
                    gotoFalse,
                    node.BodyStatement,
                    gotoEndStatement,
                    elseLabelStatement,
                    node.ElseStatement,
                    endLabelStatement
                ));
                return RewriteStatement(result);
            }
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            // for <var> from <first> to <second>
            //      <body>
            //
            // ---->
            //
            // {
            //      var <var> = <lower>
            //      while (<var> <= <upper>)
            //      {
            //          <body>
            //          <var> = <var> +- <step>
            //      }   
            // }
            //
            // ---->
            //
            // {
            //      if (<first> <= <second>)
            //      {
            //          var <var> = <first>
            //          while (<var> <= <second>)
            //          {
            //              <body>
            //              <var> = <var> + <step>
            //          }
            //      }
            //      else
            //      {
            //          var <var> = <first>
            //          while (<var> >= <second>)
            //          {
            //              <body>
            //              <var> = <var> - <step>
            //          }  
            //      }
            //
            // }
            //

            BoundBinaryExpression firstIfCondition = new(node.FirstBoundary, BoundBinaryOperator.Bind(SyntaxKind.LessThanEqualsToken, typeof(double), typeof(double)), node.SecondBoundary);

            BoundBinaryOperator incrementOp(bool invert) => BoundBinaryOperator.Bind(invert ? SyntaxKind.MinusToken : SyntaxKind.PlusToken, typeof(double), typeof(double));
            BoundBinaryOperator conditionOp(bool invert) => BoundBinaryOperator.Bind(invert ? SyntaxKind.GreaterThanEqualsToken : SyntaxKind.LessThanEqualsToken, typeof(double), typeof(double));

            BoundVariableDeclarationStatement variableDeclaration = new(node.Variable, node.FirstBoundary);
            BoundVariableExpression variableExpression = new(node.Variable);

            BoundBinaryExpression whileCondition(bool invert) => new(variableExpression, conditionOp(invert), node.SecondBoundary);
            BoundExpressionStatement increment(bool invert) => new(new BoundVariableAssignmentExpression(node.Variable, new BoundBinaryExpression(variableExpression, incrementOp(invert), node.Step)));

            BoundBlockStatement whileBody(bool invert) => new([node.Body, increment(invert)]);
            BoundWhileStatement whileStatement(bool invert) => new(whileCondition(invert), whileBody(invert));

            BoundBlockStatement ifStatementBody(bool invert = false) => new([variableDeclaration, whileStatement(invert)]);

            BoundIfStatement firstIfStatement = new(firstIfCondition, ifStatementBody(), ifStatementBody(true));

            return RewriteStatement(firstIfStatement);

        }
    }
}