using System;
using X_Programming_Language.Errors;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Values
{
    public class Value
    {
        public object? value { get; private set; }
        public string Type { get; private set; }
        public string DefaultType { get; private set; }
        public bool Immutable { get; private set; }
        public bool CanUseInSafeCode { get; private set; }
        public Position? PositionStart { get; private set; }
        public Position? PositionEnd { get; private set; }
        public Context? Context { get; private set; }
        public Type ValueInstance { get; private set; }

        public Func<object?, string>? RepresentValue { get; private set; }

        public Value(Type valueInstance, object? value, string? type = null, Func<object?, string>? representValue = null, bool immutable = false, bool canUseInSafeCode = true, string? defaultType = null)
        {
            this.value = value;
            Type = type ?? "object";
            DefaultType = defaultType ?? Type;
            SetPosition();
            SetContext();
            RepresentValue = representValue;
            Immutable = immutable;
            CanUseInSafeCode = canUseInSafeCode;
            ValueInstance = valueInstance;
        }

        public Value SetPosition(Position? positionStart = null, Position? positionEnd = null)
        {
            PositionStart = positionStart;
            PositionEnd = positionEnd;
            return this;

        }

        public Value SetDefaultType(string defaultType)
        {
            DefaultType = defaultType;
            return this;
        }

        public Value SetContext(Context? context = null)
        {
            Context = context;
            return this;
        }

        public Value SetImmutable(bool immutable)
        {
            Immutable = immutable;
            return this;
        }

        public virtual Value Copy()
        {
            var copy = new Value(ValueInstance, value, Type, RepresentValue, Immutable, CanUseInSafeCode, DefaultType);
            copy.SetPosition(PositionStart, PositionEnd);
            copy.SetContext(Context);
            return copy;
        }

        public virtual Tuple<Value?, Error?> AddedTo(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> SubtractedBy(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> MultipliedBy(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> DividedBy(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> PoweredBy(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> ModulusOf(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public Tuple<Value, Error?> IsEqualTo(Value other)
        {
            if (value == null && other.value == null) return new(new BoolValue(true), null);
            else if (value == null || other.value == null) return new(new BoolValue(false), null);
            return new(new BoolValue(value.Equals(other.value)), null);
        }

        public Tuple<Value, Error?> IsNotEqualTo(Value other)
        {
            var val = IsEqualTo(other);
            if ((bool)val.Item1.value!) return new(new BoolValue(false), null);
            return new(new BoolValue(true), null);
        }

        public virtual Tuple<Value?, Error?> IsLesserThan(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> IsGreaterThan(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> IsLesserThanOrEqualTo(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> IsGreaterThanOrEqualTo(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> GetValueByKey(Value key)
        {
            return new(null, new IllegalOperationError(key.PositionStart!, key.PositionEnd!, $"Cannot get value attr from '{Type}' type", Context));
        }

        public virtual Tuple<List<Value>?, Error?> GetAllValues()
        {
            return new(null, new IllegalOperationError(PositionStart!, PositionEnd!, $"Cannot get value attr from '{Type}' type", Context));
        }

        public virtual Tuple<int, Error?> CountValues()
        {
            return new(0, new IllegalOperationError(PositionStart!, PositionEnd!, $"Cannot count values from '{Type}' type", Context));
        }

        public virtual Tuple<Value?, Error?> HandleAnd(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> HandleOr(Value other)
        {
            return HandleIllegalOperationForOtherValue(other);
        }

        public virtual Tuple<Value?, Error?> HandleNot()
        {
            return new(null, new IllegalOperationError(PositionStart!, PositionEnd!, "Illegal operation"));
        }

        private Tuple<Value?, Error?> HandleIllegalOperationForOtherValue(Value other)
        {
            return new(null, new IllegalOperationError(other.PositionStart!, other.PositionEnd!, "Illegal operation", other.Context));
        }

        public override string ToString()
        {
            return RepresentValue != null ? RepresentValue(value) : $"{value}";
        }
    }
}

