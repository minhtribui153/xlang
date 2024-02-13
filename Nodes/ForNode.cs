using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class ForNode : Node
    {
        public Token VariableNameToken { get; private set; }
        public Node StartValueNode { get; private set; }
        public Node EndValueNode { get; private set; }
        public Node? StepValueNode { get; private set; }
        public Node BodyNode { get; private set; }
        public bool ShouldReturnNull { get; private set; }

        public ForNode(Token variableNameToken, Node startValueNode, Node endValueNode, Node? stepValueNode, Node bodyNode, bool shouldReturnNull) : base(null)
        {
            VariableNameToken = variableNameToken;
            StartValueNode = startValueNode;
            EndValueNode = endValueNode;
            StepValueNode = stepValueNode;
            BodyNode = bodyNode;
            ShouldReturnNull = shouldReturnNull;

            PositionStart = VariableNameToken.PositionStart;
            PositionEnd = BodyNode.PositionEnd;
        }

        public override string ToString()
        {
            string stepString = StepValueNode != null ? $" step {StepValueNode}" : "";
            return $"[for: from {StartValueNode} to {EndValueNode}{stepString} => {BodyNode}]";
        }
    }
}

