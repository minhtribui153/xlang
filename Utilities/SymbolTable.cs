using System;
using System.Collections.Immutable;
using System.Reflection;
using X_Programming_Language.Errors;
using X_Programming_Language.Values;

namespace X_Programming_Language.Utilities
{
    public class SymbolTable
    {
        public Dictionary<string, Value> Symbols { get; private set; }
        public ImmutableDictionary<string, Value> ImmutableSymbols { get; private set; }
        public ImmutableDictionary<string, BaseFunctionValue> Functions { get; private set; }
        public ImmutableDictionary<string, TypeValue> Types { get; private set; }
        public SymbolTable? Parent { get; private set; }

        public SymbolTable(SymbolTable? parent = null)
        {
            Symbols = new();
            ImmutableSymbols = new Dictionary<string, Value>().ToImmutableDictionary();
            Functions = new Dictionary<string, BaseFunctionValue>().ToImmutableDictionary();
            Types = new Dictionary<string, TypeValue>().ToImmutableDictionary();
            Parent = parent;
        }

        public Value? Get(string name)
        {
            Value? value = Symbols.ContainsKey(name)
                ? Symbols[name]
                : ImmutableSymbols.ContainsKey(name)
                    ? ImmutableSymbols[name]
                    : Functions.ContainsKey(name)
                        ? Functions[name]
                            : null;

            if (value == null && Parent != null)
            {
                return Parent.Get(name);
            }
            return value;
        }

        /* Sets a Symbol into a Dictionary/Immutable Dictionary. If Symbol == Immutable and Immutable Dictionary contains that Symbol, returns true for error, otherwise false for no error */
        public void Set(string name, Value value)
        {
            if (value.Immutable)
            {
                if (ImmutableSymbols.ContainsKey(name)) return;
                SetImmutableValue(name, value);
            } else
            {
                Symbols[name] = value;
            }
            
        }

        public void SetImmutableValue(string name, Value value)
        {
            Dictionary<string, Value> dict = new();

            foreach (KeyValuePair<string, Value> symbol in ImmutableSymbols) dict.Add(symbol.Key, symbol.Value);

            dict.Add(name, value);

            ImmutableSymbols = dict.ToImmutableDictionary();
        }

        public void Remove(string name)
        {
            Symbols.Remove(name);
            if (ImmutableSymbols.ContainsKey(name)) ImmutableSymbols.Remove(name);
        }

        public TypeValue? GetType(string name)
        {
            TypeValue? typeValue = Types.ContainsKey(name) ? Types[name] : null;

            if (typeValue == null && Parent != null)
            {
                return Parent.GetType(name);
            }

            return typeValue;
        }

        public void SetType(string name, TypeValue typeValue)
        {
            Dictionary<string, TypeValue> dict = new();

            foreach (KeyValuePair<string, TypeValue> type in Types) dict.Add(type.Key, type.Value);

            dict.Add(name, typeValue);

            Types = dict.ToImmutableDictionary();
        }

        public BaseFunctionValue? GetFunction(string name)
        {
            BaseFunctionValue? value = Functions.ContainsKey(name) ? Functions[name] : null;

            if (value == null && Parent != null)
            {
                return Parent.GetFunction(name);
            }

            return value;
        }

        public void SetFunction(string name, BaseFunctionValue functionValue)
        {
            Dictionary<string, BaseFunctionValue> dict = new();

            foreach (KeyValuePair<string, BaseFunctionValue> function in Functions) dict.Add(function.Key, function.Value);

            dict.Add(name, functionValue);

            Functions = dict.ToImmutableDictionary();
        }
    }
}

