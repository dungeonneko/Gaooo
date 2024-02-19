using System;

namespace Gaooo
{
    public class GaoooScenario : IDisposable
    {
        public GaoooSystem Sys { get; private set; }
        public GaoooFilePath FilePath { get { return _stream.FilePath; } }
        public string Label { get; private set; }
        public int LineNumber { get { return _stream.LineNumber; } }
        public bool IsEnd { get { return _stream.EndOfStream; } }
        private GaoooScenarioStream _stream;

        public GaoooScenario(GaoooSystem sys)
        {
            Sys = sys;
            Label = string.Empty;
            _stream = new GaoooScenarioStream(sys);
        }

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
            }
        }

        public void Jump(GaoooTarget target, bool procLine)
        {
            var nextFilePath = _stream.FilePath;
            if (!target.FilePath.IsEmpty)
            {
                nextFilePath = target.FilePath;
            }

            _stream = new GaoooScenarioStream(Sys, nextFilePath);
            Label = target.Label;

            if (!string.IsNullOrEmpty(target.Label) && target.LineNumber < 0)
            {
                _stream.SkipTo(target.Label);
            }
            else
            {
                _stream.SkipTo(target.LineNumber, procLine);
            }
        }

        public void EndIf(GaoooTag tag)
        {
            _stream.SkipTab();

            while (true) {
                (var nextTab, var tags) = _stream.Peek();
                var nextTag = tags.Dequeue().Name;
                if (nextTab < tag.Tab)
                {
                    break;
                }
                if (nextTab == tag.Tab && nextTag != "elif" && nextTag != "else")
                {
                    break;
                }
                _stream.Pop();
                _stream.SkipTab();
            }
        }

        public void NextIf()
        {
            _stream.SkipTab();
        }

        public void SkipTab()
        {
            _stream.SkipTab();
        }

        public GaoooTag? Update()
        {
            return _stream.Pop();
        }
    }
}
