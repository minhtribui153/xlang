using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class ReturnNode : Node
    {
        public Node NodeToReturn { get; private set; }

        public ReturnNode(Node nodeToReturn, Position positionStart, Position positionEnd) : base(null)
        {
            NodeToReturn = nodeToReturn;

            PositionStart = positionStart;
            PositionEnd = positionEnd;
        }

        public override string ToString()
        {
            return $"[return: {NodeToReturn}]";
        }
    }
}

