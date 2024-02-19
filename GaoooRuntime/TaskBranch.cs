using Gaooo;
using System.Linq;

namespace GaoooRuntime
{
    internal class TaskBranch : GaoooTask
    {
        private GameInput _input;
        private GameText _text;
        private bool _clicked;

        internal TaskBranch(GameInput input, GameText text, GaoooTag tag) : base(tag)
        {
            _input = input;
            _text = text;
            _text.SelectedLinkIndex = 0;
            _clicked = false;
        }

        public override void Update(double dt)
        {
            //_text.GetSelectedIndexFromPos(_input.MousePosition);

            _clicked = _input.GetKeyDown(GameInput.Button.OK);

            if (_input.GetKeyDown(GameInput.Button.Up))
            {
                var cur = _text.SelectedLink;
                var objs = _text.Links.Where(x => x.Pos == cur.Pos && x.Line < cur.Line)
                    .OrderByDescending(x => x.Line).ToList();
                if (objs.Count > 0)
                {
                    _text.SelectedLinkIndex = objs[0].Index;
                }
            }
            else if (_input.GetKeyDown(GameInput.Button.Down))
            {
                var cur = _text.SelectedLink;
                var objs = _text.Links.Where(x => x.Pos == cur.Pos && x.Line > cur.Line)
                    .OrderBy(x => x.Line).ToList();
                if (objs.Count > 0)
                {
                    _text.SelectedLinkIndex = objs[0].Index;
                }
            }
            else if (_input.GetKeyDown(GameInput.Button.Left))
            {
                var cur = _text.SelectedLink;
                var objs = _text.Links.Where(x => x.Line == cur.Line && x.Pos < cur.Pos)
                    .OrderByDescending(x => x.Pos).ToList();
                if (objs.Count > 0)
                {
                    _text.SelectedLinkIndex = objs[0].Index;
                }
            }
            else if (_input.GetKeyDown(GameInput.Button.Right))
            {
                var cur = _text.SelectedLink;
                var objs = _text.Links.Where(x => x.Line == cur.Line && x.Pos > cur.Pos)
                    .OrderBy(x => x.Pos).ToList();
                if (objs.Count > 0)
                {
                    _text.SelectedLinkIndex = objs[0].Index;
                }
            }
        }

        public override bool IsEnd()
        {
            return _clicked && _text.SelectedLinkIndex >= 0 && _text.SelectedLinkIndex < _text.Links.Count;
        }

        public override GaoooTask NextTask()
        {
            var link = _text.SelectedLink;
            var target = GaoooTarget.FromTargetTag(link.Tag);
            return Sys.Call(Tag, target);
        }
    }
}
