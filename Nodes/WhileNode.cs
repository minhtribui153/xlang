using System;
namespace X_Programming_Language.Nodes
{
    public class WhileNode : Node
    {
        public Node ConditionNode { get; private set; }
        public Node BodyNode { get; private set; }
        public bool ShouldReturnNull { get; private set; }

        public WhileNode(Node conditionNode, Node bodyNode, bool shouldReturnNull) : base(null)
        {
            ConditionNode = conditionNode;
            BodyNode = bodyNode;
            ShouldReturnNull = shouldReturnNull;

            PositionStart = ConditionNode.PositionStart;
            PositionEnd = ConditionNode.PositionEnd;
        }
    }
}

