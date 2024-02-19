using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace Gaooo
{
    internal class GaoooBuiltinFunctions
    {
        private GaoooSystem _sys;
        private Dictionary<string, Action<Stack<GaoooValue>>> _functions;

        public GaoooBuiltinFunctions(GaoooSystem sys)
        {
            _sys = sys;
            _functions = new Dictionary<string, Action<Stack<GaoooValue>>>
            {
                { "DB_LOAD", DB_LOAD },
                { "DB_GET", DB_GET },
                { "DB_GET_OR_DEFAULT", DB_GET_OR_DEFAULT },
                { "EVAL_STR", EVAL_STR },
                { "LIST", LIST },
                { "COUNT", COUNT },
                { "SHUFFLE", SHUFFLE },
                { "REMOVE_AT", REMOVE_AT },
                { "POP_BACK", POP_BACK },
                { "POP_FRONT", POP_FRONT },
                { "PUSH_BACK", PUSH_BACK },
                { "PUSH_FRONT", PUSH_FRONT },
                { "RAND", RAND },
            };
        }

        public void Invoke(string name, Stack<GaoooValue> tokens)
        {
            if (_functions.ContainsKey(name))
            {
                _functions[name](tokens);
            }
            else
            {
                _sys.Error("Unknown function:" + name);
            }
        }

        private void DB_LOAD(Stack<GaoooValue> tokens)
        {
            var path = tokens.Pop().ToObjectString();
            _sys.Db.Load(new GaoooFilePath(_sys, path));
        }

        private void DB_GET(Stack<GaoooValue> tokens)
        {
            var key = tokens.Pop().ToObjectString();
            var val = _sys.Db.Get(key);
            if (val is GaoooValueNull)
            {
                _sys.Error("db value not found:" + key);
            }
            tokens.Push(val);
        }

        private void DB_GET_OR_DEFAULT(Stack<GaoooValue> tokens)
        {
            var def = tokens.Pop();
            var key = tokens.Pop().ToObjectString();
            GaoooValue value;
            tokens.Push(_sys.Db.TryGet(key, out value) ? value : def);
        }

        private void EVAL_STR(Stack<GaoooValue> tokens)
        {
            tokens.Push(new GaoooExpression(_sys).EvalStr(tokens.Pop().ToString().Trim('\"')));
        }

        private void LIST(Stack<GaoooValue> tokens)
        {
            tokens.Push(new GaoooValueList());
        }

        private void COUNT(Stack<GaoooValue> tokens)
        {
            var list = _sys.GetVariableAsList(tokens.Pop().ToString());
            if (list != null)
            {
                tokens.Push(new GaoooValueNumber(list.Count));
            }
            else
            {
                tokens.Push(new GaoooValueNull());
            }
        }

        private void SHUFFLE(Stack<GaoooValue> tokens)
        {
            var list = _sys.GetVariableAsList(tokens.Pop().ToString());
            if (list != null)
            {
                tokens.Push(list.Shuffle());
            }
            else
            {
                tokens.Push(new GaoooValueNull());
            }
            
        }

        private void REMOVE_AT(Stack<GaoooValue> tokens)
        {
            var arg = tokens.Pop().Eval(_sys);
            var argNumber = arg as GaoooValueNumber;
            if (argNumber == null)
            {
                _sys.Error("Invalid argument: " + arg + " is not a number");
                tokens.Push(new GaoooValueNull());
                return;
            }

            var index = (int)argNumber.RawValue;
            var list = _sys.GetVariableAsList(tokens.Pop().ToString());
            var value = (GaoooValue)new GaoooValueNull();
            if (list != null)
            {
                value = list.RemoveAt(index);
            }
            tokens.Push(value);
        }

        private void POP_BACK(Stack<GaoooValue> tokens)
        {
            var list = _sys.GetVariableAsList(tokens.Pop().ToString());
            var value = (GaoooValue)new GaoooValueNull();
            if (list != null)
            {
                value = list.PopBack();
            }
            tokens.Push(value);
        }

        private void POP_FRONT(Stack<GaoooValue> tokens)
        {
            var list = _sys.GetVariableAsList(tokens.Pop().ToString());
            var value = (GaoooValue)new GaoooValueNull();
            if (list != null)
            {
                value = list.PopFront();
            }
            tokens.Push(value);
        }

        private void PUSH_BACK(Stack<GaoooValue> tokens)
        {
            var value = tokens.Pop().Eval(_sys);
            var list = _sys.GetVariableAsList(tokens.Pop().ToString());
            if (list != null)
            {
                list.PushBack(value);
                tokens.Push(list);
            }
            else
            {
                tokens.Push(new GaoooValueNull()); 
            }
        }

        private void PUSH_FRONT(Stack<GaoooValue> tokens)
        {
            var value = tokens.Pop().Eval(_sys);
            var list = _sys.GetVariableAsList(tokens.Pop().ToString());
            if (list != null)
            {
                list.PushFront(value);
                tokens.Push(list);
            }            
            else
            {
                tokens.Push(new GaoooValueNull());
            }
        }

        private void RAND(Stack<GaoooValue> tokens)
        {
            tokens.Push(new GaoooValueNumber(new Random().Next()));
        }
    }
}
