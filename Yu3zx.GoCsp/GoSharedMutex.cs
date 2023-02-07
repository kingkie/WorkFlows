using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yu3zx.GoCsp
{
    public class GoSharedMutex : GoMutex
    {
        enum lock_status
        {
            st_shared,
            st_unique,
            st_upgrade
        };

        struct wait_node
        {
            public Action _ntf;
            public long _waitHostID;
            public lock_status _status;
        };

        class shared_count
        {
            public int _count = 0;
        };

        LinkedList<wait_node> _waitQueue;
        Dictionary<long, shared_count> _sharedMap;

        public GoSharedMutex(SharedStrand Strand) : base(Strand)
        {
            _waitQueue = new LinkedList<wait_node>();
            _sharedMap = new Dictionary<long, shared_count>();
        }

        public GoSharedMutex() : base()
        {
            _waitQueue = new LinkedList<wait_node>();
            _sharedMap = new Dictionary<long, shared_count>();
        }

        protected override void async_lock_(long id, Action ntf)
        {
            if (0 == _sharedMap.Count && (0 == base._lockID || id == base._lockID))
            {
                base._lockID = id;
                base._recCount++;
                ntf();
            }
            else
            {
                _waitQueue.AddLast(new wait_node() { _ntf = ntf, _waitHostID = id, _status = lock_status.st_unique });
            }
        }

        protected override void async_try_lock_(long id, Action<bool> ntf)
        {
            if (0 == _sharedMap.Count && (0 == base._lockID || id == base._lockID))
            {
                base._lockID = id;
                base._recCount++;
                ntf(true);
            }
            else
            {
                ntf(false);
            }
        }

        protected override void async_timed_lock_(long id, int ms, Action<bool> ntf)
        {
            if (0 == _sharedMap.Count && (0 == base._lockID || id == base._lockID))
            {
                base._lockID = id;
                base._recCount++;
                ntf(true);
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                LinkedListNode<wait_node> node = _waitQueue.AddLast(new wait_node()
                {
                    _ntf = delegate ()
                    {
                        timer.Cancel();
                        ntf(true);
                    },
                    _waitHostID = id,
                    _status = lock_status.st_unique
                });
                timer.Timeout(ms, delegate ()
                {
                    _waitQueue.Remove(node);
                    ntf(false);
                });
            }
            else
            {
                _waitQueue.AddLast(new wait_node() { _ntf = () => ntf(true), _waitHostID = id, _status = lock_status.st_unique });
            }
        }

        shared_count find_map(long id)
        {
            shared_count ct = null;
            if (!_sharedMap.TryGetValue(id, out ct))
            {
                ct = new shared_count();
                _sharedMap.Add(id, ct);
            }
            return ct;
        }

        private void async_lock_shared_(long id, Action ntf)
        {
            if (0 != _sharedMap.Count || 0 == base._lockID)
            {
                find_map(id)._count++;
                ntf();
            }
            else
            {
                _waitQueue.AddLast(new wait_node() { _ntf = ntf, _waitHostID = id, _status = lock_status.st_shared });
            }
        }

        private void async_lock_pess_shared_(long id, Action ntf)
        {
            if (0 == _waitQueue.Count && (0 != _sharedMap.Count || 0 == base._lockID))
            {
                find_map(id)._count++;
                ntf();
            }
            else
            {
                _waitQueue.AddLast(new wait_node() { _ntf = ntf, _waitHostID = id, _status = lock_status.st_shared });
            }
        }

        private void async_try_lock_shared_(long id, Action<bool> ntf)
        {
            if (0 != _sharedMap.Count || 0 == base._lockID)
            {
                find_map(id)._count++;
                ntf(true);
            }
            else
            {
                ntf(false);
            }
        }

        private void async_timed_lock_shared_(long id, int ms, Action<bool> ntf)
        {
            if (0 != _sharedMap.Count || 0 == base._lockID)
            {
                find_map(id)._count++;
                ntf(true);
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                LinkedListNode<wait_node> node = _waitQueue.AddLast(new wait_node()
                {
                    _ntf = delegate ()
                    {
                        timer.Cancel();
                        ntf(true);
                    },
                    _waitHostID = id,
                    _status = lock_status.st_shared
                });
                timer.Timeout(ms, delegate ()
                {
                    _waitQueue.Remove(node);
                    ntf(false);
                });
            }
            else
            {
                _waitQueue.AddLast(new wait_node() { _ntf = () => ntf(true), _waitHostID = id, _status = lock_status.st_shared });
            }
        }

        private void async_lock_upgrade_(long id, Action ntf)
        {
            base.async_lock_(id, ntf);
        }

        private void async_try_lock_upgrade_(long id, Action<bool> ntf)
        {
            base.async_try_lock_(id, ntf);
        }

        protected override void async_unlock_(long id, Action ntf)
        {
            if (0 == --base._recCount && 0 != _waitQueue.Count)
            {
                _mustTick = true;
                wait_node queueFront = _waitQueue.First.Value;
                _waitQueue.RemoveFirst();
                queueFront._ntf();
                if (lock_status.st_shared == queueFront._status)
                {
                    base._lockID = 0;
                    find_map(queueFront._waitHostID)._count++;
                    for (LinkedListNode<wait_node> it = _waitQueue.First; null != it;)
                    {
                        if (lock_status.st_shared == it.Value._status)
                        {
                            find_map(it.Value._waitHostID)._count++;
                            it.Value._ntf();
                            LinkedListNode<wait_node> oit = it;
                            it = it.Next;
                            _waitQueue.Remove(oit);
                        }
                        else
                        {
                            it = it.Next;
                        }
                    }
                }
                else
                {
                    base._lockID = queueFront._waitHostID;
                    base._recCount++;
                }
                _mustTick = false;
            }
            ntf();
        }

        private void async_unlock_shared_(long id, Action ntf)
        {
            if (0 == --find_map(id)._count)
            {
                _sharedMap.Remove(id);
                if (0 == _sharedMap.Count && 0 != _waitQueue.Count)
                {
                    _mustTick = true;
                    wait_node queueFront = _waitQueue.First.Value;
                    _waitQueue.RemoveFirst();
                    queueFront._ntf();
                    if (lock_status.st_shared == queueFront._status)
                    {
                        base._lockID = 0;
                        find_map(queueFront._waitHostID)._count++;
                        for (LinkedListNode<wait_node> it = _waitQueue.First; null != it;)
                        {
                            if (lock_status.st_shared == it.Value._status)
                            {
                                find_map(it.Value._waitHostID)._count++;
                                it.Value._ntf();
                                LinkedListNode<wait_node> oit = it;
                                it = it.Next;
                                _waitQueue.Remove(oit);
                            }
                            else
                            {
                                it = it.Next;
                            }
                        }
                    }
                    else
                    {
                        base._lockID = queueFront._waitHostID;
                        base._recCount++;
                    }
                    _mustTick = false;
                }
            }
            ntf();
        }

        private void async_unlock_upgrade_(long id, Action ntf)
        {
            base.async_unlock_(id, ntf);
        }

        private void async_unlock_and_lock_shared_(long id, Action ntf)
        {
            async_unlock_(id, () => async_lock_shared_(id, ntf));
        }

        private void async_unlock_and_lock_upgrade_(long id, Action ntf)
        {
            async_unlock_and_lock_shared_(id, () => async_lock_upgrade_(id, ntf));
        }

        private void async_unlock_upgrade_and_lock_(long id, Action ntf)
        {
            async_unlock_upgrade_(id, () => async_unlock_shared_(id, () => async_lock_(id, ntf)));
        }

        private void async_unlock_shared_and_lock_(long id, Action ntf)
        {
            async_unlock_shared_(id, () => async_lock_(id, ntf));
        }

        protected override void async_cancel_(long id, Action ntf)
        {
            shared_count tempCount;
            if (_sharedMap.TryGetValue(id, out tempCount))
            {
                base.async_cancel_(id, NilAction.action);
                tempCount._count = 1;
                async_unlock_shared_(id, ntf);
            }
            else if (id == base._lockID)
            {
                base._recCount = 1;
                async_unlock_(id, ntf);
            }
            else
            {
                for (LinkedListNode<wait_node> it = _waitQueue.Last; null != it; it = it.Previous)
                {
                    if (it.Value._waitHostID == id)
                    {
                        _waitQueue.Remove(it);
                        break;
                    }
                }
                ntf();
            }
        }

        internal void async_lock_shared(long id, Action ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_lock_shared_(id, ntf);
                else SelfStrand().AddLast(() => async_lock_shared_(id, ntf));
            else SelfStrand().Post(() => async_lock_shared_(id, ntf));
        }

        internal void async_lock_pess_shared(long id, Action ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_lock_pess_shared_(id, ntf);
                else SelfStrand().AddLast(() => async_lock_pess_shared_(id, ntf));
            else SelfStrand().Post(() => async_lock_pess_shared_(id, ntf));
        }

        internal void async_try_lock_shared(long id, Action<bool> ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_lock_shared_(id, ntf);
                else SelfStrand().AddLast(() => async_try_lock_shared_(id, ntf));
            else SelfStrand().Post(() => async_try_lock_shared_(id, ntf));
        }

        internal void async_timed_lock_shared(long id, int ms, Action<bool> ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_timed_lock_shared_(id, ms, ntf);
                else SelfStrand().AddLast(() => async_timed_lock_shared_(id, ms, ntf));
            else SelfStrand().Post(() => async_timed_lock_shared_(id, ms, ntf));
        }

        internal void async_lock_upgrade(long id, Action ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_lock_upgrade_(id, ntf);
                else SelfStrand().AddLast(() => async_lock_upgrade_(id, ntf));
            else SelfStrand().Post(() => async_lock_upgrade_(id, ntf));
        }

        internal void async_try_lock_upgrade(long id, Action<bool> ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_lock_upgrade_(id, ntf);
                else SelfStrand().AddLast(() => async_try_lock_upgrade_(id, ntf));
            else SelfStrand().Post(() => async_try_lock_upgrade_(id, ntf));
        }

        internal void async_unlock_shared(long id, Action ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_unlock_shared_(id, ntf);
                else SelfStrand().AddLast(() => async_unlock_shared_(id, ntf));
            else SelfStrand().Post(() => async_unlock_shared_(id, ntf));
        }

        internal void async_unlock_upgrade(long id, Action ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_unlock_upgrade_(id, ntf);
                else SelfStrand().AddLast(() => async_unlock_upgrade_(id, ntf));
            else SelfStrand().Post(() => async_unlock_upgrade_(id, ntf));
        }

        internal void unlock_and_lock_shared(long id, Action ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_unlock_and_lock_shared_(id, ntf);
                else SelfStrand().AddLast(() => async_unlock_and_lock_shared_(id, ntf));
            else SelfStrand().Post(() => async_unlock_and_lock_shared_(id, ntf));
        }

        internal void unlock_and_lock_upgrade(long id, Action ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_unlock_and_lock_upgrade_(id, ntf);
                else SelfStrand().AddLast(() => async_unlock_and_lock_upgrade_(id, ntf));
            else SelfStrand().Post(() => async_unlock_and_lock_upgrade_(id, ntf));
        }

        internal void unlock_upgrade_and_lock(long id, Action ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_unlock_upgrade_and_lock_(id, ntf);
                else SelfStrand().AddLast(() => async_unlock_upgrade_and_lock_(id, ntf));
            else SelfStrand().Post(() => async_unlock_upgrade_and_lock_(id, ntf));
        }

        internal void unlock_shared_and_lock(long id, Action ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_unlock_shared_and_lock_(id, ntf);
                else SelfStrand().AddLast(() => async_unlock_shared_and_lock_(id, ntf));
            else SelfStrand().Post(() => async_unlock_shared_and_lock_(id, ntf));
        }

        public Task lock_shared()
        {
            return Generator.mutex_lock_shared(this);
        }

        public Task lock_pess_shared()
        {
            return Generator.mutex_lock_pess_shared(this);
        }

        public Task lock_upgrade()
        {
            return Generator.mutex_lock_upgrade(this);
        }

        public Task try_lock_shared(AsyncResultWrap<bool> res)
        {
            return Generator.mutex_try_lock_shared(res, this);
        }

        public ValueTask<bool> try_lock_shared()
        {
            return Generator.mutex_try_lock_shared(this);
        }

        public Task try_lock_upgrade(AsyncResultWrap<bool> res)
        {
            return Generator.mutex_try_lock_upgrade(res, this);
        }

        public ValueTask<bool> try_lock_upgrade()
        {
            return Generator.mutex_try_lock_upgrade(this);
        }

        public Task timed_lock_shared(AsyncResultWrap<bool> res, int ms)
        {
            return Generator.mutex_timed_lock_shared(res, this, ms);
        }

        public ValueTask<bool> timed_lock_shared(int ms)
        {
            return Generator.mutex_timed_lock_shared(this, ms);
        }

        public Task unlock_shared()
        {
            return Generator.mutex_unlock_shared(this);
        }

        public Task unlock_upgrade()
        {
            return Generator.mutex_unlock_upgrade(this);
        }
    }
}
