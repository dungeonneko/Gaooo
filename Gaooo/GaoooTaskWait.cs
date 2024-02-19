namespace Gaooo
{
    internal class GaoooTaskWait : GaoooTask
    {
        private double _elapsed = 0;
        private double _time = 0;

        internal GaoooTaskWait(GaoooTag tag) : base(tag)
        {
            _time = tag.GetAttrValue("time", 0.0) / 1000.0;
        }

        public override void Update(double dt)
        {
            _elapsed += dt;
        }

        public override bool IsEnd()
        {
            return _elapsed >= _time;
        }
    }
}
