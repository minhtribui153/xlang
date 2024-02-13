using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class SwitchNode : Node
    {
        public Node MainExpression { get; private set; }
        public List<Tuple<Node, Node, bool>> Cases { get; private set; }
        public Tuple<Node, bool>? DefaultCase { get; private set; }

        public SwitchNode(Node mainExpression, List<Tuple<Node, Node, bool>> cases, Tuple<Node, bool>? defaultCase) : base(null)
        {
            MainExpression = mainExpression;
            Cases = cases;
            DefaultCase = defaultCase;
        }
    }
}

