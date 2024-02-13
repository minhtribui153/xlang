using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Errors
{
    public class OverflowError : Error
    {
        public OverflowError(Position positionStart, Position positionEnd, string details, Context? context = null) : base(positionStart, positionEnd, "OverflowError", details, context)
        {
        }
    }
}

