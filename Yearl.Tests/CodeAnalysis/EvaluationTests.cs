using Yearl.CodeAnalysis;
using Yearl.CodeAnalysis.Syntax;
using Yearl.Tests.CodeAnalysis.Text;


namespace Yearl.Tests.CodeAnalysis
{
    public class EvaluationTests
    {
        [Theory]
        [InlineData("1", 1.0)]
        [InlineData("+1", 1.0)]
        [InlineData("-1", -1.0)]
        [InlineData("14 + 12", 26.0)]
        [InlineData("12 - 3", 9.0)]
        [InlineData("4 * 2", 8.0)]
        [InlineData("9 / 3", 3.0)]
        [InlineData("5 ^ 2", 25.0)]
        [InlineData("(10)", 10.0)]
        [InlineData("12 == 3", false)]
        [InlineData("3 == 3", true)]
        [InlineData("12 != 3", true)]
        [InlineData("3 != 3", false)]
        [InlineData("False == False", true)]
        [InlineData("True == False", false)]
        [InlineData("False != False", false)]
        [InlineData("True != False", true)]
        [InlineData("0 < 1", true)]
        [InlineData("0 < 0", false)]
        [InlineData("1 < 0", false)]
        [InlineData("0 <= 1", true)]
        [InlineData("0 <= 0", true)]
        [InlineData("1 <= 0", false)]
        [InlineData("0 > 1", false)]
        [InlineData("0 > 0", false)]
        [InlineData("1 > 0", true)]
        [InlineData("0 >= 1", false)]
        [InlineData("0 >= 0", true)]
        [InlineData("1 >= 0", true)]
        [InlineData("True", true)]
        [InlineData("False", false)]
        [InlineData("!True", false)]
        [InlineData("!False", true)]
        [InlineData("{ var a = 0 (a = 10) * a }", 100.0)]
        public void Evaluator_Computes_CorrectValues(string text, object expectedValue)
        {
            AssertValue(text, expectedValue);
        }

        [Fact]
        public void Evaluator_VariableDeclarationStatement_Reports_Redeclaration()
        {
            string text = @"
                {
                    var x = 10
                    var y = 100
                    {
                        var x = 10
                    }
                    var [x] = 5
                }
            ";

            string diagnostics = @"
                Variable 'x' is already declared.
            ";

            AssertErrors(text, diagnostics);
        }

        [Fact]
        public void Evaluator_NameExpression_Reports_Undefined()
        {
            string text = @"[x] * 10";

            string diagnostics = @"
                Variable 'x' doesn't exist.
            ";

            AssertErrors(text, diagnostics);
        }

        [Fact]
        public void Evaluator_VariableAssignment_Reports_Undefined()
        {
            string text = @"[x] = 10";

            string diagnostics = @"
                Variable 'x' doesn't exist.
            ";

            AssertErrors(text, diagnostics);
        }

        [Fact]
        public void Evaluator_VariableAssignment_Reports_CannotAssign()
        {
            string text = @"
                {
                    const x = 10
                    x [=] 0
                }
            ";

            string diagnostics = @"
                Variable 'x' is read-only and cannot be assigned to.
            ";

            AssertErrors(text, diagnostics);
        }

        [Fact]
        public void Evaluator_VariableAssignment_Reports_CannotConvert()
        {
            string text = @"
                {
                    var x = 10
                    x = [True]
                }
            ";

            string diagnostics = @"
                Cannot convert type 'System.Boolean' to 'System.Double'.
            ";

            AssertErrors(text, diagnostics);
        }

        [Fact]
        public void Evaluator_BlockStatement_NoInfiniteLoop()
        {
            string text = @"
                {
                [)][]
            ";

            string diagnostics = @"
                Unexpected token <RightParenthesisToken>, expected <IdentifierToken>.
                Unexpected token <EndOfFileToken>, expected <RightCurlyBraceToken>.
            ";

            AssertErrors(text, diagnostics);
        }

        [Fact]
        public void Evaluator_NameExpression_Reports_NoErrorForInsertedToken()
        {
            string text = @"[]";

            string diagnostics = @"
                Unexpected token <EndOfFileToken>, expected <IdentifierToken>.
            ";

            AssertErrors(text, diagnostics);
        }

        [Fact]
        public void Evaluator_UnaryExpression_Reports_Undefined()
        {
            string text = @"[+]True";

            string diagnostics = @"
                Unary operator '+' is not defined for type 'System.Boolean'.
            ";

            AssertErrors(text, diagnostics);
        }

        [Fact]
        public void Evaluator_BinaryExpression_Reports_Undefined()
        {
            string text = @"10 [*] False";

            string diagnostics = @"
                Binary operator '*' is not defined for types 'System.Double' and 'System.Boolean'.
            ";

            AssertErrors(text, diagnostics);
        }

        private static void AssertValue(string text, object expectedValue)
        {
            SyntaxTree syntaxTree = SyntaxTree.Parse(text);
            Compilation compilation = new(syntaxTree);
            Dictionary<VariableSymbol, object> variables = new();
            EvaluationResult result = compilation.Evaluate(variables);

            Assert.Empty(result.Errors);
            Assert.Equal(expectedValue, result.Value);
        }

        private static void AssertErrors(string text, string diagnosticText)
        {
            AnnotatedText annotatedText = AnnotatedText.Parse(text);
            SyntaxTree syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            Compilation compilation = new(syntaxTree);
            EvaluationResult result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            string[] expectedErrors = AnnotatedText.UnindentLines(diagnosticText);

            if (annotatedText.Spans.Length != expectedErrors.Length)
                throw new Exception("ERROR: Must mark as many spans as there are expected errors");

            Assert.Equal(expectedErrors.Length, result.Errors.Length);

            for (int i = 0; i < expectedErrors.Length; i++)
            {
                string expectedMessage = expectedErrors[i];
                string actualMessage = result.Errors[i].Message;
                Assert.Equal(expectedMessage, actualMessage);

                Yearl.CodeAnalysis.Text.TextSpan expectedSpan = annotatedText.Spans[i];
                Yearl.CodeAnalysis.Text.TextSpan actualSpan = result.Errors[i].Span;
                Assert.Equal(expectedSpan, actualSpan);
            }
        }
    }
}