using System;
using System.Collections.Generic;
using System.Linq;

namespace Gaooo
{
    public class GaoooLayerObject
    {
        public readonly GaoooTag Tag;        
        public Dictionary<string, GaoooLayerObject> Children { get; private set; } = new();
        private readonly Dictionary<string, GaoooLayerTrack> _tracks = new();
        private readonly Dictionary<string, object?> _properties = new();

        public bool ContainsObject(string key) => Children.ContainsKey(key) || Children.Any(x => x.Value.ContainsObject(key));

        public GaoooLayerObject(GaoooTag tag)
        {
            Tag = tag;
        }

        public GaoooLayerObject Copy()
        {
            var obj = new GaoooLayerObject(Tag);
            foreach (var child in Children)
            {
                obj.Children[child.Key] = child.Value.Copy();
            }
            foreach (var track in _tracks)
            {
                obj._tracks[track.Key] = track.Value.Copy();
            }
            foreach (var prop in _properties)
            {
                obj._properties[prop.Key] = prop.Value;
            }
            return obj;
        }

        public void Update(double dt)
        {
            foreach (var prop in Tag)
            {
                GaoooLayerTrack? track = null;
                if (_tracks.ContainsKey(prop.Key))
                {
                    track = _tracks[prop.Key];
                    track.Update(dt);
                    if (track.HasKey)
                    {
                        _properties[prop.Key] = track.GetValue();
                    }
                }
                else
                {
                    _properties[prop.Key] = Tag.GetAttrValue(prop.Key);
                }
            }

            foreach (var child in Children)
            {
                child.Value.Update(dt);
            }
        }

        public void AddObject(GaoooTag tag)
        {
            var parent = tag.GetAttrValue<string>("parent");
            if (Tag.Id == parent)
            {
                Children.Add(tag.Id, new GaoooLayerObject(tag));
                return;
            }

            if (Children.ContainsKey(parent))
            {
                Children[parent].AddObject(tag);
                return;
            }

            foreach (var c in Children)
            {
                if (c.Value.ContainsObject(parent))
                {
                    AddObject(tag);
                    return;
                }
            }

            throw new Exception("parent not found");
        }

        public void RemoveObject(string id)
        {
            if (Children.ContainsKey(id))
            {
                Children.Remove(id);
                return;
            }

            foreach (var c in Children)
            {
                if (c.Value.ContainsObject(id))
                {
                    RemoveObject(id);
                    return;
                }
            }

            throw new Exception("object not found");
        }

        public GaoooLayerObject? FindObject(string id)
        {
            if (Children.ContainsKey(id))
            {
                return Children[id];
            }

            foreach (var c in Children)
            {
                if (c.Value.ContainsObject(id))
                {
                    return c.Value.FindObject(id);
                }
            }

            throw new Exception("object not found");
        }
        
        public T GetProperty<T>(string key)
        {
            var obj = _properties[key];
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public T GetPropertyOrDefault<T>(string key, T defaultValue)
        {
            if (_properties.ContainsKey(key))
            {
                return GetProperty<T>(key);
            }
            return defaultValue;
        }

        public object? GetProperty(string key)
        {
            return _properties[key];
        }

        public bool HasProperty(string key)
        {
            return _properties.ContainsKey(key);
        }

        public GaoooLayerTrack AddTrack(GaoooTag tag)
        {
            var track = new GaoooLayerTrack(this, tag);
            _tracks[track.Attr] = track;
            return track;
        }
    }
}
