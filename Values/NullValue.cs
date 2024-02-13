using System;

namespace X_Programming_Language.Values
{
    public class NullValue : Value
    {
        public NullValue() : base(typeof(NullValue), null, "nil", _ => "nil", true, false)
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}

