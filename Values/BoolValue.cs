using System;
using X_Programming_Language.Errors;

namespace X_Programming_Language.Values
{
    public class BoolValue: Value
    {
        public BoolValue(bool value) : base(typeof(BoolValue), value, "bool", _ => value ? "true" : "false")
        {
        }

        public bool IsTrue()
        {
            return (bool)value!;
        }

        public override Tuple<Value?, Error?> HandleAnd(Value other)
        {
            if (other is BoolValue)
            {
                return new(new BoolValue((bool) value! && (bool) other.value!), null);
            }
            return base.HandleAnd(other);
        }

        public override Tuple<Value?, Error?> HandleOr(Value other)
        {
            if (other is BoolValue)
            {
                return new(new BoolValue((bool)value! || (bool)other.value!), null);
            }
            return base.HandleAnd(other);
        }

        public override Tuple<Value?, Error?> HandleNot()
        {
            return new(new BoolValue(!(bool)value!), null);
        }
    }
}

