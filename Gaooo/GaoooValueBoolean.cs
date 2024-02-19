using System;

namespace Gaooo
{
    internal class GaoooValueBoolean : GaoooValue
    {
        public bool RawValue = false;

        public GaoooValueBoolean(bool value)
        {
            RawValue = value;
        }

        public override GaoooValue Clone()
        {
            return new GaoooValueBoolean(RawValue);
        }

        public override GaoooValue Eval(GaoooSystem sys)
        {
            return this;
        }

        public override string ToString()
        {
            return RawValue.ToString();
        }

        public override object? ToObject()
        {
            return RawValue;
        }
    }
}
