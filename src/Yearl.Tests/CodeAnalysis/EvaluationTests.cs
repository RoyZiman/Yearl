﻿using Yearl.CodeAnalysis;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;
using Yearl.Tests.CodeAnalysis.Text;

namespace Yearl.Tests.CodeAnalysis
{
    public class EvaluationTests
    {
        [Theory]
        [InlineData("1", 1d)]
        [InlineData("+1", 1d)]
        [InlineData("-1", -1d)]
        [InlineData("14 + 12", 26d)]
        [InlineData("12 - 3", 9d)]
        [InlineData("4 * 2", 8d)]
        [InlineData("9 / 3", 3d)]
        [InlineData("5 ^ 2", 25d)]
        [InlineData("(10)", 10d)]
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
        [InlineData("var a = 10 return(a)", 10d)]
        [InlineData("\"test\"", "test")]
        [InlineData("\"te\\\"st\"", "te\"st")]
        [InlineData("\"test\" == \"test\"", true)]
        [InlineData("\"test\" != \"test\"", false)]
        [InlineData("\"test\" == \"abc\"", false)]
        [InlineData("\"test\" != \"abc\"", true)]
        [InlineData("\"test\" + \"abc\"", "testabc")]
        [InlineData("String(True)", "True")]
        [InlineData("String(1)", "1")]
        [InlineData("Bool(\"true\")", true)]
        [InlineData("Number(\"1\")", 1.0)]
        [InlineData("floor(1)", 1.0)]
        [InlineData("floor(1.5)", 1.0)]
        [InlineData("{ var a : Dynamic = 0 var b : Dynamic = \"b\" return(a == b) }", false)]
        [InlineData("{ var a : Dynamic = 0 var b : Dynamic = \"b\" return(a != b) }", true)]
        [InlineData("{ var a : Dynamic = 0 var b : Dynamic = 0 return(a == b) }", true)]
        [InlineData("{ var a : Dynamic = 0 var b : Dynamic = 0 return(a != b) }", false)]
        [InlineData("{ var a = 10 return(a * a) }", 100d)]
        [InlineData("{ var a = 0 return((a = 10) * a) }", 100d)]
        [InlineData("{ var a = 0 if a == 0 a = 10 return(a) }", 10d)]
        [InlineData("{ var a = 0 if a == 4 a = 10 return(a) }", 0d)]
        [InlineData("{ var a = 0 if a == 0 a = 10 else a = 5 return(a) }", 10d)]
        [InlineData("{ var a = 0 if a == 4 a = 10 else a = 5 return(a) }", 5d)]
        [InlineData("{ var result = 0 for i from 1 to 10 { result = result + i } return(result) }", 55d)]
        [InlineData("{ var result = 0 for i from -1 to -10 { result = result + i } return(result) }", 0d)]
        [InlineData("{ var result = 0 for i from 0 to 10 step 2 { result = result + i } return(result) }", 30d)]
        [InlineData("{ var result = 0 for i from 0 to -10 step -1 { result = result + i } return(result) }", -55d)]
        [InlineData("{ var i = 10 var result = 0 while i > 0 { result = result + i i = i - 1} return(result) }", 55d)]
        [InlineData("{ var i = 0 while i < 5 { i = i + 1 if i == 5 continue } return(i) }", 5d)]
        [InlineData("{ var a = 1 a += (2 + 3) return(a) }", 6d)]
        [InlineData("{ var a = 1 a -= (2 + 3) return(a) }", -4d)]
        [InlineData("{ var a = 1 a *= (2 + 3) return(a) }", 5d)]
        [InlineData("{ var a = 1 a /= (1 + 3) return(a) }", 0.25)]
        [InlineData("{ var a = False a &= False return(a) }", false)]
        [InlineData("{ var a = True a &= False return(a) }", false)]
        [InlineData("{ var a = False a &= True return(a) }", false)]
        [InlineData("{ var a = True a &= True return(a) }", true)]
        [InlineData("{ var a = False a |= False return(a) }", false)]
        [InlineData("{ var a = True a |= False return(a) }", true)]
        [InlineData("{ var a = False a |= True return(a) }", true)]
        [InlineData("{ var a = True a |= True return(a) }", true)]
        [InlineData("{ var a = \"a\" a += \"bc\" return(a) }", "abc")]
        [InlineData("{ var a = 1 var b = 2 var c = 3 a += b += c return(a) }", 6d)]
        [InlineData("{ var a = 1 var b = 2 var c = 3 a += b += c return(b) }", 5d)]
        public void Evaluator_Computes_CorrectValues(string text, object expectedValue) => AssertValue(text, expectedValue);

