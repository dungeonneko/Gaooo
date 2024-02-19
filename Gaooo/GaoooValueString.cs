namespace Gaooo
{
    public class GaoooValueString : GaoooValue
    {
        public static readonly GaoooValueString Empty = new GaoooValueString(string.Empty);
        public string RawValue = string.Empty;

        public GaoooValueString(string value)
        {
            RawValue = value.Replace("\\\"", "\"");
        }

        public override string ToString()
        {
            return "\"" + RawValue.Replace("\"", "\\\"") + "\"";
        }

        public override GaoooValue Clone()
        {
            return new GaoooValueString(RawValue);
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
