using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class ListNode : Node
    {
        public List<Node> ElementNodes { get; private set; }
        public Token? TypeToken { get; private set; }

        public ListNode(Token? typeToken, List<Node> elementNodes, Position positionStart, Position positionEnd) :  base(null)
        {
            TypeToken = typeToken;
            ElementNodes = elementNodes;
            PositionStart = positionStart;
            PositionEnd = positionEnd;
        }

        public override string ToString()
        {
            return $"[list: {UtilFunctions.ListToString(ElementNodes)}]";
        }
    }
}

