using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Gaooo
{
    public class Deque<T> : LinkedList<T>
    {
        public void PushFront(T item)
        {
            AddFirst(item);
        }

        public void PushBack(T item)
        {
            AddLast(item);
        }

        public T PopFront()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Deque is empty.");
            }
            var item = First.Value;
            RemoveFirst();
            return item;
        }

        public T PopBack()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Deque is empty.");
            }
            var item = Last.Value;
            RemoveLast();
            return item;
        }

        public T RemoveAt(int index)
        {
            var e = GetEnumerator();
            for (int i = 0; i <= index && e.MoveNext(); i++)
            {
                if (i == index)
                {
                    Remove(e.Current);
                    return e.Current;
                }
            }
            throw new IndexOutOfRangeException();
        }

        public void Shuffle()
        {
            var random = new Random();
            var n = Count;
            while (n > 1)
            {
                var k = random.Next(n--);
                var node = First;
                for (int i = 0; i < k; i++)
                {
                    node = node.Next;
                }
                Remove(node);
                AddLast(node);
            }
        }
    }
}
