using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Errors
{
    public class InvalidSyntaxError : Error
    {
        public InvalidSyntaxError(Position positionStart, Position positionEnd, string details, Context? context = null) : base(positionStart, positionEnd, "InvalidSyntaxError", details, context)
        {
        }
    }
}

