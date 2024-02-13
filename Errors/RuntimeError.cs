using System;
using X_Programming_Language.Constants;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Errors
{
    public class RuntimeError: Error
    {
        public RuntimeError(Position positionStart, Position positionEnd, string details, Context? context = null, string errorName = "RuntimeError") : base(positionStart, positionEnd, errorName, details, context)
        {
        }
    }
}

