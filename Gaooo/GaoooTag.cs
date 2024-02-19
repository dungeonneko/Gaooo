using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gaooo
{
    public class GaoooParams : Dictionary<string, GaoooValue>
    {
        public new GaoooValue this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                base[key] = value;
            }
        }
    }

    public class GaoooTag : GaoooParams
    {
        public bool IsEmpty { get { return Sys == null; } }
        public GaoooSystem? Sys { get; private set; }
        public string Name { get; private set; }
        public int Tab { get; private set; }
        public GaoooFilePath FilePath { get; private set; }
        public int LineNumber { get; private set; }
        public string Id { get { return GetAttrValue<string>("id"); } }
        public string Type { get { return GetAttrValue<string>("type"); } }

        public static readonly GaoooTag Empty = new GaoooTag(null, string.Empty, -1, GaoooFilePath.Empty, -1);

        public GaoooTag(GaoooSystem? sys, string tag, int tab, GaoooFilePath filepath, int lineNumber)
        {
            Sys = sys;
            Name = tag;
            Tab = tab;
            FilePath = filepath;
            LineNumber = lineNumber;
        }

        public GaoooTag(GaoooSystem sys, string tag, int tab, GaoooFilePath filepath, int lineNumber, GaoooParams attrs)
        {
            Sys = sys;
            Name = tag;
            Tab = tab;
            FilePath = filepath;
            LineNumber = lineNumber;
            foreach (var kv in attrs)
            {
                this[kv.Key] = kv.Value;
            }
        }

        public object? GetAttrValue(string key)
        {
            return Eval(key).ToObject();
        }

        public T GetAttrValue<T>(string key)
        {
            var obj = GetAttrValue(key);
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public T GetAttrValue<T>(string key, T defaultValue)
        {
            var obj = GetAttrValue(key);
            if (obj == null)
            {
                return defaultValue;
            }
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public GaoooValue Eval(string key)
        {
            if (!ContainsKey(key) || Sys == null)
            {
                return new GaoooValueNull();
            }
            return this[key].Eval(Sys);
        }

        public GaoooTag GetEvaluatedTag()
        {
            var ret = new GaoooTag(Sys, Name, Tab, FilePath, LineNumber);
            foreach (var kv in this)
            {
                var val = Eval(kv.Key);
                if (val != null)
                {
                    ret[kv.Key] = val;
                }
            }
            return ret;
        }
    }
}
