using System.Collections.Generic;
using System.Drawing;
using System;
using System.Linq;

namespace Gaooo
{
    public class GaoooValueList : GaoooValue
    {
        public Deque<GaoooValue> RawValue = new Deque<GaoooValue>();
        public int Count { get { return RawValue.Count; } }

        public GaoooValueList()
        {
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", RawValue.Select(x => x.ToString())) + "]";
        }

        public override GaoooValue Clone()
        {
            return CloneDerived();
        }

        public GaoooValueList CloneDerived()
        {
            var list = new GaoooValueList();
            foreach (var item in RawValue)
            {
                list.RawValue.PushBack(item.Clone());
            }
            return list;
        }

        public override GaoooValue Eval(GaoooSystem sys)
        {
            return this;
        }

        public override object? ToObject()
        {
            var list = new Deque<object>();
            foreach (var item in RawValue)
            {
                list.PushBack(item);
            }
            return list;
        }

        public void PushFront(GaoooValue item)
        {
            RawValue.PushFront(item);
        }

        public void PushBack(GaoooValue item)
        {
            RawValue.PushBack(item);
        }

        public GaoooValue PopFront()
        {
            return RawValue.PopFront();
        }

        public GaoooValue PopBack()
        {
            return RawValue.PopBack();
        }

        public GaoooValue RemoveAt(int index)
        {
            return RawValue.RemoveAt(index);
        }

        public GaoooValueList Shuffle()
        {
            var list = CloneDerived();
            list.RawValue.Shuffle();
            return list;
        }
    }
}
