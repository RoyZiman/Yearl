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
            // goto continue
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            BoundLabel bodyLabel = GenerateLabel();

            var gotoContinue = new BoundGotoStatement(node.ContinueLabel);
            var bodyLabelStatement = new BoundLabelStatement(bodyLabel);
            var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
            var gotoTrue = new BoundConditionalGotoStatement(bodyLabel, node.Condition);
            var breakLabelStatement = new BoundLabelStatement(node.BreakLabel);

            var result = new BoundBlockStatement(
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

            var variableDeclaration = new BoundVariableDeclarationStatement(node.Variable, node.FirstBoundary);
            var variableExpression = new BoundVariableExpression(node.Variable);

            var secondBoundSymbol = new LocalVariableSymbol("System.SecondBound", true, TypeSymbol.Number);
            var secondBoundDeclaration = new BoundVariableDeclarationStatement(secondBoundSymbol, node.SecondBoundary);

            var stepSymbol = new LocalVariableSymbol("System.Step", true, TypeSymbol.Number);
            var stepDeclaration = new BoundVariableDeclarationStatement(stepSymbol, node.Step);

            var positiveStepCondition = new BoundBinaryExpression(
                new BoundBinaryExpression(
                    new BoundVariableExpression(stepSymbol),
                    BoundBinaryOperator.Bind(SyntaxKind.GreaterThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number),
                    new BoundLiteralExpression(0d)
                ),
                BoundBinaryOperator.Bind(SyntaxKind.AndToken, TypeSymbol.Bool, TypeSymbol.Bool),
                new BoundBinaryExpression(
                    variableExpression,
                    BoundBinaryOperator.Bind(SyntaxKind.LessThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number),
                    new BoundVariableExpression(secondBoundSymbol)
                )
            );

            var negativeStepCondition = new BoundBinaryExpression(
                new BoundBinaryExpression(
                    new BoundVariableExpression(stepSymbol),
                    BoundBinaryOperator.Bind(SyntaxKind.LessThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number),
                    new BoundLiteralExpression(0d)
                ),
                BoundBinaryOperator.Bind(SyntaxKind.AndToken, TypeSymbol.Bool, TypeSymbol.Bool),
                new BoundBinaryExpression(
                    variableExpression,
                    BoundBinaryOperator.Bind(SyntaxKind.GreaterThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number),
                    new BoundVariableExpression(secondBoundSymbol)
                )
            );

            var condition = new BoundBinaryExpression(
                positiveStepCondition,
                BoundBinaryOperator.Bind(SyntaxKind.OrToken, TypeSymbol.Bool, TypeSymbol.Bool),
                negativeStepCondition
            );

            var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);

            var increment = new BoundExpressionStatement(
                new BoundVariableAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Number, TypeSymbol.Number),
                        new BoundVariableExpression(stepSymbol)
                    )
                )
            );

            var whileBody = new BoundBlockStatement([node.Body, continueLabelStatement, increment
]);

            var whileStatement = new BoundWhileStatement(condition, whileBody, node.BreakLabel, GenerateLabel());

            var result = new BoundBlockStatement(
                         [
                            variableDeclaration,
                            secondBoundDeclaration,
                            stepDeclaration,
                            whileStatement
                        ]);

            return RewriteStatement(result);


            // var firstBoundSymbol = new LocalVariableSymbol("System.FirstBound", true, TypeSymbol.Number);
            // var firstBoundDeclaration = new BoundVariableDeclarationStatement(firstBoundSymbol, node.SecondBoundary);
            // var firstBoundExpression = new BoundVariableExpression(firstBoundSymbol);
            // var secondBoundSymbol = new LocalVariableSymbol("System.SecondBound", true, TypeSymbol.Number);
            // var secondBoundDeclaration = new BoundVariableDeclarationStatement(secondBoundSymbol, node.SecondBoundary);
            // var secondBoundExpression = new BoundVariableExpression(secondBoundSymbol);


            // var variableDeclaration = new BoundVariableDeclarationStatement(node.Variable, firstBoundExpression);
            // var variableExpression = new BoundVariableExpression(node.Variable);


            // var ascendingCondition = new BoundBinaryExpression(
            //     new BoundBinaryExpression(
            //         firstBoundExpression,
            //         BoundBinaryOperator.Bind(SyntaxKind.LessThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number),
            //         variableExpression
            //     ),
            //     BoundBinaryOperator.Bind(SyntaxKind.AndToken, TypeSymbol.Bool, TypeSymbol.Bool),
            //     new BoundBinaryExpression(
            //         variableExpression,
            //         BoundBinaryOperator.Bind(SyntaxKind.LessThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number),
            //         secondBoundExpression

            //     )
            // );

            // var descendingCondition = new BoundBinaryExpression(
            //     new BoundBinaryExpression(
            //         firstBoundExpression,
            //         BoundBinaryOperator.Bind(SyntaxKind.GreaterThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number),
            //         variableExpression
            //     ),
            //     BoundBinaryOperator.Bind(SyntaxKind.AndToken, TypeSymbol.Bool, TypeSymbol.Bool),
            //     new BoundBinaryExpression(
            //         variableExpression,
            //         BoundBinaryOperator.Bind(SyntaxKind.GreaterThanEqualsToken, TypeSymbol.Number, TypeSymbol.Number),
            //         secondBoundExpression
            //     )
            // );

            // var condition = new BoundBinaryExpression(
            //     ascendingCondition,
            //     BoundBinaryOperator.Bind(SyntaxKind.OrToken, TypeSymbol.Bool, TypeSymbol.Bool),
            //     descendingCondition
            // );

            // var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);

            // var increment = new BoundExpressionStatement(
            //    new BoundVariableAssignmentExpression(
            //        node.Variable,
            //        new BoundBinaryExpression(
            //            variableExpression,
            //            BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Number, TypeSymbol.Number),
            //            node.Step
            //        )
            //    )
            //);

            // var whileBody = new BoundBlockStatement([node.Body, continueLabelStatement, increment]
            //             );
            // var whileStatement = new BoundWhileStatement(condition, whileBody, node.BreakLabel, GenerateLabel());

            // var result = new BoundBlockStatement([firstBoundDeclaration, secondBoundDeclaration, variableDeclaration, whileStatement]);

            // return RewriteStatement(result);
        }
    }
}