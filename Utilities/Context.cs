using System;
namespace X_Programming_Language.Utilities
{
    public class Context
    {
        public string DisplayName { get; private set; }
        public Context? Parent { get; private set; }
        public Position? ParentEntryPosition { get; private set; }
        public SymbolTable? SymbolTable { get; set; }
        public bool IsInLoop { get; private set; }
        public bool IsInSwitch { get; private set; }
        public bool IsInFunction { get; private set; }

        public Context(string displayName, Context? parent = null, Position? parentEntryPosition = null)
        {
            DisplayName = displayName;
            Parent = parent;
            ParentEntryPosition = parentEntryPosition;
            SymbolTable = null;
            IsInLoop = false;
            IsInSwitch = false;
            IsInFunction = false;
        }

        public void SetIsInLoop(bool value) { IsInLoop = value; }
        public void SetIsInSwitch(bool value) { IsInSwitch = value; }
        public void SetIsInFunction(bool value) { IsInFunction = value; }
    }
}

