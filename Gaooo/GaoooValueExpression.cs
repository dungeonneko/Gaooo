using System;
using System.Collections.Generic;
using System.Text;

namespace Gaooo
{
    public class GaoooValueExpression : GaoooValue
    {
        public static readonly GaoooValueExpression Empty = new GaoooValueExpression(string.Empty);
        public string RawValue = string.Empty;

        public GaoooValueExpression(string value)
        {
            RawValue = value;
        }

        public override string ToString()
        {
            return "{" + RawValue + "}";
        }

        public override GaoooValue Clone()
        {
            return new GaoooValueExpression(RawValue);
        }

        public override GaoooValue Eval(GaoooSystem sys)
        {
            return new GaoooExpression(sys).EvalStr(RawValue);
        }

        public override object? ToObject()
        {
            return "{" + RawValue + "}";
        }
    }
}
