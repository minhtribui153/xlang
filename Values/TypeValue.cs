using System;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Values
{
    public class TypeValue : Value
    {
        public string GenericTypeName { get; private set; }
        public Type? GenericType { get; private set; }

        public TypeValue(string genericTypeName, Type? genericType = null) : base(typeof(TypeValue), null, "type", _ => $"<type \"{genericTypeName}\">")
        {
            GenericTypeName = genericTypeName;
            GenericType = genericType;
        }
    }
}

