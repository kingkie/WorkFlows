using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.GoCsp
{
    public class GoConditionVariable
    {
        SharedStrand _strand;
        LinkedList<TupleEx<long, GoMutex, Action>> _waitQueue;
        bool _mustTick;

        public GoConditionVariable(SharedStrand Strand)
        {
            _strand = Strand;
            _waitQueue = new LinkedList<TupleEx<long, GoMutex, Action>>();
            _mustTick = false;
        }

        public GoConditionVariable() : this(SharedStrand.DefaultStrand()) { }

        internal void async_wait(long id, GoMutex mutex, Action ntf)
        {
            mutex.async_unlock(id, delegate ()
            {
                if (_strand.RunningInThisThread())
                    if (!_mustTick) _waitQueue.AddLast(new TupleEx<long, GoMutex, Action>(id, mutex, () => mutex.async_lock(id, ntf)));
                    else _strand.AddLast(() => _waitQueue.AddLast(new TupleEx<long, GoMutex, Action>(id, mutex, () => mutex.async_lock(id, ntf))));
                else _strand.Post(() => _waitQueue.AddLast(new TupleEx<long, GoMutex, Action>(id, mutex, () => mutex.async_lock(id, ntf))));
            });
        }

        private void async_timed_wait_(long id, int ms, GoMutex mutex, Action<bool> ntf)
        {
            if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(_strand);
                LinkedListNode<TupleEx<long, GoMutex, Action>> node = _waitQueue.AddLast(new TupleEx<long, GoMutex, Action>(id, mutex, delegate ()
                {
                    timer.Cancel();
                    mutex.async_lock(id, () => ntf(true));
                }));
                timer.Timeout(ms, delegate ()
                {
                    _waitQueue.Remove(node);
                    mutex.async_lock(id, () => ntf(false));
                });
            }
            else
            {
                _waitQueue.AddLast(new TupleEx<long, GoMutex, Action>(id, mutex, () => mutex.async_lock(id, () => ntf(true))));
            }
        }

        internal void async_timed_wait(long id, int ms, GoMutex mutex, Action<bool> ntf)
        {
            mutex.async_unlock(id, delegate ()
            {
                if (_strand.RunningInThisThread())
                    if (!_mustTick) async_timed_wait_(id, ms, mutex, ntf);
                    else _strand.AddLast(() => async_timed_wait_(id, ms, mutex, ntf));
                else _strand.Post(() => async_timed_wait_(id, ms, mutex, ntf));
            });
        }

        private void notify_one_()
        {
            if (0 != _waitQueue.Count)
            {
                Action ntf = _waitQueue.First.Value.value3;
                _waitQueue.RemoveFirst();
                ntf();
            }
        }

        public void notify_one()
        {
            if (_strand.RunningInThisThread())
                if (!_mustTick) notify_one_();
                else _strand.AddLast(() => notify_one_());
            else _strand.Post(() => notify_one_());
        }

        private void notify_all_()
        {
            _mustTick = true;
            while (0 != _waitQueue.Count)
            {
                Action ntf = _waitQueue.First.Value.value3;
                _waitQueue.RemoveFirst();
                ntf();
            }
            _mustTick = false;
        }

        public void notify_all()
        {
            if (_strand.RunningInThisThread())
                if (!_mustTick) notify_all_();
                else _strand.AddLast(() => notify_all_());
            else _strand.Post(() => notify_all_());
        }

        private void async_cancel_(long id, Action ntf)
        {
            for (LinkedListNode<TupleEx<long, GoMutex, Action>> it = _waitQueue.First; null != it; it = it.Next)
            {
                if (id == it.Value.value1)
                {
                    GoMutex mtx = it.Value.value2;
                    mtx.AsyncCancel(id, ntf);
                    return;
                }
            }
            ntf();
        }

        internal void AsyncCancel(long id, Action ntf)
        {
            if (_strand.RunningInThisThread())
                if (!_mustTick) async_cancel_(id, ntf);
                else _strand.AddLast(() => async_cancel_(id, ntf));
            else _strand.Post(() => async_cancel_(id, ntf));
        }

        public Task Wait(GoMutex mutex)
        {
            return Generator.condition_wait(this, mutex);
        }

        public Task TimedWait(AsyncResultWrap<bool> res, GoMutex mutex, int ms)
        {
            return Generator.condition_timed_wait(res, this, mutex, ms);
        }

        public ValueTask<bool> TimedWait(GoMutex mutex, int ms)
        {
            return Generator.condition_timed_wait(this, mutex, ms);
        }

        public Task Cancel()
        {
            return Generator.condition_cancel(this);
        }
    }
}
