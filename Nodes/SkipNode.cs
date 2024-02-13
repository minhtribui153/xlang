using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class SkipNode : Node
    {
        public SkipNode(Token currentToken) : base(null)
        {
            PositionStart = currentToken.PositionStart;
            PositionEnd = currentToken.PositionEnd;
        }
    }
}

