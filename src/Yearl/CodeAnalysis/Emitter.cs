﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Collections.Immutable;
using System.Text;
using Yearl.CodeAnalysis.Binding;
using Yearl.CodeAnalysis.Errors;
using Yearl.CodeAnalysis.Symbols;
using Yearl.CodeAnalysis.Syntax;

namespace Yearl.CodeAnalysis
{
    internal sealed class Emitter
    {
        private readonly ErrorHandler _errors = new();

        private readonly Dictionary<TypeSymbol, TypeReference> _knownTypes;
        private readonly MethodReference _objectEqualsReference;
        private readonly MethodReference _consoleReadLineReference;
        private readonly MethodReference _consoleWriteLineReference;
        private readonly MethodReference _mathFloorReference;
        private readonly MethodReference _stringConcat2Reference;
        private readonly MethodReference _stringConcat3Reference;
        private readonly MethodReference _stringConcat4Reference;
        private readonly MethodReference _stringConcatArrayReference;
        private readonly MethodReference _convertToBooleanReference;
        private readonly MethodReference _convertToDoubleReference;
        private readonly MethodReference _convertToStringReference;
        private readonly AssemblyDefinition _assemblyDefinition;
        private readonly Dictionary<FunctionSymbol, MethodDefinition> _methods = [];
        private readonly Dictionary<VariableSymbol, VariableDefinition> _locals = [];
        private readonly Dictionary<BoundLabel, int> _labels = [];
        private readonly List<(int InstructionIndex, BoundLabel Target)> _fixups = [];

        private TypeDefinition _typeDefinition;


        private Emitter(string moduleName, string[] references)
        {
            var assemblies = new List<AssemblyDefinition>();

            foreach (string reference in references)
            {
                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly(reference);
                    assemblies.Add(assembly);
                }
                catch (BadImageFormatException)
                {
                    _errors.ReportInvalidReference(reference);
                }
            }

            var builtInTypes = new List<(TypeSymbol type, string MetadataName)>()
            {
                (TypeSymbol.Dynamic, "System.Object"),
                (TypeSymbol.Bool, "System.Boolean"),
                (TypeSymbol.Number, "System.Double"),
                (TypeSymbol.String, "System.String"),
                (TypeSymbol.Void, "System.Void"),
            };

            var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
            _assemblyDefinition = AssemblyDefinition.CreateAssembly(assemblyName, moduleName, ModuleKind.Console);
            _knownTypes = [];

            foreach (var (typeSymbol, metadataName) in builtInTypes)
            {
                var typeReference = ResolveType(typeSymbol.Name, metadataName);
                _knownTypes.Add(typeSymbol, typeReference);
            }

            TypeReference ResolveType(string yearlName, string metadataName)
            {
                var foundTypes = assemblies.SelectMany(a => a.Modules)
                                           .SelectMany(m => m.Types)
                                           .Where(t => t.FullName == metadataName)
                                           .ToArray();
                if (foundTypes.Length == 1)
                {
                    var typeReference = _assemblyDefinition.MainModule.ImportReference(foundTypes[0]);
                    return typeReference;
                }
                else if (foundTypes.Length == 0)
                {
                    _errors.ReportRequiredTypeNotFound(yearlName, metadataName);
                }
                else
                {
                    _errors.ReportRequiredTypeAmbiguous(yearlName, metadataName, foundTypes);
                }

                return null!;
            }

