using System;
namespace X_Programming_Language.Nodes
{
    public class IfNode : Node
    {
        public List<Tuple<Node, Node, bool>> Cases { get; private set; }
        public Tuple<Node, bool>? ElseCase { get; private set; }

        public IfNode(List<Tuple<Node, Node, bool>> cases, Tuple<Node, bool>? elseCase) : base(null)
        {
            Cases = cases;
            ElseCase = elseCase;

            PositionStart = Cases[0].Item1.PositionStart;
            PositionEnd = (ElseCase != null ? ElseCase.Item1 : Cases[^1].Item1).PositionEnd;
        }
    }
}

