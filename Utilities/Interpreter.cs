using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using X_Programming_Language.Constants;
using X_Programming_Language.Errors;
using X_Programming_Language.Nodes;
using X_Programming_Language.Results;
using X_Programming_Language.Values;

namespace X_Programming_Language.Utilities
{
    public class Interpreter
    {
        public Interpreter()
        {
        }

        public RuntimeResult Visit(Node node, Context context)
        {
            if (node.ToString() == "[continue]") return VisitContinueNode((node as ContinueNode)!, context);
            else if (node.ToString() == "[break]") return VisitBreakNode((node as BreakNode)!, context);
            var methodName = $"Visit{node.GetType().Name}";
            MethodInfo? method = GetType().GetMethod(methodName);

            return method == null
                ? throw new Exception($"No visit method '{methodName}' defined")
                : (RuntimeResult) method.Invoke(this, new object?[] { node, context })!;
        }

        public RuntimeResult VisitNumberNode(NumberNode node, Context context)
        {
            if (node.Token!.Type == "int") return new RuntimeResult().Success(new IntValue((int)node.Token.Value!).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
            else return new RuntimeResult().Success(new FloatValue((float)node.Token.Value!).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }

        public RuntimeResult VisitStringNode(StringNode node, Context context)
        {
            return new RuntimeResult().Success(new StringValue((string)node.Token!.Value!).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }

        public RuntimeResult VisitListNode(ListNode node, Context context)
        {
            var res = new RuntimeResult();
            var elements = new List<Value>();
            var elementType = node.TypeToken != null
                ? context.SymbolTable!.Types.ContainsKey((string)node.TypeToken.Value!)
                    ? context.SymbolTable.GetType((string)node.TypeToken.Value!)
                    : null
                : null;

            int indexCount = 0;

            foreach (Node elementNode in node.ElementNodes)
            {
                var elementValue = res.Register(Visit(elementNode, context));
                if (res.ShouldReturn()) return res;
                if (node.TypeToken != null && (string)node.TypeToken.Value! != "object" && elementValue.Type != (string)node.TypeToken.Value!)
                    return res.Failure(new InvalidTypeError(
                        elementNode.PositionStart!,
                        elementNode.PositionEnd!,
                        $"Element at index '{indexCount}' has an invalid type '{elementValue.Type}' (expected type '{node.TypeToken.Value!}')",
                        context
                    ));
                elements.Add(elementValue);
                indexCount += 1;
            }



            return res.Success(new ListValue(elementType, elements).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }

        public RuntimeResult VisitBooleanNode(BooleanNode node, Context context)
        {
            if ((string)node.Token!.Value! == "true") return new RuntimeResult().Success(new BoolValue(true).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
            else return new RuntimeResult().Success(new BoolValue(false).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }

        public RuntimeResult VisitVariableAccessNode(VariableAccessNode node, Context context)
        {
            var res = new RuntimeResult();
            var varName = (string)node.VariableNameToken.Value!;
            var value = context.SymbolTable!.Get(varName);

            if (value == null) return res.Failure(new RuntimeError(
                node.PositionStart!,
                node.PositionEnd!,
                $"Ident '{varName}' is not defined",
                context
            ));

            value = value.Copy().SetPosition(node.PositionStart, node.PositionEnd).SetContext(context);
            return res.Success(value);
        }

        public RuntimeResult VisitVariableAssignNode(VariableAssignNode node, Context context)
        {

            var res = new RuntimeResult();
            var varName = (string)node.Token!.Value!;

            var value = res.Register(Visit(node.ValueNode, context));
            if (res.Error != null) return res;
            if (node.TypeToken != null && node.TypeToken.Value != null) value.SetDefaultType((string)node.TypeToken.Value);
            else value.SetDefaultType("object");

            if (node.IsAssign)
            {
                value.SetImmutable(node.IsImmutable);

                if (context.SymbolTable!.Get(varName) != null)
                    return res.Failure(new RuntimeError(
                        node.Token!.PositionStart!,
                        node.Token!.PositionEnd!,
                        $"Cannot declare var ident '{varName}', ident already exists",
                        context
                    ));
                else if (node.TypeToken != null && (string)node.TypeToken.Value! != value.Type! && (string)node.TypeToken.Value! != "object")
                    return res.Failure(new InvalidTypeError(
                        value.PositionStart!,
                        value.PositionEnd!,
                        $"Expected '{node.TypeToken.Type}', not '{value.Type!}'",
                        context
                    ));
            }
            else
            {
                var tempval = context.SymbolTable!.Get(varName);
              
                if (tempval != null)
                {
                    if (tempval.Immutable)
                        return res.Failure(new RuntimeError(
                            node.PositionStart!,
                            node.PositionEnd!,
                            $"Cannot reassign to immutable var ident '{varName}'",
                            context
                        ));
                    else if (tempval.DefaultType != value.Type! && tempval.DefaultType != "object")
                        return res.Failure(new InvalidTypeError(
                            value.PositionStart!,
                            value.PositionEnd!,
                            $"Expected '{tempval.DefaultType}', not '{value.Type!}'",
                            context
                        ));
                }
                else return res.Failure(new RuntimeError(
                    node.PositionStart!,
                    node.PositionEnd!,
                    $"Var ident '{varName}' is not defined",
                    context
                ));
            }
            context.SymbolTable!.Set(varName, value);
            return res.Success(value);
        }

        public RuntimeResult VisitBinOperatorNode(BinOperatorNode node, Context context)
        {
            var res = new RuntimeResult();
            var left = res.Register(Visit(node.LeftNode, context));
            if (res.Error != null) return res;
            var right = res.Register(Visit(node.RightNode, context));
            if (res.Error != null) return res;

            Tuple<Value?, Error?> callback = new Tuple<Value?, Error?>(new NullValue(), null);

            if (node.Token!.Type.Equals(TokenIdentifier.TT_OP) && node.Token.Value!.Equals('+'))
                callback = left.AddedTo(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_OP) && node.Token.Value!.Equals('-'))
                callback = left.SubtractedBy(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_OP) && node.Token.Value!.Equals('*'))
                callback = left.MultipliedBy(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_OP) && node.Token.Value!.Equals('/'))
                callback = left.DividedBy(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_OP) && node.Token.Value!.Equals('^'))
                callback = left.PoweredBy(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_OP) && node.Token.Value!.Equals('%'))
                callback = left.ModulusOf(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_OP) && node.Token.Value!.Equals('&'))
                callback = left.HandleAnd(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_OP) && node.Token.Value!.Equals('|'))
                callback = left.HandleOr(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_COND) && node.Token.Value!.Equals("=="))
                callback = left.IsEqualTo(right)!;
            else if (node.Token.Type.Equals(TokenIdentifier.TT_COND) && node.Token.Value!.Equals("!="))
                callback = left.IsNotEqualTo(right)!;
            else if (node.Token.Type.Equals(TokenIdentifier.TT_OP) && node.Token.Value!.Equals('<'))
                callback = left.IsLesserThan(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_OP) && node.Token.Value!.Equals('>'))
                callback = left.IsGreaterThan(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_COND) && node.Token.Value!.Equals("<="))
                callback = left.IsLesserThanOrEqualTo(right);
            else if (node.Token.Type.Equals(TokenIdentifier.TT_COND) && node.Token.Value!.Equals(">="))
                callback = left.IsGreaterThanOrEqualTo(right);


            var result = callback.Item1;
            var error = callback.Item2;

            if (error != null) return res.Failure(error);
            return res.Success(result!.SetPosition(node.PositionStart, node.PositionEnd));
        }

        public RuntimeResult VisitUnaryOperatorNode(UnaryOperatorNode node, Context context)
        {
            var res = new RuntimeResult();
            var value = res.Register(Visit(node.Node, context));
            if (res.Error != null) return res;

            Error? error = null;

            if (node.Token!.Matches(TokenIdentifier.TT_OP, '-'))
            {
                Tuple<Value?, Error?> callback = value.MultipliedBy(new IntValue(-1));
                value = callback.Item1;
                error = callback.Item2;
            } else if (node.Token.Matches(TokenIdentifier.TT_OP, '!'))
            {
                Tuple<Value?, Error?> callback = value.HandleNot();
                value = callback.Item1;
                error = callback.Item2;
            }

            if (error != null) return res.Failure(error);

            return res.Success(value!.SetPosition(node.PositionStart, node.PositionEnd));
        }

        public RuntimeResult VisitIfNode(IfNode node, Context context)
        {
            var res = new RuntimeResult();

            foreach (Tuple<Node, Node, bool> callback in node.Cases)
            {
                var condition = callback.Item1;
                var expr = callback.Item2;
                var shouldReturnNull = callback.Item3;

                var value = res.Register(Visit(condition, context));
                if (res.ShouldReturn()) return res;
                var conditionValue = (BoolValue)value;

                if (conditionValue.IsTrue())
                {
                    var exprValue = res.Register(Visit(expr, context));
                    if (res.ShouldReturn()) return res;
                    return res.Success(shouldReturnNull ? new NullValue() : exprValue);
                }
            }

            if (node.ElseCase != null)
            {
                var shouldReturnNull = node.ElseCase.Item2;
                var elseValue = res.Register(Visit(node.ElseCase.Item1, context));
                if (res.Error != null) return res;
                return res.Success(shouldReturnNull ? new NullValue() : elseValue);
            }

            return res.Success(new NullValue());
        }

        public RuntimeResult VisitSwitchNode(SwitchNode node, Context context)
        {
            context.SetIsInSwitch(true);
            var res = new RuntimeResult();

            var mainExprValue = res.Register(Visit(node.MainExpression, context));
            if (res.ShouldReturn()) return res;

            foreach (Tuple<Node, Node, bool> callback in node.Cases)
            {
                var expr = callback.Item1;
                var statements = callback.Item2;
                var shouldReturnNull = callback.Item3;

                var value = res.Register(Visit(expr, context));
                if (res.ShouldReturn()) return res;
                var conditionValue = new BoolValue(mainExprValue.value! == value.value!);

                if (conditionValue.IsTrue())
                {
                    var statementsValue = res.Register(Visit(statements, context));
                    if (res.ShouldReturn()) return res;
                    return res.Success(shouldReturnNull ? new NullValue() : statementsValue);
                }
            }

            if (node.DefaultCase != null)
            {
                var shouldReturnNull = node.DefaultCase.Item2;
                var defaultCaseValue = res.Register(Visit(node.DefaultCase.Item1, context));
                if (res.Error != null) return res;
                return res.Success(shouldReturnNull ? new NullValue() : defaultCaseValue);
            }

            return res.Success(new NullValue());
        }

        public RuntimeResult VisitForNode(ForNode node, Context context)
        {
            context.SetIsInLoop(true);
            var res = new RuntimeResult();
            var elements = new List<Value>();

            var startValue = res.Register(Visit(node.StartValueNode, context));
            if (res.Error != null) return res;
            if (!startValue.Type.Equals("int"))
                return res.Failure(new InvalidTypeError(
                    node.StartValueNode.PositionStart!,
                    node.StartValueNode.PositionEnd!,
                    $"Expected 'int', not '{startValue.Type}'",
                    context
                ));

            var endValue = res.Register(Visit(node.EndValueNode, context));
            if (res.Error != null) return res;
            if (!endValue.Type.Equals("int"))
                return res.Failure(new InvalidTypeError(
                    node.EndValueNode.PositionStart!,
                    node.EndValueNode.PositionEnd!,
                    $"Expected 'int', not '{endValue.Type}'",
                    context
                ));

            var stepValue = new IntValue(1);

            if (node.StepValueNode != null)
            {
                var resValue = res.Register(Visit(node.StepValueNode, context));
                if (res.Error != null) return res;
                if (!resValue.Type.Equals("int"))
                    return res.Failure(new InvalidTypeError(
                        node.StepValueNode.PositionStart!,
                        node.StepValueNode.PositionEnd!,
                        $"Expected 'int', not '{resValue.Type}'",
                        context
                    ));
                stepValue = (IntValue)resValue;
            }

            var i = (int) startValue.value!;

            Func<object?, bool> condition = _ => i > (int) endValue.value!;

            if ((int)stepValue.value! >= 0) condition = _ => i < (int)endValue.value!;

            while (condition(null))
            {
                context.SymbolTable!.Set((string) node.VariableNameToken.Value!, new IntValue(i));
                i += (int)stepValue.value!;

                var value = res.Register(Visit(node.BodyNode, context));
                if (res.ShouldReturn() && !res.LoopShouldContinue && !res.LoopShouldBreak ) return res;

                if (res.LoopShouldContinue) continue;
                if (res.LoopShouldBreak) break;

                elements.Add(value);
            }

            context.SetIsInLoop(false);
            return res.Success(node.ShouldReturnNull
                ? new NullValue()
                : new ListValue(context.SymbolTable!.GetType("object")!, elements).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }

        public RuntimeResult VisitForEachNode(ForEachNode node, Context context)
        {
            context.SetIsInLoop(true);
            var res = new RuntimeResult();
            var elements = new List<Value>();


            string fromVariableName = (string)node.VariableToGetFromNameToken.Value!;
            var fromVariableValue = context.SymbolTable!.Get(fromVariableName);

            if (fromVariableValue == null) return res.Failure(new RuntimeError(
                node.PositionStart!,
                node.PositionEnd!,
                $"Ident '{node.VariableToGetFromNameToken.Value!}' is not defined",
                context
            ));

            fromVariableValue = fromVariableValue.Copy().SetPosition(node.PositionStart, node.PositionEnd).SetContext(context);


            var i = 0;
            var count = fromVariableValue.CountValues();
            if (count.Item2 != null) return res.Failure(count.Item2);

            Func<object?, bool> condition = _ => i < count.Item1;

            var getValues = fromVariableValue.GetAllValues();
            if (getValues.Item2 != null) return res.Failure(getValues.Item2);

            while (condition(null))
            {
                
                context.SymbolTable!.Set((string)node.VariableNameToken.Value!, getValues.Item1![i]);
                i += 1;

                var value = res.Register(Visit(node.BodyNode, context));
                if (res.ShouldReturn() && !res.LoopShouldContinue && !res.LoopShouldBreak) return res;

                if (res.LoopShouldContinue) continue;
                if (res.LoopShouldBreak) break;

                elements.Add(value);
            }

            context.SetIsInLoop(false);
            return res.Success(node.ShouldReturnNull
                ? new NullValue()
                : new ListValue(context.SymbolTable!.GetType("object")!, elements).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }

        public RuntimeResult VisitWhileNode(WhileNode node, Context context)
        {
            context.SetIsInLoop(true);
            var res = new RuntimeResult();
            var elements = new List<Value>();

            while (true)
            {
                var condition = res.Register(Visit(node.ConditionNode, context));
                if (res.ShouldReturn()) return res;
                if (!condition.Type.Equals("bool"))
                    return res.Failure(new InvalidTypeError(
                        node.ConditionNode.PositionStart!,
                        node.ConditionNode.PositionEnd!,
                        $"Expected 'bool', not '{condition.Type}'",
                        context
                    ));

                if (!((BoolValue)condition).IsTrue()) break;

                var value = res.Register(Visit(node.BodyNode, context));
                if (res.ShouldReturn() && !res.LoopShouldContinue && !res.LoopShouldBreak) return res;

                if (res.LoopShouldContinue) continue;
                if (res.LoopShouldBreak) break;

                elements.Add(value);
            }

            context.SetIsInLoop(false);
            return res.Success(node.ShouldReturnNull
                ? new NullValue()
                : new ListValue(context.SymbolTable!.GetType("object")!, elements).SetContext(context).SetPosition(node.PositionStart, node.PositionEnd));
        }

        public RuntimeResult VisitFunctionDefinitionNode(FunctionDefinitionNode node, Context context)
        {
            context.SetIsInFunction(true);
            var res = new RuntimeResult();

            var funcName = (string)node.Token!.Value!;
            var bodyNode = node.BodyNode;
            var argNames = new List<Tuple<string, string>>();

            
            if (context.SymbolTable!.Get(funcName) != null)
                return res.Failure(new RuntimeError(
                    node.Token!.PositionStart!,
                    node.Token!.PositionEnd!,
                    $"Cannot declare func name '{funcName}', ident already exists",
                    context
                ));

            foreach (Tuple<Token, Token> arg in node.ArgumentNameTokens)
            {
                if (!context.SymbolTable.Types.ContainsKey((string)arg.Item2.Value!))
                    return res.Failure(new InvalidTypeError(
                        arg.Item2!.PositionStart!,
                        arg.Item2!.PositionEnd!,
                        $"Undefined type ident '{arg.Item2!.Value!}'",
                        context
                    ));
                argNames.Add(new((string)arg.Item1.Value!, (string)arg.Item2.Value!));
            }
            var funcValue = new FunctionValue(funcName, bodyNode, argNames, node.ShouldAutoReturn);
            funcValue.SetContext(context);
            funcValue.SetPosition(node.PositionStart, node.PositionEnd);

            if (node.Token != null) context.SymbolTable!.SetFunction(funcName, funcValue);

            context.SetIsInFunction(false);
            return res.Success(funcValue);
        }

        public RuntimeResult VisitCallNode(CallNode node, Context context)
        {
            var res = new RuntimeResult();
            var args = new List<Value>();

            var valueToCall = res.Register(Visit(node.NodeToCall, context));
            if (res.ShouldReturn()) return res;

            if (valueToCall.Type != "method")
                return res.Failure(new InvalidTypeError(
                    valueToCall!.PositionStart!,
                    valueToCall!.PositionEnd!,
                    $"Expected 'method', not '{valueToCall.Type}'",
                    valueToCall.Context
                ));

            var valueToCallConverted = context.SymbolTable!.GetFunction((string)valueToCall!.SetPosition(node.PositionStart, node.PositionEnd).value!);
            foreach (Node argNode in node.ArgumentNodes)
            {
                args.Add(res.Register(Visit(argNode, context)));
                if (res.ShouldReturn()) return res;
            }

            var returnValue = res.Register(valueToCallConverted!.Execute(args));
            if (res.ShouldReturn()) return res;
            returnValue = returnValue.Copy().SetPosition(node.PositionStart, node.PositionEnd).SetContext(context);
            return res.Success(returnValue);
        }

        public RuntimeResult VisitReturnNode(ReturnNode node, Context context)
        {
            if (!context.IsInFunction) return new RuntimeResult().Failure(new RuntimeError(
                node.PositionStart!,
                node.PositionEnd!,
                "Cannot use keyword 'return' outside function"
            ));
            var res = new RuntimeResult();
            Value value = new NullValue();

            if (node.NodeToReturn != null)
            {
                value = res.Register(Visit(node.NodeToReturn, context));
                if (res.ShouldReturn()) return res;
            }

            return res.SuccessReturn(value);
        }

        public RuntimeResult VisitContinueNode(ContinueNode node, Context context)
        {
            if (!context.IsInLoop) return new RuntimeResult().Failure(new RuntimeError(
                node.PositionStart!,
                node.PositionEnd!,
                "Cannot use keyword 'continue' outside loop"
            ));
            return new RuntimeResult().SuccessContinue();
        }

        public RuntimeResult VisitBreakNode(BreakNode node, Context context)
        {
            if (!context.IsInLoop && !context.IsInSwitch) return new RuntimeResult().Failure(new RuntimeError(
                node.PositionStart!,
                node.PositionEnd!,
                "Cannot use keyword 'break' outside loop or 'switch' statement"
            ));
            return new RuntimeResult().SuccessBreak();
        }

        public RuntimeResult VisitGetAttributeNode(GetAttributeNode node, Context context)
        {
            var res = new RuntimeResult();
            var atomValue = res.Register(Visit(node.AtomNode, context));
            var keyValue = res.Register(Visit(node.KeyNode, context));
            if (res.Error != null) return res;

            var callback = atomValue.GetValueByKey(keyValue);

            if (callback.Item2 != null) return res.Failure(callback.Item2);

            return res.Success(callback.Item1!);
        }

        public RuntimeResult VisitInstanceOfNode(InstanceOfNode node, Context context)
        {
            var res = new RuntimeResult();
            var atomValue = res.Register(Visit(node.AtomNode, context));
            var typeValue = context.SymbolTable!.GetType((string)node.Token!.Value!);

            if (typeValue == null)
                return res.Failure(new InvalidTypeError(
                    node.Token!.PositionStart!,
                    node.Token!.PositionEnd!,
                    $"Undefined type ident '{node.Token!.Value}'",
                    context
                ));

            if (atomValue.Type == typeValue.GenericTypeName || typeValue.GenericTypeName == "object") return res.Success(new BoolValue(true).SetPosition(node.PositionStart, node.PositionEnd).SetContext(context));
            return res.Success(new BoolValue(false).SetPosition(node.PositionStart, node.PositionEnd).SetContext(context));
        }

        public RuntimeResult VisitSkipNode(SkipNode node, Context context)
        {
            return new RuntimeResult().Success(new NullValue());
        }
    }
}

