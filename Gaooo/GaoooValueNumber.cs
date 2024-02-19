using System;

namespace Gaooo
{
    internal class GaoooValueNumber : GaoooValue
    {
        public double RawValue = 0.0;

        public GaoooValueNumber(double value)
        {
            RawValue = value;
        }

        public override string ToString()
        {
            return RawValue.ToString();
        }

        public override GaoooValue Clone()
        {
            return new GaoooValueNumber(RawValue);
        }

        public override GaoooValue Eval(GaoooSystem sys)
        {
            return this;
        }

        public override object? ToObject()
        {
            return RawValue;
        }
    }
}
