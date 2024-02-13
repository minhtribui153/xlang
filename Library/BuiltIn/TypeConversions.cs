using System;
using X_Programming_Language.Results;
using X_Programming_Language.Values;

namespace X_Programming_Language.Library.BuiltIn
{
    public class TypeConversions
    {
        public static RuntimeResult ParseString(List<Value> parameters)
        {
            return new RuntimeResult().Success(new StringValue($"{parameters[0].value}").SetContext(parameters[0].Context).SetPosition(parameters[0].PositionStart, parameters[0].PositionEnd));
        }
    }
}

