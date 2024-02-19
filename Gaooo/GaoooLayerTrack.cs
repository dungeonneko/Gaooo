using System;
using System.Collections.Generic;

namespace Gaooo
{
    public class GaoooLayerTrack
    {
        public string Attr => Tag.GetAttrValue<string>("attr");
        public double Duration => Tag.GetAttrValue("duration", 0.0);
        public bool Loop => Tag.GetAttrValue("loop", false);
        public bool HasKey => KeyFrames.Count > 0;

        public GaoooLayerObject Object { get; private set; }        
        public GaoooTag Tag { get; private set; }
        public List<GaoooTag> KeyFrames { get; private set; }
        public double TimeInMilliSeconds { get; private set; }
        private string _rawInit;

        public GaoooLayerTrack(GaoooLayerObject obj, GaoooTag tag)
        {
            Object = obj;
            Tag = tag;
            KeyFrames = new List<GaoooTag>();
            _rawInit = obj.GetPropertyOrDefault(Attr, string.Empty);
        }

        public GaoooLayerTrack Copy()
        {
            var copied = new GaoooLayerTrack(Object, Tag);
            copied.KeyFrames = KeyFrames;
            copied.TimeInMilliSeconds = TimeInMilliSeconds;
            copied._rawInit = _rawInit;
            return copied;
        }

        public void Update(double dt)
        {
            TimeInMilliSeconds += dt * 1000.0;
            if (TimeInMilliSeconds > Duration)
            {
                if (Loop && Duration > 0)
                {
                    TimeInMilliSeconds %= Duration;
                }
                else
                {
                    TimeInMilliSeconds = Duration;
                }
            }
        }

        public object? GetValue()
        {
            GaoooTag? prev = null;
            GaoooTag? next = null;
            double prevTime = 0.0;
            double nextTime = 0.0;

            foreach (var kf in KeyFrames)
            {
                prev = next;
                next = kf;
                prevTime = nextTime;
                nextTime = kf.GetAttrValue("time", 0.0);
                if (TimeInMilliSeconds < nextTime)
                {
                    break;
                }
            }

            var rawNext = next?.Eval("value").ToObject() ?? null;
            var rawPrev = prev?.Eval("value").ToObject() ?? null;

            if (Tag.Sys != null && rawPrev != null && rawNext != null)
            {
                var ty = Tag.Sys.GetPropType(Object.Tag.Type, Attr);
                var rate = (TimeInMilliSeconds - prevTime) / (nextTime - prevTime);
                rate = Math.Max(0, Math.Min(rate, 1));
                return Tag.Sys.GetLerpValue<object>(rawPrev, rawNext, ty, rate);
            }
            else if (Tag.Sys != null && rawNext != null)
            {
                var ty = Tag.Sys.GetPropType(Object.Tag.Type, Attr);
                var rate = (TimeInMilliSeconds - prevTime) / (nextTime - prevTime);
                rate = Math.Max(0, Math.Min(rate, 1));
                return Tag.Sys.GetLerpValue<object>(_rawInit, rawNext, ty, rate);
            }
            else
            {
                return Object.GetProperty(Attr);
            }
        }
    }
}
