using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class VariableAccessNode : Node
    {
        public Token VariableNameToken { get; private set; }
        public VariableAccessNode(Token variableNameToken) : base(null)
        {
            VariableNameToken = variableNameToken;
            PositionStart = variableNameToken.PositionStart;
            PositionEnd = variableNameToken.PositionEnd;
        }

        public override string ToString()
        {
            return $"[var_access: \"{VariableNameToken.Value}\"]";
        }
    }
}

