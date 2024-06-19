using System.Collections.Immutable;
using Yearl.CodeAnalysis.Binding;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount = 0;
        private BoundLabel GenerateLabel()
        {
            string name = $"Label{++_labelCount}";
            return new BoundLabel(name);
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

            BoundLabel continueLabel = GenerateLabel();
            BoundLabel checkLabel = GenerateLabel();
            BoundLabel endLabel = GenerateLabel();

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
                BoundLabel endLabel = GenerateLabel();
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

                BoundLabel elseLabel = GenerateLabel();
                BoundLabel endLabel = GenerateLabel();

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

            VariableSymbol firstBoundSymbol = new LocalVariableSymbol("system.FirstBound", true, TypeSymbol.Number);
            BoundVariableDeclarationStatement firstBoundDeclaration = new(firstBoundSymbol, node.FirstBoundary);
            BoundVariableExpression firstBound = new(firstBoundSymbol);

            VariableSymbol secondBoundSymbol = new LocalVariableSymbol("system.SecondBound", true, TypeSymbol.Number);
            BoundVariableDeclarationStatement secondBoundDeclaration = new(secondBoundSymbol, node.SecondBoundary);
            BoundVariableExpression secondBound = new(secondBoundSymbol);

            VariableSymbol stepSymbol = new LocalVariableSymbol("system.Step", true, TypeSymbol.Number);
            BoundVariableDeclarationStatement stepDeclaration = new(stepSymbol, node.Step);
            BoundVariableExpression step = new(stepSymbol);

            BoundVariableDeclarationStatement variableDeclaration = new(node.Variable, firstBound);
            BoundVariableExpression variableExpression = new(node.Variable);


            BoundBinaryExpression firstIfCondition = new(firstBound, BoundBinaryOperator.Bind(SyntaxKind.LessThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number), secondBound);

            BoundBinaryOperator incrementOp(bool invert) => BoundBinaryOperator.Bind(invert ? SyntaxKind.MinusToken : SyntaxKind.PlusToken, TypeSymbol.Number, TypeSymbol.Number);
            BoundBinaryOperator conditionOp(bool invert) => BoundBinaryOperator.Bind(invert ? SyntaxKind.GreaterThanEqualsToken : SyntaxKind.LessThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number);

            BoundBinaryExpression whileCondition(bool invert) => new(variableExpression, conditionOp(invert), secondBound);
            BoundExpressionStatement increment(bool invert) => new(new BoundVariableAssignmentExpression(node.Variable, new BoundBinaryExpression(variableExpression, incrementOp(invert), step)));

            BoundBlockStatement whileBody(bool invert) => new([node.Body, increment(invert)]);
            BoundWhileStatement whileStatement(bool invert) => new(whileCondition(invert), whileBody(invert));

            BoundBlockStatement ifStatementBody(bool invert = false) => new([stepDeclaration, variableDeclaration, whileStatement(invert)]);

            BoundIfStatement firstIfStatement = new(firstIfCondition, ifStatementBody(), ifStatementBody(true));

            BoundBlockStatement result = new([firstBoundDeclaration, secondBoundDeclaration, RewriteStatement(firstIfStatement)]);
            return result;

        }
    }
}