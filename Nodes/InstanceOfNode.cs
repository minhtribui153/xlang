using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class InstanceOfNode : Node
    {
        public Node AtomNode { get; private set; }

        public InstanceOfNode(Node atomNode, Token typeToken) : base(typeToken)
        {
            AtomNode = atomNode;
        }
    }
}

