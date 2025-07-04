﻿using System.Collections.Immutable;
using Yearl.CodeAnalysis.Binding;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount = 0;
        private BoundLabel GenerateLabel()
        {
            string name = $"Label{++_labelCount}";
            return new BoundLabel(name);
        }
        public static BoundBlockStatement Lower(FunctionSymbol function, BoundStatement statement)
        {
            Lowerer lowerer = new();
            var result = lowerer.RewriteStatement(statement);
            return RemoveDeadCode(Flatten(function, result));
        }

        private static BoundBlockStatement Flatten(FunctionSymbol function, BoundStatement statement)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            Stack<BoundStatement> stack = new();
            stack.Push(statement);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is BoundBlockStatement block)
                {
                    foreach (var s in block.Statements.Reverse())
                        stack.Push(s);
                }
                else
                {
                    builder.Add(current);
                }
            }

            if (function.Type == TypeSymbol.Void)
            {
                if (builder.Count == 0 || CanFallThrough(builder.Last()))
                {
                    builder.Add(new BoundReturnStatement(null));
                }
            }

            return new BoundBlockStatement(builder.ToImmutable());
        }

        private static bool CanFallThrough(BoundStatement boundStatement)
        {
            return boundStatement.Kind != BoundNodeKind.ReturnStatement &&
                   boundStatement.Kind != BoundNodeKind.GotoStatement;
        }

        private static BoundBlockStatement RemoveDeadCode(BoundBlockStatement node)
        {
            var controlFlow = ControlFlowGraph.Create(node);
            var reachableStatements = new HashSet<BoundStatement>(
                controlFlow.Blocks.SelectMany(b => b.Statements));

            var builder = node.Statements.ToBuilder();
            for (int i = builder.Count - 1; i >= 0; i--)
            {
                if (!reachableStatements.Contains(builder[i]))
                    builder.RemoveAt(i);
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
            // goto continue
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            var bodyLabel = GenerateLabel();

            BoundGotoStatement gotoContinue = new(node.ContinueLabel);
            BoundLabelStatement bodyLabelStatement = new(bodyLabel);
            BoundLabelStatement continueLabelStatement = new(node.ContinueLabel);
            BoundConditionalGotoStatement gotoTrue = new(bodyLabel, node.Condition);
            BoundLabelStatement breakLabelStatement = new(node.BreakLabel);

            BoundBlockStatement result = new(
            [
                gotoContinue,
                bodyLabelStatement,
                node.Body,
                continueLabelStatement,
                gotoTrue,
                breakLabelStatement
            ]);

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
                var endLabel = GenerateLabel();
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

                var elseLabel = GenerateLabel();
                var endLabel = GenerateLabel();

                BoundConditionalGotoStatement gotoFalse = new(elseLabel, node.Condition, false);
                BoundGotoStatement gotoEndStatement = new(endLabel);
                BoundLabelStatement elseLabelStatement = new(elseLabel);
                BoundLabelStatement endLabelStatement = new(endLabel);
                BoundBlockStatement result = new(
                [
                    gotoFalse,
                    node.BodyStatement,
                    gotoEndStatement,
                    elseLabelStatement,
                    node.ElseStatement,
                    endLabelStatement
                ]);
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
            //      var <var> = <first>
            //      const System.SecondBound = <second>
            //      const System.Step = <step>
            //
            //      while (System.Step >= 0 && <var> <= System.SecondBound ||
            //             System.Step <= 0 && <var> >= System.SecondBound
            //      {
            //          <body>
            //          <var> = <var> + <step>
            //      }   
            // }
            //

            BoundVariableDeclarationStatement variableDeclaration = new(node.Variable, node.FirstBoundary);
            BoundVariableExpression variableExpression = new(node.Variable);

            LocalVariableSymbol secondBoundSymbol = new("System.SecondBound", true, TypeSymbol.Number, node.SecondBoundary.ConstantValue!);
            BoundVariableDeclarationStatement secondBoundDeclaration = new(secondBoundSymbol, node.SecondBoundary);

            LocalVariableSymbol stepSymbol = new("System.Step", true, TypeSymbol.Number, node.Step.ConstantValue!);
            BoundVariableDeclarationStatement stepDeclaration = new(stepSymbol, node.Step);

            BoundBinaryExpression positiveStepCondition = new(
                new BoundBinaryExpression(
                    new BoundVariableExpression(stepSymbol),
                    BoundBinaryOperator.Bind(SyntaxKind.GreaterThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number)!,
                    new BoundLiteralExpression(0d)
                ),
                BoundBinaryOperator.Bind(SyntaxKind.AndToken, TypeSymbol.Bool, TypeSymbol.Bool)!,
                new BoundBinaryExpression(
                    variableExpression,
                    BoundBinaryOperator.Bind(SyntaxKind.LessThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number)!,
                    new BoundVariableExpression(secondBoundSymbol)
                )
            );

            BoundBinaryExpression negativeStepCondition = new(
                new BoundBinaryExpression(
                    new BoundVariableExpression(stepSymbol),
                    BoundBinaryOperator.Bind(SyntaxKind.LessThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number)!,
                    new BoundLiteralExpression(0d)
                ),
                BoundBinaryOperator.Bind(SyntaxKind.AndToken, TypeSymbol.Bool, TypeSymbol.Bool)!,
                new BoundBinaryExpression(
                    variableExpression,
                    BoundBinaryOperator.Bind(SyntaxKind.GreaterThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number)!,
                    new BoundVariableExpression(secondBoundSymbol)
                )
            );

            BoundBinaryExpression condition = new(
                positiveStepCondition,
                BoundBinaryOperator.Bind(SyntaxKind.OrToken, TypeSymbol.Bool, TypeSymbol.Bool)!,
                negativeStepCondition
            );

            BoundLabelStatement continueLabelStatement = new(node.ContinueLabel);

            BoundExpressionStatement increment = new(
                new BoundVariableAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Number, TypeSymbol.Number)!,
                        new BoundVariableExpression(stepSymbol)
                    )
                )
            );

            BoundBlockStatement whileBody = new([node.Body, continueLabelStatement, increment
]);

            BoundWhileStatement whileStatement = new(condition, whileBody, node.BreakLabel, GenerateLabel());

            BoundBlockStatement result = new(
                         [
                            variableDeclaration,
                            secondBoundDeclaration,
                            stepDeclaration,
                            whileStatement
                        ]);

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            if (node.Condition.ConstantValue != null)
            {
                bool condition = (bool)node.Condition.ConstantValue.Value;
                condition = node.JumpIfTrue ? condition : !condition;
                if (condition)
                    return RewriteStatement(new BoundGotoStatement(node.Label));
                else
                    return RewriteStatement(new BoundNopStatement());
            }

            return base.RewriteConditionalGotoStatement(node);
        }
    }
}