using Gaooo;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace GaoooRuntime
{
    public class GameText
    {
        public string Text { get; set; } = string.Empty;

        public int MaxNumberOfChara = 25;
        public int MaxNumberOfLines = 3;
        public double ScrollSpeed = 0.1;
        public double ScrollRate { get { return ScrollSpeed > 0.0 ? _scrollTimer / ScrollSpeed : 1.0; } }
        public List<GameTextLink> Links { get; private set; } = new();
        public int SelectedLinkIndex = 0;
        public bool SelectedLinkIsValid { get { return SelectedLinkIndex >= 0 && SelectedLinkIndex < Links.Count; } }
        public GameTextLink SelectedLink { get { return Links[SelectedLinkIndex]; } }

        private List<string> _lines = new();
        private double _timer = 0.0;
        private double _scrollTimer = 0.0;
        private int _linkColumns = 1;
        private GameTextLink _work = null;

        public void Add(char c)
        {
            if (_work != null)
            {
                _work.Text += c;
            }

            if (_lines.Count == 0)
            {
                _lines.Add(string.Empty);
            }

            var s = _lines.Last();

            switch (c)
            {
                case '\n':
                    if (s.Length > 0)
                    {
                        _lines.Add(string.Empty);
                    }
                    return;
                case '。':
                case '、':
                case '！':
                    if (s.Length == 0)
                    {
                        return;
                    }
                    break;
                default:
                    break;
            }

            s += c.ToString();
            _lines[_lines.Count - 1] = s;
            if (s.Length == MaxNumberOfChara)
            {
                _lines.Add(string.Empty);
            }
        }

        public void Clear()
        {
            _lines.Clear();
            Links.Clear();
        }

        public bool NeedScroll()
        {
            return _lines.Count > MaxNumberOfLines || (_lines.Count >= MaxNumberOfLines && _lines.Last().Length >= MaxNumberOfChara);
        }

        public void Update(double dt, Point posInScreen, GaoooTag currentTag)
        {
            _timer += dt;
            var timeInMillisec = (int)(_timer * 1000);

            if (NeedScroll())
            {
                _scrollTimer += dt;
                if (_scrollTimer >= ScrollSpeed)
                {
                    _lines.RemoveAt(0);
                    _scrollTimer = 0.0;

                    foreach (var l in Links)
                    {
                        l.Line -= 1;
                    }
                }

                SelectedLinkIndex = -1;
            }
            else
            {
                _scrollTimer = 0.0;
                SelectedLinkIndex = GetSelectedIndexFromPos(posInScreen);
            }

            var s = "";
            var lastLine = _lines.Count - 1;
            for (var line = 0; line <= lastLine; ++line)
            {
                var lineText = _lines[line];
                if (SelectedLinkIsValid && line == SelectedLink.Line)
                {
                    if ((timeInMillisec % 200) < 100)
                    {
                        if (currentTag.Name == "endbranch")
                        {
                            lineText = lineText.Substring(0, SelectedLink.Pos - 1) +
                                "◎" + lineText.Substring(SelectedLink.Pos);
                        }
                    }
                }

                s += lineText;

                if (line == lastLine)
                {
                    if ((timeInMillisec % 200) < 100)
                    {
                        if (currentTag.Name == "l")
                        {
                            s += "◇";
                        }
                        else if (currentTag.Name == "p")
                        {
                            s += "▽";
                        }
                    }
                }

                s += "\n";
            }

            Text = s;
        }

        public void BranchBegin(GaoooTag tag)
        {
            Clear();
            _linkColumns = tag.GetAttrValue("col", 1);
        }

        public void LinkBegin(GaoooTag tag)
        {
            if (Links.Count % _linkColumns == 0)
            {
                Add('\n');
            }
            Add('　');
            if (Links.Count % _linkColumns != 0)
            {
                Add('　');
            }
            _work = new GameTextLink(Links.Count, _lines.Count - 1, _lines.Last().Length, tag);
            Links.Add(_work);
        }

        public void LinkEnd(GaoooTag tag)
        {
            _work = null;
        }

        public int GetSelectedIndexFromPos(Point pos)
        {
            var x = (int)pos.X;
            var y = (int)pos.Y;
            var line = 0;
            var posInLine = 0;
            var lineCount = 0;
            if (lineCount == 0)
            {
                return 0;
            }

            foreach (var l in _lines)
            {
                if (y < lineCount + 16)
                {
                    break;
                }
                lineCount += 16;
                line += 1;
            }
            var s = _lines[line];
            var charCount = 0;
            foreach (var c in s)
            {
                if (x < charCount + 16)
                {
                    break;
                }
                charCount += 16;
                posInLine += 1;
            }
            var index = Links.FindIndex(x => x.Line == line && x.Pos == posInLine);
            return index;
        }
    }
}
