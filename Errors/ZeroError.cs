using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Errors
{
    public class ZeroError: Error
    {
        public ZeroError(Position positionStart, Position positionEnd, string details, Context? context = null) : base(positionStart, positionEnd, "ZeroError", details, context)
        {
        }
    }
}

