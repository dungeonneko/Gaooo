using System;

namespace Gaooo
{
    public class GaoooTransition
    {
        public GaoooTag? Tag = null;
        public double Duration = 0.0;
        public double ElapsedTime = 0.0;
        public double Rate { get { return Duration > 0.0 ? ElapsedTime / Duration : 1.0; } }
        public bool IsEnd { get { return Rate >= 1.0; } }
        public string Method { get { return Tag == null ? "" : Tag.GetAttrValue<string>("method"); } }

        public void Start(GaoooTag tag)
        {
            Tag = tag;
            Duration = tag.GetAttrValue("time", 0.0) * 0.001;
            ElapsedTime = 0.0;
        }

        public void Update(double dt)
        {
            ElapsedTime = Math.Min(ElapsedTime + dt, Duration);            
        }
    }
}
