using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Yu3zx.GoCsp
{
    public class GoMutex
    {
        struct wait_node
        {
            public Action _ntf;
            public long _id;
        }

        SharedStrand _strand;
        LinkedList<wait_node> _waitQueue;
        protected long _lockID;
        protected int _recCount;
        protected bool _mustTick;

        public GoMutex(SharedStrand Strand)
        {
            _strand = Strand;
            _waitQueue = new LinkedList<wait_node>();
            _lockID = 0;
            _recCount = 0;
            _mustTick = false;
        }

        public GoMutex() : this(SharedStrand.DefaultStrand()) { }

        protected virtual void async_lock_(long id, Action ntf)
        {
            if (0 == _lockID || id == _lockID)
            {
                _lockID = id;
                _recCount++;
                ntf();
            }
            else
            {
                _waitQueue.AddLast(new wait_node() { _ntf = ntf, _id = id });
            }
        }

        protected virtual void async_try_lock_(long id, Action<bool> ntf)
        {
            if (0 == _lockID || id == _lockID)
            {
                _lockID = id;
                _recCount++;
                ntf(true);
            }
            else
            {
                ntf(false);
            }
        }

        protected virtual void async_timed_lock_(long id, int ms, Action<bool> ntf)
        {
            if (0 == _lockID || id == _lockID)
            {
                _lockID = id;
                _recCount++;
                ntf(true);
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(_strand);
                LinkedListNode<wait_node> node = _waitQueue.AddLast(new wait_node()
                {
                    _ntf = delegate ()
                    {
                        timer.Cancel();
                        ntf(true);
                    },
                    _id = id
                });
                timer.Timeout(ms, delegate ()
                {
                    _waitQueue.Remove(node);
                    ntf(false);
                });
            }
            else
            {
                _waitQueue.AddLast(new wait_node() { _ntf = () => ntf(true), _id = id });
            }
        }

        protected virtual void async_unlock_(long id, Action ntf)
        {
            Debug.Assert(id == _lockID);
            if (0 == --_recCount)
            {
                if (0 != _waitQueue.Count)
                {
                    _recCount = 1;
                    wait_node queueFront = _waitQueue.First.Value;
                    _waitQueue.RemoveFirst();
                    _lockID = queueFront._id;
                    queueFront._ntf();
                }
                else
                {
                    _lockID = 0;
                }
            }
            ntf();
        }

        protected virtual void async_cancel_(long id, Action ntf)
        {
            if (id == _lockID)
            {
                _recCount = 1;
                async_unlock_(id, ntf);
            }
            else
            {
                for (LinkedListNode<wait_node> it = _waitQueue.Last; null != it; it = it.Previous)
                {
                    if (it.Value._id == id)
                    {
                        _waitQueue.Remove(it);
                        break;
                    }
                }
                ntf();
            }
        }

        internal void async_lock(long id, Action ntf)
        {
            if (_strand.RunningInThisThread())
                if (!_mustTick) async_lock_(id, ntf);
                else _strand.AddLast(() => async_lock_(id, ntf));
            else _strand.Post(() => async_lock_(id, ntf));
        }

        internal void async_try_lock(long id, Action<bool> ntf)
        {
            if (_strand.RunningInThisThread())
                if (!_mustTick) async_try_lock_(id, ntf);
                else _strand.AddLast(() => async_try_lock_(id, ntf));
            else _strand.Post(() => async_try_lock_(id, ntf));
        }

        internal virtual void async_timed_lock(long id, int ms, Action<bool> ntf)
        {
            if (_strand.RunningInThisThread())
                if (!_mustTick) async_timed_lock_(id, ms, ntf);
                else _strand.AddLast(() => async_timed_lock_(id, ms, ntf));
            else _strand.Post(() => async_timed_lock_(id, ms, ntf));
        }

        internal virtual void async_unlock(long id, Action ntf)
        {
            if (_strand.RunningInThisThread())
                if (!_mustTick) async_unlock_(id, ntf);
                else _strand.AddLast(() => async_unlock_(id, ntf));
            else _strand.Post(() => async_unlock_(id, ntf));
        }

        internal virtual void AsyncCancel(long id, Action ntf)
        {
            if (_strand.RunningInThisThread())
                if (!_mustTick) async_cancel_(id, ntf);
                else _strand.AddLast(() => async_cancel_(id, ntf));
            else _strand.Post(() => async_cancel_(id, ntf));
        }

        public Task Lock()
        {
            return Generator.mutex_lock(this);
        }

        public Task try_lock(AsyncResultWrap<bool> res)
        {
            return Generator.mutex_try_lock(res, this);
        }

        public ValueTask<bool> try_lock()
        {
            return Generator.mutex_try_lock(this);
        }

        public Task timed_lock(AsyncResultWrap<bool> res, int ms)
        {
            return Generator.mutex_timed_lock(res, this, ms);
        }

        public ValueTask<bool> timed_lock(int ms)
        {
            return Generator.mutex_timed_lock(this, ms);
        }

        public Task unlock()
        {
            return Generator.mutex_unlock(this);
        }

        public SharedStrand SelfStrand()
        {
            return _strand;
        }
    }
}
