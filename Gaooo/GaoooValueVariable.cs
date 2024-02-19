using System;
using System.Collections.Generic;
using System.Text;

namespace Gaooo
{
    public class GaoooValueVariable : GaoooValue
    {
        public static readonly GaoooValueVariable Empty = new GaoooValueVariable(string.Empty);
        public string RawValue = string.Empty;

        public GaoooValueVariable(string value)
        {
            RawValue = value;
        }

        public override string ToString()
        {
            return RawValue;
        }

        public override GaoooValue Clone()
        {
            return new GaoooValueVariable(RawValue);
        }

        public override GaoooValue Eval(GaoooSystem sys)
        {
            var value = sys.GetVariable(RawValue);
            return value.Eval(sys);
        }

        public override object? ToObject()
        {
            return RawValue;
        }
    }
}
