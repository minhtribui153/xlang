using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Errors
{
    public class IllegalOperationError: Error
    {
        public IllegalOperationError(Position positionStart, Position positionEnd, string details, Context? context = null) : base(positionStart, positionEnd, "IllegalOperationError", details, context)
        {
        }
    }
}

