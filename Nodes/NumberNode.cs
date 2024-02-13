using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class NumberNode : Node
    {

        public NumberNode(Token token) : base(token)
        {
            
        }

        public override string ToString()
        {
            return $"{Token}";
        }
    }
}

