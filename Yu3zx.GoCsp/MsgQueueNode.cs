using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.GoCsp
{
    public class MsgQueueNode<T>
    {
        internal T _value;
        internal MsgQueueNode<T> _next;

        public MsgQueueNode(T value)
        {
            _value = value;
            _next = null;
        }

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }

    public class MsgQueue<T>
    {
        int _count;
        MsgQueueNode<T> _head;
        MsgQueueNode<T> _tail;

        public MsgQueue()
        {
            _count = 0;
            _head = _tail = null;
        }

        public void AddLast(T value)
        {
            AddLast(new MsgQueueNode<T>(value));
        }

        public void AddFirst(T value)
        {
            AddFirst(new MsgQueueNode<T>(value));
        }

        public void AddLast(MsgQueueNode<T> node)
        {
            if (null == _tail)
            {
                _head = node;
            }
            else
            {
                _tail._next = node;
            }
            node._next = null;
            _tail = node;
            _count++;
        }

        public void AddFirst(MsgQueueNode<T> node)
        {
            node._next = _head;
            _head = node;
            if (0 == _count++)
            {
                _tail = node;
            }
        }

        public void RemoveFirst()
        {
            _head = _head._next;
            if (0 == --_count)
            {
                _tail = null;
            }
        }

        public void Clear()
        {
            _count = 0;
            _head = _tail = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            MsgQueueNode<T> it = _head;
            while (null != it)
            {
                yield return it._value;
                it = it._next;
            }
            yield break;
        }

        public T[] ToList
        {
            get
            {
                int idx = 0;
                T[] list = new T[_count];
                MsgQueueNode<T> it = _head;
                while (null != it)
                {
                    list[idx++] = it._value;
                    it = it._next;
                }
                return list;
            }
        }

        public MsgQueueNode<T> First
        {
            get
            {
                return _head;
            }
        }

        public MsgQueueNode<T> Last
        {
            get
            {
                return _tail;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }
    }
}
