using System;
using System.Collections.Generic;
using System.Text;

namespace Gaooo
{
    public class GaoooValueLabel : GaoooValue
    {
        public static readonly GaoooValueLabel Empty = new GaoooValueLabel(string.Empty);
        public string RawValue = string.Empty;

        public GaoooValueLabel(string value)
        {
            RawValue = value;
        }

        public override string ToString()
        {
            return "*" + RawValue;
        }

        public override GaoooValue Clone()
        {
            return new GaoooValueLabel(RawValue);
        }

        public override GaoooValue Eval(GaoooSystem sys)
        {
            return this;
        }

        public override object? ToObject()
        {
            return "*" + RawValue;
        }
    }
}
