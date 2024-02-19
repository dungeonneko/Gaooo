using Gaooo;
using System;

namespace GaoooRuntime
{
    public class TaskText : GaoooTask
    {
        internal enum TextState
        {
            Displaying,
            Displayed,
        }

        internal TextState State { get; private set; } = TextState.Displaying;
        internal string Data { get; private set; } = string.Empty;
        internal double MessageWaitSec = 0.015;

        private Func<bool> _onUserInput;
        private double _tempSpeed = 0.0;
        private string _rest = string.Empty;
        private double _wait = 0.0;
        private GameText _message;

        public TaskText(Func<bool> onUserInput, GameText message, GaoooTag tag, string text) : base(tag)
        {
            State = TextState.Displaying;
            Data = text;
            _rest = text;
            _wait = MessageWaitSec;
            _tempSpeed = MessageWaitSec;
            _message = message;
            _onUserInput = onUserInput;
        }

        public override void Update(double dt)
        {
            if (_onUserInput())
            {
                _tempSpeed = 0.0;
            }

            switch (State)
            {
                case TextState.Displaying:
                    _wait -= dt;
                    while (_wait <= 0.0 && !string.IsNullOrEmpty(_rest))
                    {
                        _wait += _tempSpeed;
                        _message.Add(_rest[0]);
                        _rest = _rest.Substring(1);
                    }
                    if (string.IsNullOrEmpty(_rest))
                    {
                        if (State == TextState.Displaying)
                        {
                            State = TextState.Displayed;
                        }
                    }
                    break;
                case TextState.Displayed:
                    break;
            }
        }

        public override bool IsEnd()
        {
            return State == TextState.Displayed;
        }
    }
}
