using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class StringNode : Node
    {

        public StringNode(Token token) : base(token)
        {
            
        }

        public override string ToString()
        {
            return $"[string: \"{Token!.Value}\"]";
        }
    }
}

