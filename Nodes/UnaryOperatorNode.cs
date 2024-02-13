using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class UnaryOperatorNode : Node
    {
        public Node Node { get; private set; }

        public UnaryOperatorNode(Token operatorToken, Node node) : base(operatorToken)
        {
            Node = node;
        }

        public override string ToString()
        {
            return $"({Token}, {Node})";
        }
    }
}

