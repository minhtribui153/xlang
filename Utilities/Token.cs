using System;
namespace X_Programming_Language.Utilities
{
    public class Token
    {
        public string Type { get; private set; }
        public object? Value { get; private set; }
        public Position? PositionStart { get; private set; }
        public Position? PositionEnd { get; private set; }

        public Token(string type, object? value = null, Position? positionStart = null, Position? positionEnd = null)
        {
            Type = type;
            Value = value;

            if (positionStart != null)
            {
                PositionStart = positionStart.Copy();
                PositionEnd = positionStart.Copy();
                PositionEnd.Advance();
            }

            if (positionEnd != null)
            {
                PositionEnd = positionEnd.Copy();
            }
        }

        public bool Matches(string type, object? value = null)
        {
            return Type.Equals(type) && Value!.Equals(value);
        }

        public override string ToString()
        {
            if (Value != null)
            {
                string result = Value.GetType().Equals(typeof(string))
                    ? $"\"{Value}\""
                    : Value.GetType().Equals(typeof(char))
                        ? $"'{Value}'"
                        : $"{Value}";
                return $"[{Type}: {result} | {PositionStart?.Index ?? 0}:{PositionEnd?.Index ?? 0}]";
            }
            return $"[{Type} | {PositionStart?.Index ?? 0}:{PositionEnd?.Index ?? 0}]";
        }
    }
}

