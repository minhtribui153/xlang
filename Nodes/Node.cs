using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public abstract class Node
    {
        public Token? Token { get; private set; }
        public Position? PositionStart { get; set; }
        public Position? PositionEnd { get; set; }

        public Node(Token? token)
        {
            Token = token;
            if (token != null)
            {
                PositionStart = token.PositionStart;
                PositionEnd = token.PositionEnd;
            }
        }
    }
}

