using System;
using X_Programming_Language.Results;
using X_Programming_Language.Utilities;
using X_Programming_Language.Values;

namespace X_Programming_Language.Library.BuiltIn
{
    public class BuiltInFunctions
    {
        public static List<string> UserProgramInputs = new();

        public static RuntimeResult PrintLn(List<Value> parameters)
        {
            Console.WriteLine(parameters[0] is StringValue ? parameters[0].value : parameters[0]);
            return new RuntimeResult().Success(new NullValue());
        }

        public static RuntimeResult Input(List<Value> parameters)
        {
            Console.Write(parameters[0].value);
            var value = UtilFunctions.Input(UserProgramInputs);
            return new RuntimeResult().Success(new StringValue(value));
        }
    }
}

