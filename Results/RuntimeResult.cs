using X_Programming_Language.Errors;
using X_Programming_Language.Values;

namespace X_Programming_Language.Results
{
    public class RuntimeResult
    {
        public Value? Value { get; private set; }
        public Error? Error { get; private set; }
        public Value? FunctionReturnValue { get; private set; }
        public bool LoopShouldContinue { get; private set; }
        public bool LoopShouldBreak { get; private set; }

        public RuntimeResult()
        {
            Reset();
        }

        public void Reset()
        {
            Value = null;
            Error = null;
            FunctionReturnValue = null;
            LoopShouldContinue = false;
            LoopShouldBreak = false;
        }

        public Value Register(RuntimeResult result)
        {
            if (result.Error != null) Error = result.Error;
            FunctionReturnValue = result.FunctionReturnValue;
            LoopShouldContinue = result.LoopShouldContinue;
            LoopShouldBreak = result.LoopShouldBreak;
            return result.Value!;
        }

        public RuntimeResult Success(Value value)
        {
            Reset();
            Value = value;
            return this;
        }

        public RuntimeResult SuccessReturn(Value value)
        {
            Reset();
            FunctionReturnValue = value;
            return this;
        }

        public RuntimeResult SuccessContinue()
        {
            Reset();
            LoopShouldContinue = true;
            return this;
        }

        public RuntimeResult SuccessBreak()
        {
            Reset();
            LoopShouldBreak = true;
            return this;
        }

        public RuntimeResult Failure(Error error)
        {
            Reset();
            Error = error;
            return this;
        }

        public bool ShouldReturn()
        {
            return (
                Error != null ||
                FunctionReturnValue != null ||
                LoopShouldContinue ||
                LoopShouldBreak
            );
        }
    }
}

