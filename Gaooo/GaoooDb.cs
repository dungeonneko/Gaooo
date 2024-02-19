using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Gaooo
{
    internal class GaoooDb
    {
        public GaoooSystem Sys { get; init; }
        JsonDocument? _db = null;

        public GaoooDb(GaoooSystem sys)
        {
            Sys = sys;
        }

        public void Load(GaoooFilePath path)
        {
            var stream = new FileStream(path.AbsPath, FileMode.Open, FileAccess.Read);
            _db = JsonDocument.Parse(stream);
        }

        public GaoooValue Get(string key)
        {
            GaoooValue value;
            if (false == TryGet(key, out value))
            {
                Sys.Error("db value not found:" + key);
            }
            return value;
        }

        public bool TryGet(string key, out GaoooValue value)
        {
            if (_db == null)
            {
                value = new GaoooValueNull();
                return false;
            }

            var keys = key.Split('.');
            var elem = _db.RootElement;
            for (var i = 0; i < keys.Length; ++i)
            {
                try
                {
                    elem = elem.GetProperty(keys[i]);
                }
                catch (KeyNotFoundException)
                {
                    value = new GaoooValueNull();
                    return false;
                }
            }
            value = elem.JsonToGaoooValue(true);
            return true;
        }
    }
}
