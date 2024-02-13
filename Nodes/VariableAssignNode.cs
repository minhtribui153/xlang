using System;
using X_Programming_Language.Results;
using X_Programming_Language.Utilities;
using X_Programming_Language.Values;

namespace X_Programming_Language.Nodes
{
    public class VariableAssignNode: Node
    {
        public Node ValueNode { get; set; }
        public Token? TypeToken { get; set; }
        public bool IsImmutable { get; private set; }
        public bool IsAssign { get; private set; }

        public VariableAssignNode(Token variableNameToken, Node valueNode, bool isImmutable, bool isAssign) : base(variableNameToken)
        {
            ValueNode = valueNode;
            IsImmutable = isImmutable;
            IsAssign = isAssign;
            TypeToken = null;
        }

        public VariableAssignNode(Token? typeToken, Token variableNameToken, Node valueNode, bool isImmutable, bool isAssign): base(variableNameToken)
        {
            ValueNode = valueNode;
            IsImmutable = isImmutable;
            IsAssign = isAssign;
            TypeToken = typeToken;
        }

        public override string ToString()
        {
            return $"[var: \"{Token!.Value}\" = {ValueNode}]";
        }
    }
}

