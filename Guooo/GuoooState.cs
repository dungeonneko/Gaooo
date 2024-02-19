using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Guooo
{
    internal class GuoooState
    {
        public int Duration { get; private set; }
        public bool Loop { get; private set; }
        public string NextState { get; private set; }
        public Dictionary<string, GuoooTrack> Tracks { get; private set; }
        public Dictionary<string, object?> Properties { get; private set; }
        public double TimeInMilliSeconds { get; private set; }
        public bool IsEnd { get { return TimeInMilliSeconds >= Duration && !Loop; } }

        public GuoooState()
        {
            Duration = 0;
            Loop = false;
            NextState = string.Empty;
            Tracks = new Dictionary<string, GuoooTrack>();
            Properties = new Dictionary<string, object?>();
            TimeInMilliSeconds = 0.0;
        }

        public GuoooState(XmlNode node)
        {
            Duration = node.Attributes.GetOrDefault("duration", 0, int.Parse);
            Loop = node.Attributes.GetOrDefault("loop", false, bool.Parse);
            NextState = node.Attributes.GetOrDefault("next", string.Empty, x => x);
            Tracks = new Dictionary<string, GuoooTrack>();
            Properties = new Dictionary<string, object?>();
            foreach (XmlElement c in node.ChildNodes)
            {
                var key = c.Attributes["name"].Value;
                Tracks[key] = new GuoooTrack(c);
            }
            TimeInMilliSeconds = 0.0;
        }

        public void Update(double dt)
        {
            foreach (var track in Tracks)
            {
                track.Value.Update(dt);
                Properties[track.Key] = track.Value.GetValue();
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

        public T GetProperty<T>(string key, T defaultValue)
        {
            if (Properties.ContainsKey(key))
            {
                var p = Properties[key];
                return p != null ? (T)p : defaultValue;
            }
            return defaultValue;
        }
    }
}
