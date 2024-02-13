using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Errors
{
    public class NotSupportedError : Error
    {
        public NotSupportedError(Position positionStart, Position positionEnd, string details, Context? context = null) : base(positionStart, positionEnd, "NotSupportedError", details, context)
        {
        }
    }
}

