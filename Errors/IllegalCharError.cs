using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Errors
{
    public class IllegalCharError : Error
    {
        public IllegalCharError(Position positionStart, Position positionEnd,  string details, Context? context = null) : base(positionStart, positionEnd, "IllegalCharacterError", details, context)
        {
        }
    }
}

