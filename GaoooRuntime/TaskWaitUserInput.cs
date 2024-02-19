using Gaooo;
using System;

namespace GaoooRuntime
{
    public class TaskWaitUserInput : GaoooTask
    {
        private Func<bool> _onGetClicked;
        private bool _clicked;

        public TaskWaitUserInput(Func<bool> onGetClicked, GaoooTag tag) : base(tag)
        {
            _onGetClicked = onGetClicked;
            _clicked = false;
        }

        public override void Update(double dt)
        {
            _clicked = _onGetClicked();
        }

        public override bool IsEnd()
        {
            return _clicked;
        }
    }
}
