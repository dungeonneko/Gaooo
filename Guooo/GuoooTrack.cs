using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Guooo
{
    internal class GuoooTrack
    {
        public string TypeName { get; init; }
        public int Duration { get; init; }
        public bool Loop { get; init; }
        public List<GuoooKeyFrame> KeyFrames { get; init; }
        public double TimeInMilliSeconds { get; private set; }

        public GuoooTrack(XmlNode node)
        {
            TypeName = node.Attributes.GetOrDefault("type", "string", x => x);
            Loop = node.Attributes.GetOrDefault("loop", false, bool.Parse);
            KeyFrames = new List<GuoooKeyFrame>();
            foreach (XmlElement c in node.ChildNodes)
            {
                KeyFrames.Add(new GuoooKeyFrame(TypeName, c));
            }
            Duration = KeyFrames.Last().TimeInMillisec;
            TimeInMilliSeconds = 0.0;
        }

        public void Update(double dt)
        {
            if (Duration == 0)
            {
                TimeInMilliSeconds = 0;
                return;
            }

            TimeInMilliSeconds += dt * 1000.0;
            if (Loop)
            {
                TimeInMilliSeconds %= Duration;
            }
            else
            {
                TimeInMilliSeconds = Math.Min(TimeInMilliSeconds, Duration);
            }
        }

        public object? GetValue()
        {
            GuoooKeyFrame? prev = null;
            GuoooKeyFrame? next = null;
            double prevTime = 0.0;
            double nextTime = 0.0;

            foreach (var kf in KeyFrames)
            {
                prev = next;
                next = kf;
                prevTime = nextTime;
                nextTime = kf.TimeInMillisec;
                if (TimeInMilliSeconds < nextTime)
                {
                    break;
                }
            }

            if (prev == null && next != null)
            {
                prev = next;
                next = null;
            }

            if (prev != null && next != null)
            {
                var rawNext = next.Value;
                var rawPrev = prev.Value;
                var rate = (TimeInMilliSeconds - prevTime) / (nextTime - prevTime);
                rate = Math.Max(0, Math.Min(rate, 1));

                switch (TypeName)
                {
                    case "float":
                        {
                            var lhs = (float)rawPrev.Value;
                            var rhs = (float)rawNext.Value;
                            return (float)(lhs + (rhs - lhs) * rate);
                        }
                    case "int":
                        {
                            var lhs = (float)rawPrev.Value;
                            var rhs = (float)rawNext.Value;
                            return (int)(lhs + (rhs - lhs) * rate);
                        }
                    default:
                        break;
                }
            }

            if (prev != null)
            {
                return prev.Value.Value;
            }

            return null;
        }
    }
}