        [Fact]
        public void Evaluator_CompoundExpression_Reports_Undefined()

        {
            var text = @"var x = 10
                         x [+=] False";

            var errors = @"
                Binary operator '+=' is not defined for types 'Number' and 'Bool'.
            ";

            AssertErrors(text, errors);
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
        public void Evaluator_CompoundExpression_Assignment_NonDefinedVariable_Reports_Undefined()
        {
            var text = @"[x] += 10";

            var errors = @"
                Variable 'x' doesn't exist.
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
        public void Evaluator_AssignmentExpression_Reports_NotAVariable()
        {
            string text = @"[print] = 42";

            string errors = @"
                'print' is not a variable.
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
        public void Evaluator_CompoundDeclarationExpression_Reports_CannotAssign()
        {
            var text = @"
                {
                    const x = 10
                    x [+=] 1
                }
            ";

            var errors = @"
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
                Cannot convert type 'Bool' to 'Number'.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_CallExpression_Reports_Undefined()
        {
            string text = @"[foo](42)";

            string errors = @"
                Function 'foo' doesn't exist.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_CallExpression_Reports_NotAFunction()
        {
            string text = @"
                {
                    const foo = 42
                    [foo](42)
                }
            ";

            string errors = @"
                'foo' is not a function.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_Void_Function_Should_Not_Return_Value()
        {
            string text = @"
                func test()
                {
                    return ([1])
                }
            ";

            string errors = @"
                Since the function 'test' does not return a value the return statement cannot contain an expression.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_Function_With_ReturnValue_Should_Not_Return_Void()
        {
            string text = @"
                func test(): Number
                {
                    [return]()
                }
            ";

            string errors = @"
                An expression of type 'Number' is expected.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_Not_All_Code_Paths_Return_Value()
        {
            string text = @"
                func [test](n: Number): Bool
                {
                    if (n > 10)
                       return (True)
                }
            ";

            string errors = @"
                Not all code paths return a value.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_Expression_Must_Have_Value()
        {
            string text = @"
                func test(n: Number)
                {
                    return()
                }
                const value = [test(100)]
            ";

            string errors = @"
                Expression must have a value.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_IfStatement_Reports_NotReachableCode_Warning()
        {
            string text = @"
                func test()
                {
                    const x = 4 * 3
                    if x > 12
                    {
                        [print](""x"")
                    }
                    else
                    {
                        print(""x"")
                    }
                }
            ";

            string errors = @"
                Unreachable code detected.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_ElseStatement_Reports_NotReachableCode_Warning()
        {
            string text = @"
                func test(): Number
                {
                    if True
                    {
                        return(1)
                    }
                    else
                    {
                        [return](0)
                    }
                }
            ";

            string errors = @"
                Unreachable code detected.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_WhileStatement_Reports_NotReachableCode_Warning()
        {
            string text = @"
                func test()
                {
                    while False
                    {
                        [continue]
                    }
                }
            ";

            string errors = @"
                Unreachable code detected.
            ";

            AssertErrors(text, errors);
        }

        [Theory]
        [InlineData("[break]", "break")]
        [InlineData("[continue]", "continue")]
        public void Evaluator_Invalid_Break_Or_Continue(string text, string keyword)
        {
            string errors = $@"
                The keyword '{keyword}' can only be used inside of loops.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_Script_Return()
        {
            string text = @"
                return()
            ";

            AssertValue(text, "");
        }

        [Fact]
        public void Evaluator_Parameter_Already_Declared()
        {
            string text = @"
                func sum(a: Number, b: Number, [a: Number]): Number
                {
                    return (a + b + c)
                }
            ";

            string errors = @"
                A parameter with the name 'a' already exists.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_Function_Must_Have_Name()
        {
            string text = @"
                func [(]a: Number, b: Number): Number
                {
                    return (a + b)
                }
            ";

            string errors = @"
                Unexpected token <LeftParenthesisToken>, expected <IdentifierToken>.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_Wrong_Argument_Type()
        {
            string text = @"
                func test(n: Number): Bool
                {
                    return (n > 10)
                }
                const testValue = ""string""
                test([testValue])
            ";

            string errors = @"
                Cannot convert type 'String' to 'Number'. An explicit conversion exists (are you missing a cast?)
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_Bad_Type()
        {
            string text = @"
                func test(n: [invalidtype])
                {
                }
            ";

            string errors = @"
                Type 'invalidtype' doesn't exist.
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
        public void Evaluator_InvokeFunctionArguments_Missing()
        {
            string text = @"
                print([)]
            ";

            string errors = @"
                Function 'print' requires 1 arguments but was given 0.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_Exceeding()
        {
            string text = @"
                print(""Hello""[, "" "", "" world!""])
            ";

            string errors = @"
                Function 'print' requires 1 arguments but was given 3.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_NoInfiniteLoop()
        {
            string text = @"
                print(""Hi""[[=]][)]
            ";

            string errors = @"
                Unexpected token <EqualsToken>, expected <RightParenthesisToken>.
                Unexpected token <EqualsToken>, expected <IdentifierToken>.
                Unexpected token <RightParenthesisToken>, expected <IdentifierToken>.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_FunctionParameters_NoInfiniteLoop()
        {
            string text = @"
                func hi(name: String[[[=]]][)]
                {
                    print(""Hi "" + name + ""!"" )
                }[]
            ";

            string errors = @"
                Unexpected token <EqualsToken>, expected <RightParenthesisToken>.
                Unexpected token <EqualsToken>, expected <LeftCurlyBraceToken>.
                Unexpected token <EqualsToken>, expected <IdentifierToken>.
                Unexpected token <RightParenthesisToken>, expected <IdentifierToken>.
                Unexpected token <EndOfFileToken>, expected <RightCurlyBraceToken>.
            ";

            AssertErrors(text, errors);
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
                Cannot convert type 'Number' to 'Bool'.
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
                Cannot convert type 'Bool' to 'Number'.
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
                Cannot convert type 'Bool' to 'Number'.
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
                Cannot convert type 'Bool' to 'Number'.
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
                Cannot convert type 'Number' to 'Bool'.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_UnaryExpression_Reports_Undefined()
        {
            string text = @"[+]True";

            string errors = @"
                Unary operator '+' is not defined for type 'Bool'.
            ";

            AssertErrors(text, errors);
        }

        [Fact]
        public void Evaluator_BinaryExpression_Reports_Undefined()
        {
            string text = @"10 [*] False";

            string errors = @"
                Binary operator '*' is not defined for types 'Number' and 'Bool'.
            ";

            AssertErrors(text, errors);
        }

        private static void AssertValue(string text, object expectedValue)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            var compilation = Compilation.CreateScript(null, syntaxTree);
            Dictionary<VariableSymbol, object> variables = [];
            var result = compilation.Evaluate(variables);

            Assert.Empty(result.Errors);
            Assert.Equal(expectedValue, result.Value);
        }

        private static void AssertErrors(string text, string errorText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            var compilation = Compilation.CreateScript(null, syntaxTree);
            var result = compilation.Evaluate([]);

            string[] expectedErrors = AnnotatedText.UnindentLines(errorText);

            if (annotatedText.Spans.Length != expectedErrors.Length)
                throw new Exception("ERROR: Must mark as many spans as there are expected errors");

            Assert.Equal(expectedErrors.Length, result.Errors.Length);

            for (int i = 0; i < expectedErrors.Length; i++)
            {
                string expectedMessage = expectedErrors[i];
                string actualMessage = result.Errors[i].Message;
                Assert.Equal(expectedMessage, actualMessage);

                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = result.Errors[i].Location.Span;
                Assert.Equal(expectedSpan, actualSpan);
            }
        }
    }
}