            MethodReference ResolveMethod(string typeName, string methodName, string[] parameterTypeNames)
            {
                var foundTypes = assemblies.SelectMany(a => a.Modules)
                                           .SelectMany(m => m.Types)
                                           .Where(t => t.FullName == typeName)
                                           .ToArray();
                if (foundTypes.Length == 1)
                {
                    var foundType = foundTypes[0];
                    var methods = foundType.Methods.Where(m => m.Name == methodName);

                    foreach (var method in methods)
                    {
                        if (method.Parameters.Count != parameterTypeNames.Length)
                            continue;

                        bool allParametersMatch = true;

                        for (int i = 0; i < parameterTypeNames.Length; i++)
                        {
                            if (method.Parameters[i].ParameterType.FullName != parameterTypeNames[i])
                            {
                                allParametersMatch = false;
                                break;
                            }
                        }

                        if (!allParametersMatch)
                            continue;

                        return _assemblyDefinition.MainModule.ImportReference(method);
                    }

                    _errors.ReportRequiredMethodNotFound(typeName, methodName, parameterTypeNames);
                    return null!;
                }
                else if (foundTypes.Length == 0)
                {
                    _errors.ReportRequiredTypeNotFound(null, typeName);
                }
                else
                {
                    _errors.ReportRequiredTypeAmbiguous(null, typeName, foundTypes);
                }

                return null!;
            }

            _objectEqualsReference = ResolveMethod("System.Object", "Equals", ["System.Object", "System.Object"]);
            _consoleReadLineReference = ResolveMethod("System.Console", "ReadLine", []);
            _consoleWriteLineReference = ResolveMethod("System.Console", "WriteLine", ["System.Object"]);
            _mathFloorReference = ResolveMethod("System.Math", "Floor", ["System.Double"]);
            _stringConcat2Reference = ResolveMethod("System.String", "Concat", ["System.String", "System.String"]);
            _stringConcat3Reference = ResolveMethod("System.String", "Concat", ["System.String", "System.String", "System.String"]);
            _stringConcat4Reference = ResolveMethod("System.String", "Concat", ["System.String", "System.String", "System.String", "System.String"]);
            _stringConcatArrayReference = ResolveMethod("System.String", "Concat", ["System.String[]"]);
            _convertToBooleanReference = ResolveMethod("System.Convert", "ToBoolean", ["System.Object"]);
            _convertToDoubleReference = ResolveMethod("System.Convert", "ToDouble", ["System.Object"]);
            _convertToStringReference = ResolveMethod("System.Convert", "ToString", ["System.Object"]);

