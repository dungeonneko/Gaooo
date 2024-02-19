using System;
using System.Collections.Generic;
using System.Text;

namespace Gaooo
{
    public class GaoooValueArgument : GaoooValue
    {
        public static readonly GaoooValueExpression Empty = new GaoooValueExpression(string.Empty);
        public string RawValue = string.Empty;

        public GaoooValueArgument(string value)
        {
            RawValue = value;
        }

        public override string ToString()
        {
            return "$" + RawValue;
        }

        public override GaoooValue Clone()
        {
            return new GaoooValueArgument(RawValue);
        }

        public override GaoooValue Eval(GaoooSystem sys)
        {
            var value = sys.GetArgument(RawValue);
            return value.Eval(sys);
        }

        public override object? ToObject()
        {
            return "$" + RawValue;
        }
    }
}
