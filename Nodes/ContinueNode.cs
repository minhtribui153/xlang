using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class ContinueNode : Node
    {
        public ContinueNode(Position positionStart, Position positionEnd) : base(null)
        {
            PositionStart = positionStart;
            PositionEnd = positionEnd;
        }

        public override string ToString()
        {
            return $"[continue]";
        }
    }
}
