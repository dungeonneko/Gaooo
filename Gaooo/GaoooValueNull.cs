namespace Gaooo
{
    internal class GaoooValueNull : GaoooValue
    {
        public override GaoooValue Clone()
        {
            return new GaoooValueNull();
        }

        public override GaoooValue Eval(GaoooSystem sys)
        {
            return this;
        }

        public override string ToString()
        {
            return string.Empty;
        }

        public override object? ToObject()
        {
            return null;
        }

        public override string ToObjectString()
        {
            return string.Empty;
        }
    }
}
