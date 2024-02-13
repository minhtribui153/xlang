using System;
using System.Reflection;
using X_Programming_Language.Results;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Values
{
    public class BuiltInFunctionValue : BaseFunctionValue
    {
        public List<Tuple<string, string>> ArgumentNames { get; private set; }
        public Func<List<Value>, RuntimeResult> Callback { get; private set; }

        public BuiltInFunctionValue(string name, List<Tuple<string, string>> argumentNames, Func<List<Value>, RuntimeResult> callback) : base(name)
        {
            ArgumentNames = argumentNames;
            Callback = callback;
        }

        public override RuntimeResult Execute(List<Value> args)
        {
            var res = new RuntimeResult();
            var executableContext = GenerateNewContext();
            res.Register(CheckAndPopulateArguments(ArgumentNames, args, executableContext));
            if (res.Error != null) return res;

            var returnValue = res.Register(Callback(args));
            if (res.Error != null) return res;
            return res.Success(returnValue);
        }
    }
}

