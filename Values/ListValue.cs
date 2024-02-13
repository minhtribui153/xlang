using System;
using System.Collections.Generic;
using System.Xml.Linq;
using X_Programming_Language.Errors;
using X_Programming_Language.Nodes;

namespace X_Programming_Language.Values
{
    public class ListValue : Value
    {
        public List<Value> Elements { get; private set; }
        public TypeValue? ElementType { get; private set; }
        public ListValue(TypeValue? elementType, List<Value> elements) : base(typeof(ListValue), null, "list", _ => '[' + string.Join(", ", elements) + ']')
        {
            ElementType = elementType;
            Elements = elements;

        }

        public override Tuple<Value?, Error?> AddedTo(Value other)
        {
            var newList = Elements.ToList();
            newList.Add(other);
            if (ElementType != null && ElementType.GenericTypeName != "object" && other.Type != ElementType.GenericTypeName)
                return new(null, new InvalidTypeError(
                    other.PositionStart!,
                    other.PositionEnd!,
                    $"Invalid type '{other.Type}' (expected type '{ElementType.GenericTypeName}')",
                    other.Context
                ));
            return new(new ListValue(ElementType, newList), null);
        }

        public override Tuple<Value?, Error?> MultipliedBy(Value other)
        {
            if (other is ListValue _other)
            {
                var elementType = _other.ElementType != null ? _other.ElementType.GenericTypeName : "object";
                if (ElementType != null && other.Type != ElementType.GenericTypeName)
                    return new(null, new RuntimeError(
                        other.PositionStart!,
                        other.PositionEnd!,
                        $"List has an invalid element type '{elementType}' (expected type {ElementType.GenericTypeName})",
                        other.Context
                    ));

                var newList = Elements.ToList();

                int indexCount = 0;
                
                foreach (var element in _other.Elements)
                {
                    newList.Add(element);
                    indexCount += 1;
                }
                return new(new ListValue(ElementType, newList), null);
            }
            return base.MultipliedBy(other);
        }

        public override Tuple<Value?, Error?> SubtractedBy(Value other)
        {
            if (other is IntValue)
            {
                var newList = Elements.ToList();
                if ((int)other.value! >= newList.Count || (int)other.value! < 0)
                    return new(null, new RuntimeError(
                        other.PositionStart!,
                        other.PositionEnd!,
                        $"Index {((IntValue)other).value} out of bounds",
                        other.Context
                    ));
                newList.Remove(newList[(int)other.value]);
                return new(new ListValue(ElementType, newList), null);
            }
            return base.SubtractedBy(other);
        }

        public override Tuple<Value?, Error?> GetValueByKey(Value key)
        {
            if (key is IntValue)
            {
                if ((int)key.value! < 0 || (int)key.value >= Elements.Count)
                    return new(null, new RuntimeError(key.PositionStart!, key.PositionEnd!, $"Index out of bounds", key.Context));
                return new(Elements[(int)key.value], null);
            }
            return new(null, new InvalidTypeError(key.PositionStart!, key.PositionEnd!, $"Index must be 'int', not '{key.Type}'", key.Context));
        }

        public override Tuple<int, Error?> CountValues()
        {
            return new(Elements.Count, null);
        }

        public override Tuple<List<Value>?, Error?> GetAllValues()
        {
            return new(Elements, null);
        }

        public override ListValue Copy()
        {
            var copy = new ListValue(ElementType, Elements);
            copy.SetPosition(PositionStart, PositionEnd);
            copy.SetContext(Context);
            return copy;
        }
    }
}

