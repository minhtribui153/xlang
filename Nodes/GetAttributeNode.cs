using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class GetAttributeNode : Node
    {
        public Node KeyNode;
        public Node AtomNode;
        public GetAttributeNode(Node keyNode, Node atomNode, Position positionEnd) : base(null)
        {
            KeyNode = keyNode;
            AtomNode = atomNode;
            PositionStart = atomNode.PositionStart;
            PositionEnd = positionEnd;
        }

        public override string ToString()
        {
            return $"[get_attr: {KeyNode.Token} => {AtomNode.Token}]";
        }
    }
}

