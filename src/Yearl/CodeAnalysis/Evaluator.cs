﻿using System.Diagnostics;
using Yearl.CodeAnalysis.Binding;
using Yearl.CodeAnalysis.Symbols;

namespace Yearl.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundProgram _program;
        private readonly Dictionary<VariableSymbol, object> _globals;
        private readonly Dictionary<FunctionSymbol, BoundBlockStatement> _functions = [];
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new();

        private object? _lastValue;

        public Evaluator(BoundProgram program, Dictionary<VariableSymbol, object> variables)
        {
            _program = program;
            _globals = variables;
            _locals.Push([]);

            var current = program;
            while (current != null)
            {
                foreach (var kv in current.Functions)
                {
                    var function = kv.Key;
                    var body = kv.Value;
                    _functions.Add(function, body);
                }

                current = current.Previous;
            }
        }


        public object? Evaluate()
        {
            var function = _program.MainFunction ?? _program.ScriptFunction;
            if (function == null)
                return null;

            var body = _functions[function];
            return EvaluateStatement(body);
        }

        private object? EvaluateStatement(BoundBlockStatement body)
        {
            Dictionary<BoundLabel, int> labelToIndex = [];

            for (int i = 0; i < body.Statements.Length; i++)
                if (body.Statements[i] is BoundLabelStatement l)
                    labelToIndex.Add(l.Label, i + 1);

            int index = 0;

            while (index < body.Statements.Length)
            {
                var s = body.Statements[index];
                switch (s.Kind)
                {
                    case BoundNodeKind.NopStatement:
                        index++;
                        break;
                    case BoundNodeKind.VariableDeclarationStatement:
                        EvaluateVariableDeclarationStatement((BoundVariableDeclarationStatement)s);
                        index++;
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement)s);
                        index++;
                        break;
                    case BoundNodeKind.GotoStatement:
                        var gs = (BoundGotoStatement)s;
                        index = labelToIndex[gs.Label];
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        var cgs = (BoundConditionalGotoStatement)s;
                        bool condition = (bool)EvaluateExpression(cgs.Condition)!;
                        if (condition == cgs.JumpIfTrue)
                            index = labelToIndex[cgs.Label];
                        else
                            index++;
                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;
                    case BoundNodeKind.ReturnStatement:
                        var rs = (BoundReturnStatement)s;
                        _lastValue = rs.Expression == null ? null : EvaluateExpression(rs.Expression);
                        return _lastValue;
                    default:
                        throw new Exception($"Unexpected node {s.Kind}");
                }
            }
            return _lastValue;
        }

        private void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement node)
        {
            object? value = EvaluateExpression(node.Initializer);
            Debug.Assert(value != null);

            _lastValue = value;
            Assign(node.Variable, value);
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node) => _lastValue = EvaluateExpression(node.Expression);


        private object? EvaluateExpression(BoundExpression node)
        {
            if (node.ConstantValue != null)
                return EvaluateConstantExpression(node);

            return node switch
            {
                BoundUnaryExpression n => EvaluateUnaryExpression(n),
                BoundBinaryExpression n => EvaluateBinaryExpression(n),
                BoundVariableExpression n => EvaluateVariableExpression(n),
                BoundVariableAssignmentExpression n => EvaluateVariableAssignmentExpression(n),
                BoundCallExpression n => EvaluateCallExpression(n),
                BoundConversionExpression n => EvaluateConversionExpression(n),
                _ => throw new Exception($"Unexpected node {node.Kind}"),
            };
        }

        private static object EvaluateConstantExpression(BoundExpression n)
        {
            Debug.Assert(n.ConstantValue != null);

            return n.ConstantValue.Value;
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            object? expression = EvaluateExpression(u.Expression);

            Debug.Assert(expression != null);

            return u.Operator.Kind switch
            {
                BoundUnaryOperatorKind.Identity => (double)expression,
                BoundUnaryOperatorKind.Negation => -(double)expression,
                BoundUnaryOperatorKind.LogicalNegation => !(bool)expression,
                _ => throw new Exception($"Unexpected unary operator {u.Operator}"),
            };
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            object? left = EvaluateExpression(b.Left);
            object? right = EvaluateExpression(b.Right);

            Debug.Assert(left != null && right != null);

            if (b.Operator.Kind == BoundBinaryOperatorKind.Division && (double)right == 0)
                throw new Exception("add errors to evaluator - division by 0");

            return b.Operator.Kind switch
            {
                BoundBinaryOperatorKind.Addition => b.Left.Type == TypeSymbol.Number ? (double)left + (double)right : (string)left + (string)right,
                BoundBinaryOperatorKind.Subtraction => (double)left - (double)right,
                BoundBinaryOperatorKind.Multiplication => (double)left * (double)right,
                BoundBinaryOperatorKind.Division => (double)left / (double)right,
                BoundBinaryOperatorKind.Power => Math.Pow((double)left, (double)right),
                BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
                BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
                BoundBinaryOperatorKind.Equals => Equals(left, right),
                BoundBinaryOperatorKind.NotEquals => !Equals(left, right),
                BoundBinaryOperatorKind.GreaterThan => (double)left > (double)right,
                BoundBinaryOperatorKind.GreaterThanEquals => (double)left >= (double)right,
                BoundBinaryOperatorKind.LessThan => (double)left < (double)right,
                BoundBinaryOperatorKind.LessThanEquals => (double)left <= (double)right,
                _ => throw new Exception($"Unexpected binary operator {b.Operator}"),
            };
            ;
        }

        private object EvaluateVariableExpression(BoundVariableExpression v)
        {
            if (v.Variable.Kind == SymbolKind.GlobalVariable)
            {
                return _globals[v.Variable];
            }
            else
            {
                var locals = _locals.Peek();
                return locals[v.Variable];
            }
        }

        private object EvaluateVariableAssignmentExpression(BoundVariableAssignmentExpression a)
        {
            object? value = EvaluateExpression(a.Expression);
            Debug.Assert(value != null);

            Assign(a.Variable, value);
            return value;
        }

        private object? EvaluateCallExpression(BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Input)
            {
                return Console.ReadLine();
            }
            else if (node.Function == BuiltinFunctions.Print)
            {
                object? value = EvaluateExpression(node.Arguments[0]);
                Console.WriteLine(value);
                return null;
            }
            else if (node.Function == BuiltinFunctions.Floor)
            {
                double number = (double)EvaluateExpression(node.Arguments[0])!;
                number = Math.Floor(number);
                return number;
            }
            else
            {
                Dictionary<VariableSymbol, object> locals = [];
                for (int i = 0; i < node.Arguments.Length; i++)
                {
                    var parameter = node.Function.Parameters[i];
                    object? value = EvaluateExpression(node.Arguments[i]);
                    Debug.Assert(value != null);
                    locals.Add(parameter, value);
                }

                _locals.Push(locals);

                var statement = _functions[node.Function];
                object? result = EvaluateStatement(statement);

                _locals.Pop();

                return result;
            }
        }

        private object? EvaluateConversionExpression(BoundConversionExpression node)
        {
            object? value = EvaluateExpression(node.Expression);
            if (node.Type == TypeSymbol.Dynamic)
                return value;
            else if (node.Type == TypeSymbol.Bool)
                return Convert.ToBoolean(value);
            else if (node.Type == TypeSymbol.Number)
                return Convert.ToDouble(value);
            else if (node.Type == TypeSymbol.String)
                return Convert.ToString(value);
            else
                throw new Exception($"Unexpected type {node.Type}");
        }


        private void Assign(VariableSymbol variable, object value)
        {
            if (variable.Kind == SymbolKind.GlobalVariable)
            {
                _globals[variable] = value;
            }
            else
            {
                var locals = _locals.Peek();
                locals[variable] = value;
            }
        }
    }
}