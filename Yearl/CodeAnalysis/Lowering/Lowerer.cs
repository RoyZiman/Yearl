using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using Yearl.CodeAnalysis.Binding;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        public static BoundStatement Lower(BoundStatement statement)
        {
            Lowerer lowerer = new();
            return lowerer.RewriteStatement(statement);
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