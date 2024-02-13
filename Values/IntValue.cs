using System;
using X_Programming_Language.Errors;

namespace X_Programming_Language.Values
{
    public class IntValue : Value
    {
        public IntValue(int value) : base(typeof(IntValue), value, "int", _ => $"{value}")
        {
        }

        public override Tuple<Value?, Error?> AddedTo(Value other)
        {
            if (other is IntValue _other) return new(new IntValue((int)value! + (int)_other.value!).SetContext(Context), null);
            else if (other is FloatValue _other2) return new(new FloatValue((int)value! + (float)_other2.value!).SetContext(Context), null);
            return base.AddedTo(other);
        }

        public override Tuple<Value?, Error?> SubtractedBy(Value other)
        {
            if (other is IntValue _other) return new(new IntValue((int)value! - (int)_other.value!).SetContext(Context), null);
            else if (other is FloatValue _other2) return new(new FloatValue((int)value! - (float)_other2.value!).SetContext(Context), null);
            return base.SubtractedBy(other);
        }

        public override Tuple<Value?, Error?> MultipliedBy(Value other)
        {
            if (other is IntValue _other) return new(new IntValue((int)value! * (int)_other.value!).SetContext(Context), null);
            else if (other is FloatValue _other2) return new(new FloatValue((int)value! * (float)_other2.value!).SetContext(Context), null);
            return base.MultipliedBy(other);
        }

        public override Tuple<Value?, Error?> DividedBy(Value other)
        {
            if (other is IntValue || other is FloatValue)
            {
                if ((other is FloatValue && (float)other.value! == 0.0) || (other is IntValue && (int)((IntValue)other).value! == 0)) return new(null, new ZeroError(
                    other.PositionStart!,
                    other.PositionEnd!,
                    "Division by zero",
                    Context
                ));
                if (other is IntValue _other) return new(new IntValue((int)value! / (int)_other.value!).SetContext(Context), null);
                else if (other is FloatValue _other2) return new(new FloatValue((int)value! / (float)_other2.value!).SetContext(Context), null);
            }
            return base.DividedBy(other);
        }

        public override Tuple<Value?, Error?> PoweredBy(Value other)
        {
            if (other is IntValue _other) return new(new IntValue((int) Math.Pow((int)value!, (int)_other.value!)).SetContext(Context), null);
            else if (other is FloatValue _other2) return new(new FloatValue((float) Math.Pow((int)value!, (float)_other2.value!)).SetContext(Context), null);
            return base.PoweredBy(other);
        }

        public override Tuple<Value?, Error?> ModulusOf(Value other)
        {
            if (other is IntValue || other is FloatValue)
            {
                if ((other is FloatValue && (float)other.value! == 0.0) || (other is IntValue && (int)((IntValue)other).value! == 0)) return new(null, new ZeroError(
                    other.PositionStart!,
                    other.PositionEnd!,
                    "Modulo by zero",
                    Context
                ));
                else if (other is IntValue _other) return new(new IntValue((int)value! % (int)_other.value!).SetContext(Context), null);
                else if (other is FloatValue _other2) return new(new FloatValue((int)value! % (float)_other2.value!).SetContext(Context), null);
            }
            return base.ModulusOf(other);
        }

        public override Tuple<Value?, Error?> IsLesserThan(Value other)
        {
            if (other is IntValue || other is FloatValue)
            {
                return new(new BoolValue((int)value! < (other.Type == "float" ? (float)other.value! : (int)other.value!)), null);
            }
            
            return base.IsLesserThan(other);
        }

        public override Tuple<Value?, Error?> IsGreaterThan(Value other)
        {
            if (other is IntValue || other is FloatValue)
            {
                return new(new BoolValue((int)value! > (other.Type == "float" ? (float)other.value! : (int)other.value!)), null);
            }

            return base.IsGreaterThan(other);
        }

        public override Tuple<Value?, Error?> IsLesserThanOrEqualTo(Value other)
        {
            if (other is IntValue || other is FloatValue)
            {
                return new(new BoolValue((int)value! <= (other.Type == "float" ? (float)other.value! : (int)other.value!)), null);
            }

            return base.IsLesserThanOrEqualTo(other);
        }

        public override Tuple<Value?, Error?> IsGreaterThanOrEqualTo(Value other)
        {
            if (other is IntValue || other is FloatValue)
            {
                return new(new BoolValue((int)value! >= (other.Type == "float" ? (float)other.value! : (int)other.value!)), null);
            }

            return base.IsGreaterThanOrEqualTo(other);
        }

        public override IntValue Copy()
        {
            var copy = new IntValue((int) value!);
            copy.SetPosition(PositionStart, PositionEnd);
            copy.SetContext(Context);
            return copy;
        }
    }
}

