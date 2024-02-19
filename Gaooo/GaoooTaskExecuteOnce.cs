using System;

namespace Gaooo
{
    public class GaoooTaskExecuteOnce : GaoooTask
    {
        private Action _action;

        public GaoooTaskExecuteOnce(GaoooTag tag, Action action) : base(tag)
        {
            _action = action;
        }

        public override void Update(double dt)
        {
            _action();
        }

        public override bool IsEnd()
        {
            return true;
        }
    }
}
