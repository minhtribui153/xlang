using System;
using System.Reflection;
using X_Programming_Language.Errors;
using X_Programming_Language.Nodes;
using X_Programming_Language.Results;
using X_Programming_Language.Utilities;

namespace X_Programming_Language.Values
{
    public class FunctionValue : BaseFunctionValue
    {
        public Node BodyNode { get; set; }
        public List<Tuple<string, string>> ArgumentNames { get; set; }
        public bool ShouldAutoReturn { get; private set; }

        public FunctionValue(string name, Node bodyNode, List<Tuple<string, string>> argumentNames, bool shouldAutoReturn) : base(name)
        {
            BodyNode = bodyNode;
            ArgumentNames = argumentNames;
            ShouldAutoReturn = shouldAutoReturn;
        }

        public override RuntimeResult Execute(List<Value> args)
        {
            var res = new RuntimeResult();
            var interpreter = new Interpreter();
            var executableContext = GenerateNewContext();
            executableContext.SetIsInFunction(true);

            res.Register(CheckAndPopulateArguments(ArgumentNames, args, executableContext));
            if (res.ShouldReturn()) return res;

            var value = res.Register(interpreter.Visit(BodyNode, executableContext));
            if (res.ShouldReturn() && res.FunctionReturnValue == null) return res;

            var returnValue = (ShouldAutoReturn ? value : null) ?? res.FunctionReturnValue ?? new NullValue();
            executableContext.SetIsInFunction(false);
            return res.Success(returnValue);
        }

        public override FunctionValue Copy()
        {
            var copy = new FunctionValue(Name, BodyNode, ArgumentNames, ShouldAutoReturn);
            copy.SetPosition(PositionStart, PositionEnd);
            copy.SetContext(Context);
            return copy;
        }

        private object[] ParseValuesIntoTypeField(Type type, object value)
        {
            var result = new List<object>();
            // Get all the public fields of the MyClass type.
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            // Iterate through the array of fields and print their names and values to the console.
            foreach (FieldInfo field in fields)
            {
                var fieldName = field.Name;
                var fieldValue = field.GetValue(value);
                
            }

            return result.ToArray();
        }
    }
}

