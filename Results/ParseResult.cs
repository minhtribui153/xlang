using System;
using X_Programming_Language.Constants;
using X_Programming_Language.Errors;
using X_Programming_Language.Nodes;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Results
{
    public class ParseResult
    {
        public Error? Error { get; private set; }
        public Node? Node { get; private set; }
        public Node? CurrentResultNode { get; private set; }
        public object? NodeObject { get; private set; }
        public int LastRegisteredAdvanceCount { get; private set; }
        public int AdvanceCount { get; private set; }
        public int ToReverseCount { get; private set; }
        public bool IsOnInterpreter { get; private set; }


        public ParseResult(bool isOnInterpreter = false)
        {
            Error = null;
            Node = null;
            CurrentResultNode = null;
            NodeObject = null;
            LastRegisteredAdvanceCount = 0;
            AdvanceCount = 0;
            ToReverseCount = 0;
            IsOnInterpreter = isOnInterpreter;
        }

        public void ResetError()
        {
            Error = null;
        }

        public void RegisterAdvancement()
        {
            AdvanceCount += 1;
        }

        public Node Register(ParseResult result, bool autoReturnError = true)
        {
            LastRegisteredAdvanceCount = result.AdvanceCount;
            AdvanceCount += result.AdvanceCount;
            CurrentResultNode = result.Node;
            if (result.Error != null && (CurrentResultNode == null || autoReturnError)) Error = result.Error;
            return result.Node!;
        }

        public object RegisterObject(ParseResult result, bool returnObject = true)
        {
            LastRegisteredAdvanceCount = result.AdvanceCount;
            AdvanceCount += result.AdvanceCount;
            if (result.Error != null) Error = result.Error;
            return result.NodeObject!;
        }



        public Node? TryRegister(ParseResult result)
        {
            if (result.Error != null)
            {
                ToReverseCount = result.AdvanceCount;
                Error = result.Error;
                return null;
            }
            return Register(result);
        }

        public ParseResult Success(Node node)
        {
            Node = node;
            return this;
        }

        public ParseResult Success(object nodeCallback)
        {
            NodeObject = nodeCallback;
            return this;
        }

        public ParseResult Failure(Error error)
        {
            if (Error == null || AdvanceCount == 0)  
                Error = error;
            return this;
        }

        public bool ShouldReturn(Token currentToken)
        {
            if (IsOnInterpreter) return Error != null && currentToken.Type != TokenIdentifier.TT_EOF && CurrentResultNode == null;
            return Error != null && CurrentResultNode == null;
        }
    }
}

