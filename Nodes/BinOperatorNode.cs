using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class BinOperatorNode : Node
    {
        public Node LeftNode { get; private set; }
        public Node RightNode { get; private set; }

        public BinOperatorNode(Node leftNode, Token operatorToken, Node rightNode) : base(operatorToken)
        {
            LeftNode = leftNode;
            RightNode = rightNode;

            PositionStart = leftNode.PositionStart;
            PositionEnd = rightNode != null ? rightNode.PositionEnd : leftNode.PositionEnd;
        }

        public override string ToString()
        {
            return $"({LeftNode}, {Token}, {RightNode})";
        }
    }
}

