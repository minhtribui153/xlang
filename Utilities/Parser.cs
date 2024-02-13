using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using X_Programming_Language.Constants;
using X_Programming_Language.Errors;
using X_Programming_Language.Nodes;
using X_Programming_Language.Results;
using X_Programming_Language.Values;

namespace X_Programming_Language.Utilities
{
    public class Parser
    {
        public List<Token> Tokens { get; private set; }
        public int TokenIndex { get; private set; }
        public Token? CurrentToken { get; private set; }
        public bool IsOnInterpreter { get; private set; }

        public Parser(List<Token> tokens, bool isOnInterpreter)
        {
            Tokens = tokens;
            TokenIndex = -1;
            CurrentToken = null;
            IsOnInterpreter = isOnInterpreter;
            Advance();
        }

        public Token? Advance()
        {
            TokenIndex += 1;
            UpdateCurrentToken();
            return CurrentToken;
        }

        public Token? Reverse(int amount = 1)
        {
            TokenIndex -= amount;
            UpdateCurrentToken();
            return CurrentToken;
        }

        public void UpdateCurrentToken()
        {
            if (TokenIndex >= 0 && TokenIndex < Tokens.Count)
                CurrentToken = Tokens[TokenIndex];
        }

        public ParseResult Parse()
        {
            var res = Statements();
            if (res.Error == null && CurrentToken!.Type != TokenIdentifier.TT_EOF)
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!,
                    CurrentToken.PositionEnd!,
                    "Invalid Syntax"
                ));
            return res;
        }

        public ParseResult Atom()
        {
            var res = new ParseResult(IsOnInterpreter);
            var token = CurrentToken;

            if (new string[] { TokenIdentifier.TT_INT, TokenIdentifier.TT_FLOAT }.Contains(token!.Type))
            {
                res.RegisterAdvancement();
                Advance();
                return res.Success(new NumberNode(token));
            }
            else if (token!.Type.Equals(TokenIdentifier.TT_STRING))
            {
                res.RegisterAdvancement();
                Advance();
                return res.Success(new StringNode(token));
            }
            else if (token!.Type.Equals(TokenIdentifier.TT_BOOL))
            {
                res.RegisterAdvancement();
                Advance();
                return res.Success(new BooleanNode(token));
            }
            else if (token.Type.Equals(TokenIdentifier.TT_ID))
            {
                res.RegisterAdvancement();
                Advance();
                return res.Success(new VariableAccessNode(token));
            }
            else if (token.Matches(TokenIdentifier.TT_OP, '('))
            {
                res.RegisterAdvancement();
                Advance();
                var expr = res.Register(Expression());
                if (res.ShouldReturn(CurrentToken!)) return res;

                if (CurrentToken!.Matches(TokenIdentifier.TT_OP, ')'))
                {
                    res.RegisterAdvancement();
                    Advance();
                    return res.Success(expr!);
                }
                else return res.Failure(new InvalidSyntaxError(
                    CurrentToken!.PositionStart!,
                    CurrentToken!.PositionEnd!,
                    "Expected token ')'"
                ).SetOverwritable(false));
            }
            else if (token.Matches(TokenIdentifier.TT_OP, '['))
            {
                var listExpr = res.Register(ListExpression());
                if (res.ShouldReturn(CurrentToken!)) return res;
                return res.Success(listExpr);
            }
            else if (token.Matches(TokenIdentifier.TT_KEYWORD, "if"))
            {
                var ifExpr = res.Register(IfExpression());
                if (res.ShouldReturn(CurrentToken!)) return res;
                return res.Success(ifExpr);
            }
            else if (token.Matches(TokenIdentifier.TT_KEYWORD, "for"))
            {
                var forExpr = res.Register(ForExpression());
                if (res.ShouldReturn(CurrentToken!)) return res;
                return res.Success(forExpr);
            }
            else if (token.Matches(TokenIdentifier.TT_KEYWORD, "foreach"))
            {
                var forExpr = res.Register(ForEachExpression());
                if (res.ShouldReturn(CurrentToken!)) return res;
                return res.Success(forExpr);
            }
            else if (token.Matches(TokenIdentifier.TT_KEYWORD, "while"))
            {
                var whileExpr = res.Register(WhileExpression());
                if (res.ShouldReturn(CurrentToken!)) return res;
                return res.Success(whileExpr);
            }
            else if (token.Matches(TokenIdentifier.TT_KEYWORD, "func"))
            {
                var funcExpr = res.Register(FunctionDefinition());
                if (res.ShouldReturn(CurrentToken!)) return res;
                return res.Success(funcExpr);
            }

            Reverse(1);
            var lastTokenBeforeCurrent = CurrentToken;
            Advance();

            if (
                token.Type != TokenIdentifier.TT_EOF ||
                SyntaxIdentifier.COND_OPERATORS.Contains(lastTokenBeforeCurrent!.Value!) ||
                (
                    lastTokenBeforeCurrent!.Value is char v &&
                    v != ';' &&
                    SyntaxIdentifier.OPERATORS.Contains((char)lastTokenBeforeCurrent!.Value!)
                )
            )
            {
                return res.Failure(new InvalidSyntaxError(
                    token.PositionStart!,
                    token.PositionEnd!,
                    $"Expected value, var ident, or token"
                ).SetOverwritable(true));
            }
            return res.Success(new SkipNode(token));
        }

        public ParseResult Power()
        {
            return BinOperation(Call, new object[] { '^' }, Factor);
        }

        public ParseResult Call()
        {
            var res = new ParseResult(IsOnInterpreter);
            var atom = res.Register(Atom());
            if (res.ShouldReturn(CurrentToken!)) return res;


            if (CurrentToken!.Matches(TokenIdentifier.TT_OP, '('))
            {
                res.RegisterAdvancement();
                Advance();
                List<Node> argumentNodes = new();

                if (CurrentToken.Matches(TokenIdentifier.TT_OP, ')'))
                {
                    res.RegisterAdvancement();
                    Advance();
                }
                else
                {
                    argumentNodes.Add(res.Register(Expression()));
                    if (res.ShouldReturn(CurrentToken!))
                        return res.Failure(new InvalidSyntaxError(
                            CurrentToken.PositionStart!,
                            CurrentToken.PositionEnd!,
                            "Expected token ')', keyword or value"
                        ).SetOverwritable(false));

                    while (CurrentToken.Matches(TokenIdentifier.TT_OP, ','))
                    {
                        res.RegisterAdvancement();
                        Advance();

                        argumentNodes.Add(res.Register(Expression()));
                        if (res.ShouldReturn(CurrentToken!)) return res;
                    }

                    if (!CurrentToken.Matches(TokenIdentifier.TT_OP, ')'))
                        return res.Failure(new InvalidSyntaxError(
                            CurrentToken.PositionStart!,
                            CurrentToken.PositionEnd!,
                            "Expected token ',' or token ')'"
                        ).SetOverwritable(false));

                    res.RegisterAdvancement();
                    Advance();
                }
                return res.Success(new CallNode(atom, argumentNodes));
            }
            else if (CurrentToken!.Matches(TokenIdentifier.TT_OP, '['))
            {
                res.RegisterAdvancement();
                Advance();
                var expr = res.Register(Expression());
                if (res.ShouldReturn(CurrentToken!)) return res;

                if (!CurrentToken!.Matches(TokenIdentifier.TT_OP, ']'))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken!.PositionStart!,
                        CurrentToken!.PositionEnd!,
                        "Expected token ']'"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();

                return res.Success(new GetAttributeNode(expr!, atom, CurrentToken!.PositionEnd!.Copy()));
            }
            else if (CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "instof"))
            {
                res.RegisterAdvancement();
                Advance();

                if (CurrentToken!.Type != TokenIdentifier.TT_ID)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken!.PositionStart!,
                        CurrentToken!.PositionEnd!,
                        "Expected type ident"
                    ).SetOverwritable(false));

                var typeToken = CurrentToken;

                res.RegisterAdvancement();
                Advance();

                return res.Success(new InstanceOfNode(atom, typeToken));
            }

            return res.Success(atom);
        }

        public ParseResult Factor()
        {
            var res = new ParseResult(IsOnInterpreter);
            var token = CurrentToken;

            if (token!.Type.Equals(TokenIdentifier.TT_OP) && new char[] { '+', '-' }.Contains((char) token!.Value!))
            {
                res.RegisterAdvancement();
                Advance();
                var factor = res.Register(Factor());
                if (res.ShouldReturn(CurrentToken!)) return res;
                return res.Success(new UnaryOperatorNode(token, factor!));
            }

            return Power();
        }

        public ParseResult Term()
        {
            return BinOperation(Factor, new object[] { '*', '/', '^', '%' });
        }

        public ParseResult ArithmeticExpression()
        {
            return BinOperation(Term, new object[] { '+', '-' });
        }

        public ParseResult IfExpression()
        {
            var res = new ParseResult(IsOnInterpreter);
            var allCases = res.RegisterObject(IfExpressionCases("if")) as Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>;
            if (res.ShouldReturn(CurrentToken!)) return res;
            return res.Success(new IfNode(allCases!.Item1, allCases!.Item2));
        }

        public ParseResult IfExpressionCases(string caseKeyword)
        {
            var res = new ParseResult(IsOnInterpreter);
            var cases = new List<Tuple<Node, Node, bool>>(); // condition, statements, shouldReturnNull
            Tuple<Node, bool>? elseCase = null;

            if (!CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "if"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    $"Expected keyword '{caseKeyword}'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            var condition = res.Register(Expression());
            if (res.ShouldReturn(CurrentToken!)) return res;

            if (!CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "then"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'then'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            if (CurrentToken.Type == TokenIdentifier.TT_NEWLINE)
            {
                res.RegisterAdvancement();
                Advance();

                var statements = res.Register(Statements());
                if (res.ShouldReturn(CurrentToken!)) return res;
                cases.Add(new(condition, statements, true));

                if (CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "end"))
                {
                    res.RegisterAdvancement();
                    Advance();
                }
                else
                {
                    var allCases = res.RegisterObject(IfExpressionElse()) as Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>;
                    if (res.ShouldReturn(CurrentToken!)) return res;
                    foreach (var newCase in allCases!.Item1) cases.Add(newCase);
                    elseCase = allCases.Item2;
                }
            }
            else
            {
                var expr = res.Register(Statement());
                if (res.ShouldReturn(CurrentToken!)) return res;
                cases.Add(new(condition, expr, false));

                var allCases = res.RegisterObject(IfExpressionElse(), true) as Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>;
                if (res.ShouldReturn(CurrentToken!)) return res;
                foreach (var newCase in allCases!.Item1) cases.Add(newCase);
                elseCase = allCases.Item2;
            }
            

            return res.Success(new Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>(cases, elseCase));
        }

        public ParseResult IfExpressionElse()
        {
            var res = new ParseResult(IsOnInterpreter);
            List<Tuple<Node, Node, bool>> cases = new();
            Tuple<Node, bool>? elseCase = null;

            if (CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "else"))
            {
                res.RegisterAdvancement();
                Advance();

                if (CurrentToken.Type == TokenIdentifier.TT_NEWLINE)
                {
                    res.RegisterAdvancement();
                    Advance();

                    var statements = res.Register(Statements());
                    if (res.ShouldReturn(CurrentToken!)) return res;
                    elseCase = new(statements, true);

                    if (CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "end"))
                    {
                        res.RegisterAdvancement();
                        Advance();
                    }
                    else return res.Failure(new InvalidSyntaxError(
                            CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                            "Expected keyword 'end'"
                        ).SetOverwritable(false));
                }
                else if (CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "if"))
                {
                    var allCases = res.RegisterObject(IfExpressionCases("if")) as Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>;
                    if (res.ShouldReturn(CurrentToken!)) return res;

                    foreach (var allCase in allCases!.Item1) cases.Add(allCase);
                }
                else
                {
                    var expr = res.Register(Statement());
                    if (res.ShouldReturn(CurrentToken!)) return res;
                    elseCase = new(expr, false);
                }
            }
            
            return res.Success(new Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>(cases!, elseCase));
        }

        public ParseResult ForExpression()
        {
            var res = new ParseResult(IsOnInterpreter);

            if (!CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "for"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'for'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            if (!CurrentToken.Type.Equals(TokenIdentifier.TT_ID))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected var ident"
                ).SetOverwritable(false));

            var varName = CurrentToken;
            res.RegisterAdvancement();
            Advance();

            if (!CurrentToken.Matches(TokenIdentifier.TT_OP, '='))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected token '='"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            var startValue = res.Register(Expression());
            if (res.ShouldReturn(CurrentToken!)) return res;

            if (!CurrentToken.Matches(TokenIdentifier.TT_OP, ':'))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected token ':'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            Node? stepValue = null;

            var endValue = res.Register(Expression());
            if (res.ShouldReturn(CurrentToken!)) return res;

            if (CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "step"))
            {
                res.RegisterAdvancement();
                Advance();

                stepValue = res.Register(Expression());
                if (res.ShouldReturn(CurrentToken!)) return res;
            }

            if (!CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "then"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'then'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            Node? body;

            if (CurrentToken.Type == TokenIdentifier.TT_NEWLINE)
            {
                res.RegisterAdvancement();
                Advance();

                body = res.Register(Statements());
                if (res.ShouldReturn(CurrentToken!)) return res;

                if (!CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "end"))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken!.PositionStart!, CurrentToken!.PositionEnd!,
                        "Expected keyword 'end'"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();

                return res.Success(new ForNode(varName, startValue, endValue, stepValue, body, true));
            }

            body = res.Register(Statement());
            if (res.ShouldReturn(CurrentToken!)) return res;


            return res.Success(new ForNode(varName, startValue, endValue, stepValue, body, false));
        }

        public ParseResult ForEachExpression()
        {
            var res = new ParseResult(IsOnInterpreter);

            if (!CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "foreach"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'foreach'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            if (!CurrentToken.Type.Equals(TokenIdentifier.TT_ID))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected var ident"
                ).SetOverwritable(false));

            var varName = CurrentToken;
            res.RegisterAdvancement();
            Advance();

            if (!CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "from"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'from'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            if (!CurrentToken.Type.Equals(TokenIdentifier.TT_ID))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected type ident"
                ).SetOverwritable(false));

            var varToGetFrom = CurrentToken;
            res.RegisterAdvancement();
            Advance();

            if (!CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "then"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected var ident"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            Node? body;

            if (CurrentToken.Type == TokenIdentifier.TT_NEWLINE)
            {
                res.RegisterAdvancement();
                Advance();

                body = res.Register(Statements());
                if (res.ShouldReturn(CurrentToken!)) return res;

                if (!CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "end"))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken!.PositionStart!, CurrentToken!.PositionEnd!,
                        "Expected keyword 'end'"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();

                return res.Success(new ForEachNode(varName, varToGetFrom, body, true));
            }

            body = res.Register(Statement());
            if (res.ShouldReturn(CurrentToken!)) return res;


            return res.Success(new ForEachNode(varName, varToGetFrom, body, false));
        }

        public ParseResult WhileExpression()
        {
            var res = new ParseResult(IsOnInterpreter);

            if (!CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "while"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'while'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            var condition = res.Register(Expression());
            if (res.ShouldReturn(CurrentToken!)) return res;

            if (!CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "then"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'then'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            Node? body;

            if (CurrentToken.Type == TokenIdentifier.TT_NEWLINE)
            {
                res.RegisterAdvancement();
                Advance();

                body = res.Register(Statements(), false);
                if (res.ShouldReturn(CurrentToken!)) return res;

                if (!CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "end"))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken!.PositionStart!, CurrentToken!.PositionEnd!,
                        "Expected keyword 'end'"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();

                return res.Success(new WhileNode(condition, body, true));
            }

            body = res.Register(Statement(), false);
            if (res.ShouldReturn(CurrentToken!)) return res;

            return res.Success(new WhileNode(condition, body, false));
        }

        public ParseResult SwitchExpression()
        {
            var res = new ParseResult(IsOnInterpreter);

            if (!CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "switch"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'switch'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            var expr = res.Register(Expression());
            if (res.ShouldReturn(CurrentToken!)) return res;

            if (!CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "then"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'then'"
                ).SetOverwritable(false));
            

            res.RegisterAdvancement(); 
            Advance();

            if (CurrentToken.Type != TokenIdentifier.TT_NEWLINE)
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected newline"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            if (CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "end")) return res.Success(new SwitchNode(expr, new(), null));


            var allCases = res.RegisterObject(CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "case") ? CaseExpression() : CaseExpressionDefault()) as Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>;
            if (res.ShouldReturn(CurrentToken!) && allCases == null) return res;

            if (!CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "end"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'end'"
                ).SetOverwritable(false));
            res.RegisterAdvancement();
            res.ResetError();
            Advance();
            return res.Success(new SwitchNode(expr, allCases!.Item1, allCases!.Item2));
        }

        public ParseResult CaseExpression(bool alreadyHasDefault = false)
        {
            var res = new ParseResult(IsOnInterpreter);
            var cases = new List<Tuple<Node, Node, bool>>(); // condition, statements, shouldReturnNull

            if (!CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "case"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    $"Expected keyword 'case'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            var expr = res.Register(Expression());
            if (res.ShouldReturn(CurrentToken!)) return res;

            if (!CurrentToken!.Matches(TokenIdentifier.TT_OP, ':'))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected token ':'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            if (CurrentToken.Type != TokenIdentifier.TT_NEWLINE)
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected newline"
                ).SetOverwritable(false));

            if (CurrentToken.Matches(TokenIdentifier.TT_NEWLINE, ';'))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Unexpected token ';'"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            var statements = res.Register(Statements()) as ListNode;
            if (res.ShouldReturn(CurrentToken!) && statements == null) return res;
            if (statements!.ElementNodes.Count == 0 || (statements!.ElementNodes.Count > 0 && statements!.ElementNodes[^1] is not BreakNode))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                    "Expected keyword 'break'"
                ).SetOverwritable(false));

            cases.Add(new(expr, statements, true));

            var allCases = res.RegisterObject(CaseExpressionDefault(alreadyHasDefault)) as Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>;
            if (res.ShouldReturn(CurrentToken!) && allCases == null) return res;
            foreach (var newCase in allCases!.Item1) cases.Add(newCase);
            Tuple<Node, bool>? defaultCase = allCases.Item2;

            return res.Success(new Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>(cases, defaultCase));
        }

        public ParseResult CaseExpressionDefault(bool alreadyHasDefault = false)
        {
            var res = new ParseResult(IsOnInterpreter);
            List<Tuple<Node, Node, bool>> cases = new();
            Tuple<Node, bool>? defaultCase = null;
            if (CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "case"))
            {
                var allCases = res.RegisterObject(CaseExpression(alreadyHasDefault)) as Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>;
                if (res.ShouldReturn(CurrentToken!) && allCases == null) return res;
                foreach (var newCase in allCases!.Item1) cases.Add(newCase);
                defaultCase = allCases.Item2;
            }
            else if (CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "default"))
            {
                if (alreadyHasDefault)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken!.PositionStart!, CurrentToken!.PositionEnd!,
                        "Cannot redeclare another keyword 'default', already exists in 'switch' statement"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();

                if (!CurrentToken!.Matches(TokenIdentifier.TT_OP, ':'))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                        "Expected token ':'"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();

                if (CurrentToken.Type != TokenIdentifier.TT_NEWLINE)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                        "Expected newline"
                    ).SetOverwritable(false));

                if (CurrentToken.Matches(TokenIdentifier.TT_NEWLINE, ';'))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                        "Unexpected token ';'"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();

                var statements = res.Register(Statements()) as ListNode;
                if (res.ShouldReturn(CurrentToken!) && statements == null) return res;
                if (statements!.ElementNodes.Count == 0 || (statements!.ElementNodes.Count > 0 && statements!.ElementNodes[^1] is not BreakNode))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                        "Expected keyword 'break'"
                    ).SetOverwritable(false));
                defaultCase = new(statements!, true);

                if (CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "case") || CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "default"))
                {
                    var allCases = res.RegisterObject(CaseExpressionDefault(true)) as Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>;
                    if (res.ShouldReturn(CurrentToken!) && allCases == null) return res;
                    foreach (var newCase in allCases!.Item1) cases.Add(newCase);
                }
            }

            return res.Success(new Tuple<List<Tuple<Node, Node, bool>>, Tuple<Node, bool>?>(cases, defaultCase));
        }

        public ParseResult ComparisonExpression()
        {
            var res = new ParseResult(IsOnInterpreter);

            Node node;

            if (CurrentToken!.Matches(TokenIdentifier.TT_OP, '!'))
            {
                var opToken = CurrentToken;
                res.RegisterAdvancement();
                Advance();

                node = res.Register(ComparisonExpression());
                if (res.ShouldReturn(CurrentToken!)) return res;
                return res.Success(new UnaryOperatorNode(opToken, node));
            }

            node = res.Register(BinOperation(ArithmeticExpression, new object[] { "==", "!=", "<=", ">=", '<', '>' }));

            if (res.ShouldReturn(CurrentToken))
            {
                if (res.Error!.Overwritable)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                        "Expected value, var ident, or token"
                    ).SetOverwritable(true));
                return res;
            };

            return res.Success(node);
        }

        public ParseResult ListExpression()
        {
            var res = new ParseResult(IsOnInterpreter);
            var elementNodes = new List<Node>();
            var positionStart = CurrentToken!.PositionStart!.Copy();
            Token? typeToken = null;

            if (!CurrentToken.Matches(TokenIdentifier.TT_OP, '['))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!,
                    CurrentToken.PositionEnd!,
                    $"Expected token '['"
                ).SetOverwritable(false));

            res.RegisterAdvancement();
            Advance();

            while (CurrentToken.Matches(TokenIdentifier.TT_NEWLINE, '\n'))
            {
                res.RegisterAdvancement();
                Advance();
            }

            if (CurrentToken.Matches(TokenIdentifier.TT_OP, ']'))
            {
                res.RegisterAdvancement();
                Advance();
            }
            else
            {
                elementNodes.Add(res.Register(Expression()));
                if (res.ShouldReturn(CurrentToken!))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!,
                        CurrentToken.PositionEnd!,
                        "Expected token ']', keyword or value"
                    ).SetOverwritable(false));

                while (CurrentToken.Matches(TokenIdentifier.TT_OP, ','))
                {
                    res.RegisterAdvancement();
                    Advance();

                    while (CurrentToken.Matches(TokenIdentifier.TT_NEWLINE, '\n'))
                    {
                        res.RegisterAdvancement();
                        Advance();
                    }

                    if (CurrentToken.Matches(TokenIdentifier.TT_OP, ']')) break;

                    elementNodes.Add(res.Register(Expression()));
                    if (res.ShouldReturn(CurrentToken!)) return res;
                }

                while (CurrentToken.Matches(TokenIdentifier.TT_NEWLINE, '\n'))
                {
                    res.RegisterAdvancement();
                    Advance();
                }

                if (!CurrentToken.Matches(TokenIdentifier.TT_OP, ']'))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!,
                        CurrentToken.PositionEnd!,
                        "Expected token ',' or token ']'"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();
            }

            if (CurrentToken.Matches(TokenIdentifier.TT_OP, '<'))
            {
                res.RegisterAdvancement();
                Advance();

                if (CurrentToken.Type != TokenIdentifier.TT_ID)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!,
                        CurrentToken.PositionEnd!,
                        $"Expected type ident"
                    ).SetOverwritable(false));

                typeToken = CurrentToken;

                res.RegisterAdvancement();
                Advance();

                if (!CurrentToken.Matches(TokenIdentifier.TT_OP, '>'))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!,
                        CurrentToken.PositionEnd!,
                        $"Expected token '>'"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();
            }
            return res.Success(new ListNode(typeToken, elementNodes, positionStart, CurrentToken!.PositionEnd!.Copy()));
        }

        public ParseResult Expression()
        {
            var res = new ParseResult(IsOnInterpreter);
            if (CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "assign"))
            {
                res.RegisterAdvancement();
                Advance();

                var immutable = true;

                if (CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "mut"))
                {
                    res.RegisterAdvancement();
                    Advance();
                    immutable = false;
                }

                if (CurrentToken.Type != TokenIdentifier.TT_ID)
                {
                    if (immutable)
                        return res.Failure(new InvalidSyntaxError(
                            CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                            "Expected var ident or keyword 'mut'"
                        ).SetOverwritable(false));
                    else return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                        "Expected var ident"
                    ).SetOverwritable(false));
                }

                var varName = CurrentToken;
                res.RegisterAdvancement();
                Advance();

                Token? typeToken = null;

                if (CurrentToken.Matches(TokenIdentifier.TT_OP, ':'))
                {
                    res.RegisterAdvancement();
                    Advance();

                    if (CurrentToken.Type != TokenIdentifier.TT_ID)
                        return res.Failure(new InvalidSyntaxError(
                            CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                            "Expected type ident"
                        ).SetOverwritable(false));

                    typeToken = CurrentToken;

                    res.RegisterAdvancement();
                    Advance();
                }

                if (!CurrentToken.Matches(TokenIdentifier.TT_OP, '='))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                        "Expected token '='"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();
                var expr = res.Register(Expression());
                if (res.ShouldReturn(CurrentToken!)) return res;
                return res.Success(new VariableAssignNode(typeToken, varName, expr!, immutable, true));
            }
            else if (CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "set"))
            {
                res.RegisterAdvancement();
                Advance();

                if (CurrentToken.Type != TokenIdentifier.TT_ID)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                        "Expected var ident"
                    ).SetOverwritable(false));


                var varName = CurrentToken;
                res.RegisterAdvancement();
                Advance();

                if (!CurrentToken.Matches(TokenIdentifier.TT_OP, '='))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                        "Expected token '='"
                    ).SetOverwritable(false));

                res.RegisterAdvancement();
                Advance();
                var expr = res.Register(Expression());
                if (res.ShouldReturn(CurrentToken!)) return res;
                return res.Success(new VariableAssignNode(varName, expr!, true, false));
            }
            else if (CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "switch"))
            {
                var switchExpr = res.Register(SwitchExpression());
                if (res.ShouldReturn(CurrentToken!)) return res.Failure(res.Error!);
                return res.Success(switchExpr);
            }

            var node = res.Register(BinOperation(ComparisonExpression, new object[] { '&', '|' }));
            if (res.ShouldReturn(CurrentToken) && node == null)
            {
                if (res.Error!.Overwritable)
                {
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!, CurrentToken.PositionEnd!,
                        "Expected keyword, value, var ident, or token"
                    ).SetOverwritable(true));
                }
                return res;
            };


            return res.Success(node);
        }

        public ParseResult FunctionDefinition()
        {
            var res = new ParseResult(IsOnInterpreter);

            if (!CurrentToken!.Matches(TokenIdentifier.TT_KEYWORD, "func"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!,
                    CurrentToken.PositionEnd!,
                    "Expected keyword 'func'"
                ));

            res.RegisterAdvancement();
            Advance();

            Token? varNameToken;
            if (CurrentToken.Type.Equals(TokenIdentifier.TT_ID))
            {
                varNameToken = CurrentToken;
                res.RegisterAdvancement();
                Advance();
                if (!CurrentToken.Matches(TokenIdentifier.TT_OP, '('))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!,
                        CurrentToken.PositionEnd!,
                        "Expected token '('"
                    ));
            }
            else return res.Failure(new InvalidSyntaxError(
                CurrentToken.PositionStart!,
                CurrentToken.PositionEnd!,
                "Expected func name"
            ));

            res.RegisterAdvancement();
            Advance();

            List<Tuple<Token, Token>> argumentNameTokens = new();

            if (CurrentToken.Type.Equals(TokenIdentifier.TT_ID))
            {
                var paramIdentifier = CurrentToken;
                Advance();
                if (!CurrentToken.Type.Equals(TokenIdentifier.TT_ID))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!,
                        CurrentToken.PositionEnd!,
                        "Expected type ident"
                    ));
                var typeIdentifier = CurrentToken;
                res.RegisterAdvancement();
                Advance();

                argumentNameTokens.Add(new(paramIdentifier, typeIdentifier));
                

                while (CurrentToken.Matches(TokenIdentifier.TT_OP, ','))
                {
                    res.RegisterAdvancement();
                    Advance();

                    if (!CurrentToken.Type.Equals(TokenIdentifier.TT_ID))
                        return res.Failure(new InvalidSyntaxError(
                            CurrentToken.PositionStart!,
                            CurrentToken.PositionEnd!,
                            "Expected param ident"
                        ));

                    paramIdentifier = CurrentToken;

                    res.RegisterAdvancement();
                    Advance();

                    if (!CurrentToken.Type.Equals(TokenIdentifier.TT_ID))
                        return res.Failure(new InvalidSyntaxError(
                            CurrentToken.PositionStart!,
                            CurrentToken.PositionEnd!,
                            "Expected type ident"
                        ));

                    typeIdentifier = CurrentToken;

                    argumentNameTokens.Add(new(paramIdentifier, typeIdentifier));
                    res.RegisterAdvancement();
                    Advance();
                }

                if (!CurrentToken.Matches(TokenIdentifier.TT_OP, ')'))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!,
                        CurrentToken.PositionEnd!,
                        "Expected token ',' or token ')'"
                    ));
            }
            else
            {
                if (!CurrentToken.Matches(TokenIdentifier.TT_OP, ')'))
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!,
                        CurrentToken.PositionEnd!,
                        "Expected param ident or token ')'"
                    ));
            }

            res.RegisterAdvancement();
            Advance();

            Token? returnTypeToken = null;

            if (CurrentToken.Matches(TokenIdentifier.TT_OP, ':'))
            {
                res.RegisterAdvancement();
                Advance();

                if (CurrentToken.Type == TokenIdentifier.TT_ID)
                    return res.Failure(new InvalidSyntaxError(
                        CurrentToken.PositionStart!,
                        CurrentToken.PositionEnd!,
                        "Expected type ident"
                    ));

                returnTypeToken = CurrentToken;

                res.RegisterAdvancement();
                Advance();
            }

            

            if (CurrentToken.Matches(TokenIdentifier.TT_SYNTAX_OP, "=>"))
            {
                res.RegisterAdvancement();
                Advance();
                var nodeToReturn = res.Register(Expression());
                if (res.ShouldReturn(CurrentToken!)) return res;

                return res.Success(new FunctionDefinitionNode(varNameToken, argumentNameTokens, nodeToReturn, returnTypeToken, true));
            }

            if (CurrentToken.Type != TokenIdentifier.TT_NEWLINE)
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!,
                    CurrentToken.PositionEnd!,
                    "Expected syntax token '=>' or newline"
                ));

            res.RegisterAdvancement();
            Advance();

            var body = res.Register(Statements());

            if (!CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "end"))
                return res.Failure(new InvalidSyntaxError(
                    CurrentToken.PositionStart!,
                    CurrentToken.PositionEnd!,
                    "Expected keyword 'end'"
                ));

            res.RegisterAdvancement();
            Advance();

            return res.Success(new FunctionDefinitionNode(varNameToken, argumentNameTokens, body, returnTypeToken, false));
        }

        public ParseResult Statements()
        {
            var res = new ParseResult(IsOnInterpreter);
            var statements = new List<Node>();
            var positionStart = CurrentToken!.PositionStart!.Copy();

            while (CurrentToken!.Type == TokenIdentifier.TT_NEWLINE)
            {
                res.RegisterAdvancement();
                Advance();
            }

            var statement = res.Register(Statement());
            if (res.ShouldReturn(CurrentToken!)) return res.Failure(res.Error!);

            statements.Add(statement);

            var moreStatements = true;

            while (true)
            {
                var newLineCount = 0;
                while (CurrentToken.Type == TokenIdentifier.TT_NEWLINE)
                {
                    res.RegisterAdvancement();
                    Advance();
                    newLineCount += 1;
                }

                if (newLineCount == 0) moreStatements = false;

                if (!moreStatements) break;
                statement = res.TryRegister(Statement());
                if (statement == null)
                {
                    Reverse(res.ToReverseCount);
                    moreStatements = false;
                    continue;
                }
                statements.Add(statement);
            }


            return res.Success(new ListNode(
                null,
                statements,
                positionStart,
                CurrentToken.PositionEnd!.Copy()
            ));
        }

        public ParseResult Statement()
        {
            var res = new ParseResult(IsOnInterpreter);
            var positionStart = CurrentToken!.PositionStart!.Copy();

            if (CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "return"))
            {
                res.RegisterAdvancement();
                Advance();

                var expr = res.TryRegister(Expression());
                if (expr == null) Reverse(res.ToReverseCount);
                return res.Success(new ReturnNode(expr!, positionStart, CurrentToken.PositionStart.Copy()));
            }

            if (CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "continue"))
            {
                res.RegisterAdvancement();
                Advance();

                return res.Success(new ContinueNode(positionStart, CurrentToken.PositionStart.Copy()));
            }

            if (CurrentToken.Matches(TokenIdentifier.TT_KEYWORD, "break"))
            {
                res.RegisterAdvancement();
                Advance();

                return res.Success(new BreakNode(positionStart, CurrentToken.PositionStart.Copy()));
            }

            var expr2 = res.Register(Expression());
            if (res.ShouldReturn(CurrentToken)) return res;

            return res.Success(expr2);
        }

        // Util Functions

        public ParseResult BinOperation(Func<ParseResult> functionA, object[] operators, Func<ParseResult>? functionB = null)
        {
            functionB ??= functionA;

            var res = new ParseResult(IsOnInterpreter);
            var left = res.Register(functionA());
            if (res.ShouldReturn(CurrentToken!)) return res;
            while (CurrentToken!.Value != null && operators.Contains(CurrentToken!.Value))
            {
                var opToken = CurrentToken;
                res.RegisterAdvancement();
                Advance();
                var right = res.Register(functionB());
                if (res.ShouldReturn(CurrentToken!)) return res;

                left = new BinOperatorNode(left!, opToken, right!);
            }

            return res.Success(left!);
        }
    }
}

