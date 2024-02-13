using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class ForEachNode : Node
    {
        public Token VariableNameToken { get; private set; }
        public Token VariableToGetFromNameToken { get; private set; }
        public Node BodyNode { get; private set; }
        public bool ShouldReturnNull { get; private set; }

        public ForEachNode(Token variableNameToken, Token variableToGetFromNameToken, Node bodyNode, bool shouldReturnNull) : base(null)
        {
            VariableNameToken = variableNameToken;
            VariableToGetFromNameToken = variableToGetFromNameToken;
            BodyNode = bodyNode;
            ShouldReturnNull = shouldReturnNull;

            PositionStart = VariableNameToken.PositionStart;
            PositionEnd = BodyNode.PositionEnd;
        }

        public override string ToString()
        {
            return $"[foreach: {VariableNameToken} from {VariableToGetFromNameToken} => {BodyNode}]";
        }
    }
}

