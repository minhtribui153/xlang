using System;
using X_Programming_Language.Errors;
using X_Programming_Language.Nodes;
using X_Programming_Language.Results;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Values
{
    public abstract class BaseFunctionValue : Value
    {
        public string Name { get; set; }

        public BaseFunctionValue(string name) : base(typeof(FunctionValue), name, "method", _ => $"<method '{name}'>", true)
        {
            Name = name;
        }

        public abstract RuntimeResult Execute(List<Value> args);

        public Context GenerateNewContext()
        {
            var newContext = new Context(Name, Context, PositionStart);
            newContext.SymbolTable = new SymbolTable(newContext.Parent != null && newContext.Parent.SymbolTable != null ? newContext.Parent.SymbolTable : null);
            return newContext;
        }

        public RuntimeResult CheckArguments(List<Tuple<string, string>> argumentNames, List<Value> args)
        {
            var res = new RuntimeResult();

            if (args.Count > argumentNames.Count || args.Count < argumentNames.Count)
                return res.Failure(new RuntimeError(
                    PositionStart!,
                    PositionEnd!,
                    $"Unexpected num of args passed into func '{Name}' (expected {argumentNames.Count}, found {args.Count})",
                    Context
                ));

            for (int i = 0; i < args.Count; i++)
            {
                var arg = argumentNames[i];
                var argValue = args[i];
                if (argValue.Type != arg.Item2 && arg.Item2 != "object")
                    return res.Failure(new RuntimeError(
                        argValue.PositionStart!,
                        argValue.PositionEnd!,
                        $"Invalid type '{argValue.Type}' parsed into arg '{arg.Item1}' (expected type '{arg.Item2}')"
                    ));
            }

            return res.Success(new NullValue());
        }

        public void PopulateArguments(List<Tuple<string, string>> argumentNames, List<Value> args, Context executableContext)
        {
            for (int i = 0; i < args.Count; i++)
            {
                var arg = argumentNames[i];
                var argValue = args[i];
                argValue.SetContext(executableContext);
                executableContext.SymbolTable!.SetImmutableValue(arg.Item1, argValue);
            }
        }

        public RuntimeResult CheckAndPopulateArguments(List<Tuple<string, string>> argumentNames, List<Value> args, Context executableContext)
        {
            var res = new RuntimeResult();
            res.Register(CheckArguments(argumentNames, args));
            if (res.ShouldReturn()) return res;
            PopulateArguments(argumentNames, args, executableContext);
            return res.Success(new NullValue());
        }
    }
}

