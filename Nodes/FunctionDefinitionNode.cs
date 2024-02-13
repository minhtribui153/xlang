using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Nodes
{
    public class FunctionDefinitionNode : Node
    {
        public List<Tuple<Token, Token>> ArgumentNameTokens { get; private set; }
        public Node BodyNode { get; private set; }
        public Token? ReturnTypeToken { get; private set; }
        public bool ShouldAutoReturn { get; private set; }

        public FunctionDefinitionNode(Token? variableNameToken, List<Tuple<Token, Token>> argumentNameTokens, Node bodyNode, Token? returnTypeToken, bool shouldAutoReturn) : base(variableNameToken)
        {
            ArgumentNameTokens = argumentNameTokens;
            ReturnTypeToken = returnTypeToken;
            BodyNode = bodyNode;
            ShouldAutoReturn = shouldAutoReturn;

            if (Token != null)
                PositionStart = Token.PositionStart;
            else if (ArgumentNameTokens.Count > 0)
                PositionStart = BodyNode.PositionStart;
            else
                PositionStart = BodyNode.PositionStart;
            PositionEnd = BodyNode.PositionEnd;

        }
    }
}

