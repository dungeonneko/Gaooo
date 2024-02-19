using System.Collections.Generic;

namespace Gaooo
{
    internal class GaoooStackFrame
    {
        public GaoooSystem Sys { get; init; }
        public GaoooTag Caller { get; init; }
        public GaoooTarget Target { get; init; }
        public Dictionary<string, GaoooValue> LocalVariables { get; init; }
        Stack<GaoooTarget> _statements;

        public GaoooStackFrame(GaoooSystem sys, GaoooTag caller, GaoooTarget target) {
            Sys = sys;
            Caller = caller;
            Target = target;
            LocalVariables = new Dictionary<string, GaoooValue>();
            _statements = new Stack<GaoooTarget>();
        }

        public GaoooValue GetArgument(string key)
        {
            return Caller.Eval(key);
        }

        public GaoooValue GetLocalVariable(string key)
        {
            if (LocalVariables.ContainsKey(key))
            {
                return LocalVariables[key];
            }
            else
            {
                Sys.Error("Local variable not found:" + key);
                return new GaoooValueNull();
            }
        }

        public void SetLocalVariable(string key, GaoooValue value)
        {
            LocalVariables[key] = value;
        }

        public void EnterStatement(GaoooTag from)
        {
            _statements.Push(new GaoooTarget(from));
        }

        public GaoooTarget ExitStatement()
        {
            return _statements.Pop();
        }

        public bool InStatement()
        {
            return _statements.Count > 0;
        }
    }
}
