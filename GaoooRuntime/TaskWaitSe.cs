using Gaooo;
using System;

namespace GaoooRuntime
{
    internal class TaskWaitSe : GaoooTask
    {
        private double _elapsed = 0;
        private double _timeout = 0;
        private GaoooTag _tag = null;
        private Func<GaoooTag, bool> _onWaitSe = null;

        internal TaskWaitSe(Func<GaoooTag, bool> onWaitSe, GaoooTag tag) : base(tag)
        {
            if (tag.ContainsKey("time"))
            {
                _timeout = tag.GetAttrValue("time", 0.0) / 1000.0;
            }

            _tag = tag;
            _onWaitSe = onWaitSe;
        }

        public override void Update(double dt)
        {
            _elapsed += dt;
        }

        public override bool IsEnd()
        {
            if (_timeout > 0.0 && _elapsed >= _timeout)
            {
                return true;
            }

            return _onWaitSe(_tag);
        }
    }
}
