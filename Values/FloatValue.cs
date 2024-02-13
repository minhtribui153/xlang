using System;
using X_Programming_Language.Errors;

namespace X_Programming_Language.Values
{
    public class FloatValue: Value
    {
        public FloatValue(float value): base(typeof(FloatValue), value, "float", _ => $"{value}")
        {
        }

        public override Tuple<Value?, Error?> AddedTo(Value other)
        {
            if (other is FloatValue || other is IntValue) return new(new FloatValue((float)value! + (float)other.value!).SetContext(Context), null);
            return base.AddedTo(other);
        }

        public override Tuple<Value?, Error?> SubtractedBy(Value other)
        {
            if (other is FloatValue || other is IntValue) return new(new FloatValue((float)value! - (float)other.value!).SetContext(Context), null);
            return base.DividedBy(other);
        }

        public override Tuple<Value?, Error?> MultipliedBy(Value other)
        {
            if (other is FloatValue || other is IntValue) return new(new FloatValue((float)value! * (float)other.value!).SetContext(Context), null);
            return base.MultipliedBy(other);
        }

        public override Tuple<Value?, Error?> DividedBy(Value other)
        {
            if (other is FloatValue || other is IntValue)
            {
                if ((other is FloatValue && (float)other.value! == 0.0) || (other is IntValue && (int)((IntValue)other).value! == 0)) return new(null, new ZeroError(
                    other.PositionStart!,
                    other.PositionEnd!,
                    "Division by zero",
                    Context
                ));
                return new(new FloatValue((float)value! / (float)other.value!).SetContext(Context), null);
            }
            return base.DividedBy(other);
        }

        public override Tuple<Value?, Error?> PoweredBy(Value other)
        {
            if (other is FloatValue || other is IntValue) return new(new FloatValue((float) Math.Pow((float)value!, (float)other.value!)).SetContext(Context), null);
            return base.PoweredBy(other);
        }

        public override Tuple<Value?, Error?> ModulusOf(Value other)
        {
            if (other is FloatValue || other is IntValue)
            {
                if ((other is FloatValue && (float)other.value! == 0.0) || (other is IntValue && (int)((IntValue)other).value! == 0)) return new(null, new ZeroError(
                    other.PositionStart!,
                    other.PositionEnd!,
                    "Modulo by zero",
                    Context
                ));
                return new(new FloatValue((float)value! % (float)other.value!).SetContext(Context), null);
            }
            return base.ModulusOf(other);
        }

        public override Tuple<Value?, Error?> IsLesserThan(Value other)
        {
            if (other is IntValue || other is FloatValue)
            {
                return new(new BoolValue((float)value! < (other.Type == "float" ? (float)other.value! : (int)other.value!)), null);
            }

            return base.IsLesserThan(other);
        }

        public override Tuple<Value?, Error?> IsGreaterThan(Value other)
        {
            if (other is IntValue || other is FloatValue)
            {
                return new(new BoolValue((float)value! > (other.Type == "float" ? (float)other.value! : (int)other.value!)), null);
            }

            return base.IsGreaterThan(other);
        }

        public override Tuple<Value?, Error?> IsLesserThanOrEqualTo(Value other)
        {
            if (other is IntValue || other is FloatValue)
            {
                return new(new BoolValue((float)value! <= (other.Type == "float" ? (float)other.value! : (int)other.value!)), null);
            }

            return base.IsLesserThanOrEqualTo(other);
        }

        public override Tuple<Value?, Error?> IsGreaterThanOrEqualTo(Value other)
        {
            if (other is IntValue || other is FloatValue)
            {
                return new(new BoolValue((float)value! >= (other.Type == "float" ? (float)other.value! : (int)other.value!)), null);
            }

            return base.IsGreaterThanOrEqualTo(other);
        }

        public override FloatValue Copy()
        {
            var copy = new FloatValue((float)value!);
            copy.SetPosition(PositionStart, PositionEnd);
            copy.SetContext(Context);
            return copy;
        }
    }
}

