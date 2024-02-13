using System;
namespace X_Programming_Language.Nodes
{
    public class CallNode : Node
    {
        public Node NodeToCall { get; private set; }
        public List<Node> ArgumentNodes { get; private set; }
        public CallNode(Node nodeToCall, List<Node> argumentNodes) : base(null)
        {
            NodeToCall = nodeToCall;
            ArgumentNodes = argumentNodes;

            PositionStart = NodeToCall.PositionStart;

            if (ArgumentNodes.Count > 0)
                PositionEnd = ArgumentNodes[^1].PositionEnd;
            else
                PositionEnd = NodeToCall.PositionEnd;
        }

        public override string ToString()
        {
            return $"[call: {NodeToCall} ({string.Join(", ", ArgumentNodes)})]";
        }
    }
}

