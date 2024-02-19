using System;

namespace Gaooo
{
    public abstract class GaoooValue
    {
        public abstract override string ToString();
        public abstract GaoooValue Clone();
        public abstract GaoooValue Eval(GaoooSystem sys);
        public abstract object? ToObject();
        public virtual string ToObjectString() { return ToObject()?.ToString() ?? string.Empty; }
    }
}