            var objectType = _knownTypes[TypeSymbol.Dynamic];
            if (objectType != null)
            {
                _typeDefinition = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed, objectType);
                _assemblyDefinition.MainModule.Types.Add(_typeDefinition);
            }
            else
            {
                _typeDefinition = null!;
            }
        }

        public static ImmutableArray<Error> Emit(BoundProgram program, string moduleName, string[] references, string outputPath)
        {
            if (program.Errors.Any())
                return program.Errors;

            var emitter = new Emitter(moduleName, references);
            return emitter.Emit(program, outputPath);
        }

        public ImmutableArray<Error> Emit(BoundProgram program, string outputPath)
        {
            if (_errors.Any())
                return [.. _errors];

            foreach (var functionWithBody in program.Functions)
                EmitFunctionDeclaration(functionWithBody.Key);

            foreach (var functionWithBody in program.Functions)
                EmitFunctionBody(functionWithBody.Key, functionWithBody.Value);

            if (program.MainFunction != null)
                _assemblyDefinition.EntryPoint = _methods[program.MainFunction];

            _assemblyDefinition.Write(outputPath);

            return [.. _errors];
        }

        private void EmitFunctionDeclaration(FunctionSymbol function)
        {
            var functionType = _knownTypes[function.Type];
            var method = new MethodDefinition(function.Name, MethodAttributes.Static | MethodAttributes.Private, functionType);

            foreach (var parameter in function.Parameters)
            {
                var parameterType = _knownTypes[parameter.Type];
                var parameterAttributes = ParameterAttributes.None;
                var parameterDefinition = new ParameterDefinition(parameter.Name, parameterAttributes, parameterType);
                method.Parameters.Add(parameterDefinition);
            }

            _typeDefinition.Methods.Add(method);
            _methods.Add(function, method);
        }

        private void EmitFunctionBody(FunctionSymbol function, BoundBlockStatement body)
        {
            var method = _methods[function];
            _locals.Clear();
            _labels.Clear();
            _fixups.Clear();

            var ilProcessor = method.Body.GetILProcessor();

            foreach (var statement in body.Statements)
                EmitStatement(ilProcessor, statement);

            foreach (var (InstructionIndex, Target) in _fixups)
            {
                var targetLabel = Target;
                int targetInstructionIndex = _labels[targetLabel];
                var targetInstruction = ilProcessor.Body.Instructions[targetInstructionIndex];
                var instructionToFixup = ilProcessor.Body.Instructions[InstructionIndex];
                instructionToFixup.Operand = targetInstruction;
            }

            method.Body.OptimizeMacros();
            method.Body.Optimize();
        }

        private void EmitStatement(ILProcessor ilProcessor, BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.NopStatement:
                    EmitNopStatement(ilProcessor, (BoundNopStatement)node);
                    break;
                case BoundNodeKind.VariableDeclarationStatement:
                    EmitVariableDeclaration(ilProcessor, (BoundVariableDeclarationStatement)node);
                    break;
                case BoundNodeKind.LabelStatement:
                    EmitLabelStatement(ilProcessor, (BoundLabelStatement)node);
                    break;
                case BoundNodeKind.GotoStatement:
                    EmitGotoStatement(ilProcessor, (BoundGotoStatement)node);
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    EmitConditionalGotoStatement(ilProcessor, (BoundConditionalGotoStatement)node);
                    break;
                case BoundNodeKind.ReturnStatement:
                    EmitReturnStatement(ilProcessor, (BoundReturnStatement)node);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EmitExpressionStatement(ilProcessor, (BoundExpressionStatement)node);
                    break;
                default:
                    throw new Exception($"Unexpected node kind {node.Kind}");
            }
        }

        private void EmitNopStatement(ILProcessor ilProcessor, BoundNopStatement node) => ilProcessor.Emit(OpCodes.Nop);

        private void EmitVariableDeclaration(ILProcessor ilProcessor, BoundVariableDeclarationStatement node)
        {
            var typeReference = _knownTypes[node.Variable.Type];
            var variableDefinition = new VariableDefinition(typeReference);
            _locals.Add(node.Variable, variableDefinition);
            ilProcessor.Body.Variables.Add(variableDefinition);

            EmitExpression(ilProcessor, node.Initializer);
            ilProcessor.Emit(OpCodes.Stloc, variableDefinition);
        }

        private void EmitLabelStatement(ILProcessor ilProcessor, BoundLabelStatement node) => _labels.Add(node.Label, ilProcessor.Body.Instructions.Count);

        private void EmitGotoStatement(ILProcessor ilProcessor, BoundGotoStatement node)
        {
            _fixups.Add((ilProcessor.Body.Instructions.Count, node.Label));
            ilProcessor.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
        }

        private void EmitConditionalGotoStatement(ILProcessor ilProcessor, BoundConditionalGotoStatement node)
        {
            EmitExpression(ilProcessor, node.Condition);

            var opCode = node.JumpIfTrue ? OpCodes.Brtrue : OpCodes.Brfalse;
            _fixups.Add((ilProcessor.Body.Instructions.Count, node.Label));
            ilProcessor.Emit(opCode, Instruction.Create(OpCodes.Nop));
        }

        private void EmitReturnStatement(ILProcessor ilProcessor, BoundReturnStatement node)
        {
            if (node.Expression != null)
                EmitExpression(ilProcessor, node.Expression);

            ilProcessor.Emit(OpCodes.Ret);
        }

        private void EmitExpressionStatement(ILProcessor ilProcessor, BoundExpressionStatement node)
        {
            EmitExpression(ilProcessor, node.Expression);

            if (node.Expression.Type != TypeSymbol.Void)
                ilProcessor.Emit(OpCodes.Pop);
        }

        private void EmitExpression(ILProcessor ilProcessor, BoundExpression node)
        {
            if (node.ConstantValue != null)
            {
                EmitConstantExpression(ilProcessor, node);
                return;
            }

            switch (node.Kind)
            {
                case BoundNodeKind.VariableExpression:
                    EmitVariableExpression(ilProcessor, (BoundVariableExpression)node);
                    break;
                case BoundNodeKind.VariableAssignmentExpression:
                    EmitAssignmentExpression(ilProcessor, (BoundVariableAssignmentExpression)node);
                    break;
                case BoundNodeKind.UnaryExpression:
                    EmitUnaryExpression(ilProcessor, (BoundUnaryExpression)node);
                    break;
                case BoundNodeKind.BinaryExpression:
                    EmitBinaryExpression(ilProcessor, (BoundBinaryExpression)node);
                    break;
                case BoundNodeKind.CallExpression:
                    EmitCallExpression(ilProcessor, (BoundCallExpression)node);
                    break;
                case BoundNodeKind.ConversionExpression:
                    EmitConversionExpression(ilProcessor, (BoundConversionExpression)node);
                    break;
                default:
                    throw new Exception($"Unexpected node kind {node.Kind}");
            }
        }

        private void EmitConstantExpression(ILProcessor ilProcessor, BoundExpression node)
        {
            if (node.Type == TypeSymbol.Bool)
            {
                bool value = (bool)node.ConstantValue!.Value;
                var instruction = value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
                ilProcessor.Emit(instruction);
            }
            else if (node.Type == TypeSymbol.Number)
            {
                double value = (double)node.ConstantValue!.Value;
                ilProcessor.Emit(OpCodes.Ldc_R8, value);
            }
            else if (node.Type == TypeSymbol.String)
            {
                string value = (string)node.ConstantValue!.Value;
                ilProcessor.Emit(OpCodes.Ldstr, value);
            }
            else
            {
                throw new Exception($"Unexpected constant expression type: {node.Type}");
            }
        }

        private void EmitVariableExpression(ILProcessor ilProcessor, BoundVariableExpression node)
        {
            if (node.Variable is ParameterSymbol parameter)
            {
                ilProcessor.Emit(OpCodes.Ldarg, parameter.Ordinal);
            }
            else
            {
                var variableDefinition = _locals[node.Variable];
                ilProcessor.Emit(OpCodes.Ldloc, variableDefinition);
            }
        }

        private void EmitAssignmentExpression(ILProcessor ilProcessor, BoundVariableAssignmentExpression node)
        {
            var variableDefinition = _locals[node.Variable];
            EmitExpression(ilProcessor, node.Expression);
            ilProcessor.Emit(OpCodes.Dup);
            ilProcessor.Emit(OpCodes.Stloc, variableDefinition);
        }

        private void EmitUnaryExpression(ILProcessor ilProcessor, BoundUnaryExpression node)
        {
            EmitExpression(ilProcessor, node.Expression);

            if (node.Operator.Kind == BoundUnaryOperatorKind.Identity)
            {
                // Done
            }
            else if (node.Operator.Kind == BoundUnaryOperatorKind.LogicalNegation)
            {
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
            }
            else if (node.Operator.Kind == BoundUnaryOperatorKind.Negation)
            {
                ilProcessor.Emit(OpCodes.Neg);
            }
            else
            {
                throw new Exception($"Unexpected unary operator {SyntaxFacts.GetText(node.Operator.SyntaxKind)}({node.Expression.Type})");
            }
        }

        private void EmitBinaryExpression(ILProcessor ilProcessor, BoundBinaryExpression node)
        {

            // +(string, string)

            if (node.Operator.Kind == BoundBinaryOperatorKind.Addition)
            {
                if (node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
                {
                    EmitStringConcatExpression(ilProcessor, node);
                    return;
                }
            }

            EmitExpression(ilProcessor, node.Left);
            EmitExpression(ilProcessor, node.Right);

            // ==(any, any)
            // ==(string, string)

            if (node.Operator.Kind == BoundBinaryOperatorKind.Equals)
            {
                if (node.Left.Type == TypeSymbol.Dynamic && node.Right.Type == TypeSymbol.Dynamic ||
                    node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
                {
                    ilProcessor.Emit(OpCodes.Call, _objectEqualsReference);
                    return;
                }
            }

            // !=(any, any)
            // !=(string, string)

            if (node.Operator.Kind == BoundBinaryOperatorKind.NotEquals)
            {
                if (node.Left.Type == TypeSymbol.Dynamic && node.Right.Type == TypeSymbol.Dynamic ||
                    node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
                {
                    ilProcessor.Emit(OpCodes.Call, _objectEqualsReference);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    return;
                }
            }

            switch (node.Operator.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    ilProcessor.Emit(OpCodes.Add);
                    break;
                case BoundBinaryOperatorKind.Subtraction:
                    ilProcessor.Emit(OpCodes.Sub);
                    break;
                case BoundBinaryOperatorKind.Multiplication:
                    ilProcessor.Emit(OpCodes.Mul);
                    break;
                case BoundBinaryOperatorKind.Division:
                    ilProcessor.Emit(OpCodes.Div);
                    break;
                // TODO: Implement short-circuit evaluation
                case BoundBinaryOperatorKind.LogicalAnd:
                    ilProcessor.Emit(OpCodes.And);
                    break;
                // TODO: Implement short-circuit evaluation
                case BoundBinaryOperatorKind.LogicalOr:
                    ilProcessor.Emit(OpCodes.Or);
                    break;
                case BoundBinaryOperatorKind.Equals:
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperatorKind.NotEquals:
                    ilProcessor.Emit(OpCodes.Ceq);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperatorKind.LessThan:
                    ilProcessor.Emit(OpCodes.Clt);
                    break;
                case BoundBinaryOperatorKind.LessThanEquals:
                    ilProcessor.Emit(OpCodes.Cgt);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperatorKind.GreaterThan:
                    ilProcessor.Emit(OpCodes.Cgt);
                    break;
                case BoundBinaryOperatorKind.GreaterThanEquals:
                    ilProcessor.Emit(OpCodes.Clt);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                default:
                    throw new Exception($"Unexpected binary operator {SyntaxFacts.GetText(node.Operator.SyntaxKind)}({node.Left.Type}, {node.Right.Type})");
            }
        }

        private void EmitCallExpression(ILProcessor ilProcessor, BoundCallExpression node)
        {
            foreach (var argument in node.Arguments)
                EmitExpression(ilProcessor, argument);

            if (node.Function == BuiltinFunctions.Input)
            {
                ilProcessor.Emit(OpCodes.Call, _consoleReadLineReference);
            }
            else if (node.Function == BuiltinFunctions.Print)
            {
                ilProcessor.Emit(OpCodes.Call, _consoleWriteLineReference);
            }
            else if (node.Function == BuiltinFunctions.Floor)
            {
                ilProcessor.Emit(OpCodes.Call, _mathFloorReference);
            }
            else
            {
                var methodDefinition = _methods[node.Function];
                ilProcessor.Emit(OpCodes.Call, methodDefinition);
            }
        }

        private void EmitConversionExpression(ILProcessor ilProcessor, BoundConversionExpression node)
        {
            EmitExpression(ilProcessor, node.Expression);
            bool needsBoxing = node.Expression.Type == TypeSymbol.Bool ||
                              node.Expression.Type == TypeSymbol.Number;
            if (needsBoxing)
                ilProcessor.Emit(OpCodes.Box, _knownTypes[node.Expression.Type]);

            if (node.Type == TypeSymbol.Dynamic)
            {
                // Done
            }
            else if (node.Type == TypeSymbol.Bool)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToBooleanReference);
            }
            else if (node.Type == TypeSymbol.Number)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToDoubleReference);
            }
            else if (node.Type == TypeSymbol.String)
            {
                ilProcessor.Emit(OpCodes.Call, _convertToStringReference);
            }
            else
            {
                throw new Exception($"Unexpected conversion from {node.Expression.Type} to {node.Type}");
            }
        }

        private void EmitStringConcatExpression(ILProcessor ilProcessor, BoundBinaryExpression node)
        {
            // Flatten the expression tree to a sequence of nodes to concatenate, then fold consecutive constants in that sequence.
            // This approach enables constant folding of non-sibling nodes, which cannot be done in the ConstantFolding class as it would require changing the tree.
            // Example: folding b and c in ((a + b) + c) if they are constant.

            var nodes = FoldConstants(Flatten(node)).ToList();

            switch (nodes.Count)
            {
                case 0:
                    ilProcessor.Emit(OpCodes.Ldstr, string.Empty);
                    break;

                case 1:
                    EmitExpression(ilProcessor, nodes[0]);
                    break;

                case 2:
                    EmitExpression(ilProcessor, nodes[0]);
                    EmitExpression(ilProcessor, nodes[1]);
                    ilProcessor.Emit(OpCodes.Call, _stringConcat2Reference);
                    break;

                case 3:
                    EmitExpression(ilProcessor, nodes[0]);
                    EmitExpression(ilProcessor, nodes[1]);
                    EmitExpression(ilProcessor, nodes[2]);
                    ilProcessor.Emit(OpCodes.Call, _stringConcat3Reference);
                    break;

                case 4:
                    EmitExpression(ilProcessor, nodes[0]);
                    EmitExpression(ilProcessor, nodes[1]);
                    EmitExpression(ilProcessor, nodes[2]);
                    EmitExpression(ilProcessor, nodes[3]);
                    ilProcessor.Emit(OpCodes.Call, _stringConcat4Reference);
                    break;

                default:
                    ilProcessor.Emit(OpCodes.Ldc_I4, nodes.Count);
                    ilProcessor.Emit(OpCodes.Newarr, _knownTypes[TypeSymbol.String]);

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        ilProcessor.Emit(OpCodes.Dup);
                        ilProcessor.Emit(OpCodes.Ldc_I4, i);
                        EmitExpression(ilProcessor, nodes[i]);
                        ilProcessor.Emit(OpCodes.Stelem_Ref);
                    }

                    ilProcessor.Emit(OpCodes.Call, _stringConcatArrayReference);
                    break;
            }

            // (a + b) + (c + d) --> [a, b, c, d]
            static IEnumerable<BoundExpression> Flatten(BoundExpression node)
            {
                if (node is BoundBinaryExpression binaryExpression &&
                    binaryExpression.Operator.Kind == BoundBinaryOperatorKind.Addition &&
                    binaryExpression.Left.Type == TypeSymbol.String &&
                    binaryExpression.Right.Type == TypeSymbol.String)
                {
                    foreach (var result in Flatten(binaryExpression.Left))
                        yield return result;

                    foreach (var result in Flatten(binaryExpression.Right))
                        yield return result;
                }
                else
                {
                    if (node.Type != TypeSymbol.String)
                        throw new Exception($"Unexpected node type in string concatenation: {node.Type}");

                    yield return node;
                }
            }
            // [a, "foo", "bar", b, ""] --> [a, "foobar", b]
            static IEnumerable<BoundExpression> FoldConstants(IEnumerable<BoundExpression> nodes)
            {
                StringBuilder? sb = null;

                foreach (var node in nodes)
                {
                    if (node.ConstantValue != null)
                    {
                        string stringValue = (string)node.ConstantValue.Value;

                        if (string.IsNullOrEmpty(stringValue))
                            continue;

                        sb ??= new StringBuilder();
                        sb.Append(stringValue);
                    }
                    else
                    {
                        if (sb?.Length > 0)
                        {
                            yield return new BoundLiteralExpression(sb.ToString());
                            sb.Clear();
                        }

                        yield return node;
                    }
                }

                if (sb?.Length > 0)
                    yield return new BoundLiteralExpression(sb.ToString());
            }
        }
    }
}

