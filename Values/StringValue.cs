using System;
using System.Xml.Linq;
using X_Programming_Language.Errors;

namespace X_Programming_Language.Values
{
    public class StringValue : Value
    {
        public StringValue(string value) : base(typeof(StringValue), value, "string", _ => $"\"{value}\"")
        {
        }

        public override Tuple<Value?, Error?> AddedTo(Value other)
        {
            if (other is StringValue _other)
                return new(new StringValue((string)value! + (string)_other.value!).SetContext(Context), null);
            return base.AddedTo(other);
        }

        public override Tuple<Value?, Error?> IsGreaterThan(Value other)
        {
            if (other is StringValue _other)
            {
                int comparison = string.Compare((string)value!, (string)_other.value!, comparisonType: StringComparison.Ordinal);
                return new(new BoolValue(comparison > 0).SetContext(Context), null);
            }
            return base.IsGreaterThan(other);
        }

        public override Tuple<Value?, Error?> IsLesserThan(Value other)
        {
            if (other is StringValue _other)
            {
                int comparison = string.Compare((string)value!, (string)_other.value!, comparisonType: StringComparison.Ordinal);
                return new(new BoolValue(comparison < 0).SetContext(Context), null);
            }
            return base.IsLesserThan(other);
        }

        public override Tuple<Value?, Error?> IsGreaterThanOrEqualTo(Value other)
        {
            if (other is StringValue _other)
            {
                int comparison = string.Compare((string)value!, (string)_other.value!, comparisonType: StringComparison.Ordinal);
                return new(new BoolValue(comparison >= 0).SetContext(Context), null);
            }
            return base.IsGreaterThanOrEqualTo(other);
        }

        public override Tuple<Value?, Error?> IsLesserThanOrEqualTo(Value other)
        {
            if (other is StringValue _other)
            {
                int comparison = string.Compare((string)value!, (string)_other.value!, comparisonType: StringComparison.Ordinal);
                return new(new BoolValue(comparison <= 0).SetContext(Context), null);
            }
            return base.IsLesserThanOrEqualTo(other);
        }

        public override Tuple<Value?, Error?> GetValueByKey(Value key)
        {
            if (key is IntValue)
            {
                if ((int)key.value! < 0 || (int)key.value >= ((string)value!).Length)
                    return new(null, new RuntimeError(key.PositionStart!, key.PositionEnd!, $"Index out of bounds", key.Context));
                return new(new StringValue($"{((string)value!)[(int)key.value]}"), null);
            }
            return new(null, new InvalidTypeError(key.PositionStart!, key.PositionEnd!, $"Index must be 'int', not '{key.Type}'", key.Context));
        }

        public override Tuple<List<Value>?, Error?> GetAllValues()
        {
            List<Value> values = new();

            foreach (char c in (string)value!)
            {
                var val = new StringValue($"{c}");
                val.SetContext(Context);
                values.Add(val);
            }


            return new(values, null);
        }

        public override Tuple<int, Error?> CountValues()
        {
            return new(((string)value!).Length, null);
        }

        public override StringValue Copy()
        {
            var copy = new StringValue((string)value!);
            copy.SetPosition(PositionStart, PositionEnd);
            copy.SetContext(Context);
            return copy;
        }
    }
}

