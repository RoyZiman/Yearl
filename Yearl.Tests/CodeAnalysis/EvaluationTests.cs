using Yearl.CodeAnalysis;
using Yearl.CodeAnalysis.Symbols;
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
        [InlineData("0 < 1", true)]
        [InlineData("1 < 0", false)]
        [InlineData("0 <= 1", true)]
        [InlineData("0 <= 0", true)]
        [InlineData("1 <= 0", false)]
        [InlineData("0 > 1", false)]
        [InlineData("1 > 0", true)]
        [InlineData("0 >= 1", false)]
        [InlineData("0 >= 0", true)]
        [InlineData("1 >= 0", true)]
        [InlineData("12 == 3", false)]
        [InlineData("3 == 3", true)]
        [InlineData("12 != 3", true)]
        [InlineData("3 != 3", false)]
        [InlineData("False == False", true)]
        [InlineData("True == False", false)]
        [InlineData("False != False", false)]
        [InlineData("True != False", true)]
        [InlineData("False && False", false)]
        [InlineData("True && False", false)]
        [InlineData("False && True", false)]
        [InlineData("True && True", true)]
        [InlineData("False || False", false)]
        [InlineData("True || False", true)]
        [InlineData("False || True", true)]
        [InlineData("True || True", true)]
        [InlineData("True", true)]
        [InlineData("False", false)]
        [InlineData("!True", false)]
        [InlineData("!False", true)]
        [InlineData("{ var a = 0 (a = 10) * a }", 100.0)]
        [InlineData("{ var a = 0 if a == 0 a = 10 a }", 10.0)]
        [InlineData("{ var a = 0 if a == 4 a = 10 a }", 0.0)]
        [InlineData("{ var a = 0 if a == 0 a = 10 else a = 5 a }", 10.0)]
        [InlineData("{ var a = 0 if a == 4 a = 10 else a = 5 a }", 5.0)]
        [InlineData("{ var result = 0 for i from 1 to 10 { result = result + i } result }", 55.0)]
        [InlineData("{ var result = 0 for i from 0 to -10 { result = result + i } result }", -55.0)]
        [InlineData("{ var result = 0 for i from 0 to 10 step 2 { result = result + i } result }", 30.0)]
        [InlineData("{ var i = 10 var result = 0 while i > 0 { result = result + i i = i - 1} result }", 55.0)]
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

            string errors = @"
                'x' is already declared.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_NameExpression_Reports_Undefined()
        {
            string text = @"[x] * 10";

            string errors = @"
                Variable 'x' doesn't exist.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_VariableAssignment_Reports_Undefined()
        {
            string text = @"[x] = 10";

            string errors = @"
                Variable 'x' doesn't exist.
            ";

            AssertErrors(text, errors);
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

            string errors = @"
                Variable 'x' is read-only and cannot be assigned to.
            ";

            AssertErrors(text, errors);
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

            string errors = @"
                Cannot convert type 'bool' to 'number'.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_BlockStatement_NoInfiniteLoop()
        {
            string text = @"
                {
                [)][]
            ";

            string errors = @"
                Unexpected token <RightParenthesisToken>, expected <IdentifierToken>.
                Unexpected token <EndOfFileToken>, expected <RightCurlyBraceToken>.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_NoInfiniteLoop()
        {
            string text = @"
                print(""Hi""[[=]][)]
            ";

            string diagnostics = @"
                Unexpected token <EqualsToken>, expected <RightParenthesisToken>.
                Unexpected token <EqualsToken>, expected <IdentifierToken>.
                Unexpected token <RightParenthesisToken>, expected <IdentifierToken>.
            ";

            AssertErrors(text, diagnostics);
        }

        [Fact]
        public void Evaluator_FunctionParameters_NoInfiniteLoop()
        {
            string text = @"
                func hi(name: string[[[=]]][)]
                {
                    print(""Hi "" + name + ""!"" )
                }[]
            ";

            string diagnostics = @"
                Unexpected token <EqualsToken>, expected <RightParenthesisToken>.
                Unexpected token <EqualsToken>, expected <LeftCurlyBraceToken>.
                Unexpected token <EqualsToken>, expected <IdentifierToken>.
                Unexpected token <RightParenthesisToken>, expected <IdentifierToken>.
                Unexpected token <EndOfFileToken>, expected <RightCurlyBraceToken>.
            ";

            AssertErrors(text, diagnostics);
        }

        [Fact]
        public void Evaluator_NameExpression_Reports_NoErrorForInsertedToken()
        {
            string text = @"1 + []";

            string errors = @"
                Unexpected token <EndOfFileToken>, expected <IdentifierToken>.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_IfStatement_Reports_CannotConvert()
        {
            string text = @"
                {
                    var x = 0
                    if [10]
                        x = 10
                }
            ";

            string errors = @"
                Cannot convert type 'number' to 'bool'.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_ForStatement_Reports_CannotConvert_FirstBound()
        {
            string text = @"
                {
                    var result = 0
                    for i from [False] to 10
                        result = result + i
                }
            ";

            string errors = @"
                Cannot convert type 'bool' to 'number'.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_ForStatement_Reports_CannotConvert_SecondBound()
        {
            string text = @"
                {
                    var result = 0
                    for i from 1 to [True]
                        result = result + i
                }
            ";

            string errors = @"
                Cannot convert type 'bool' to 'number'.
            ";

            AssertErrors(text, errors);
        }
        [Fact]
        public void Evaluator_ForStatement_Reports_CannotConvert_StepExpression()
        {
            string text = @"
                {
                    var result = 0
                    for i from 1 to 10 step [True]
                        result = result + i
                }
            ";

            string errors = @"
                Cannot convert type 'bool' to 'number'.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_WhileStatement_Reports_CannotConvert()
        {
            string text = @"
                {
                    var x = 0
                    while [10]
                        x = 10
                }
            ";

            string errors = @"
                Cannot convert type 'number' to 'bool'.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_UnaryExpression_Reports_Undefined()
        {
            string text = @"[+]True";

            string errors = @"
                Unary operator '+' is not defined for type 'bool'.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_BinaryExpression_Reports_Undefined()
        {
            string text = @"10 [*] False";

            string errors = @"
                Binary operator '*' is not defined for types 'number' and 'bool'.
            ";

            AssertErrors(text, errors);
        }

        private static void AssertValue(string text, object expectedValue)
        {
            SyntaxTree syntaxTree = SyntaxTree.Parse(text);
            Compilation compilation = new(syntaxTree);
            Dictionary<VariableSymbol, object> variables = [];
            EvaluationResult result = compilation.Evaluate(variables);

            Assert.Empty(result.Errors);
            Assert.Equal(expectedValue, result.Value);
        }

        private static void AssertErrors(string text, string errorText)
        {
            AnnotatedText annotatedText = AnnotatedText.Parse(text);
            SyntaxTree syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            Compilation compilation = new(syntaxTree);
            EvaluationResult result = compilation.Evaluate([]);

            string[] expectedErrors = AnnotatedText.UnindentLines(errorText);

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