using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Yu3zx.GoCsp
{
    public enum ChannelState
    {
        undefined,
        ok,
        Fail,
        csp_fail,
        Cancel,
        closed,
        overtime
    }

    public enum ChannelType
    {
        undefined,
        broadcast,
        unlimit,
        limit,
        nil,
        csp
    }

    struct NotifyPick
    {
        public AsyncTimer timer;
        public Action<ChannelState> ntf;

        public void CancelTimer()
        {
            timer?.Cancel();
        }

        public bool Invoke(ChannelState state)
        {
            if (null != ntf)
            {
                ntf(state);
                return true;
            }
            return false;
        }
    }

    public class ChannelNotifySign
    {
        internal PriorityQueueNode<NotifyPick> _ntfNode;
        internal bool _selectOnce = false;
        internal bool _disable = false;
        internal bool _success = false;

        internal void SetNode(PriorityQueueNode<NotifyPick> node)
        {
            _ntfNode = node;
        }

        internal void ResetNode()
        {
            _ntfNode = default(PriorityQueueNode<NotifyPick>);
        }

        internal bool ResetSuccess()
        {
            bool success = _success;
            _success = false;
            return success;
        }

        static internal void SetNode(ChannelNotifySign sign, PriorityQueueNode<NotifyPick> node)
        {
            if (null != sign)
            {
                sign._ntfNode = node;
            }
        }
    }

    struct PriorityQueueNode<T>
    {
        public int _priority;
        public LinkedListNode<T> _node;

        public bool Effect
        {
            get
            {
                return null != _node;
            }
        }

        public T Value
        {
            get
            {
                return _node.Value;
            }
        }
    }

    struct PriorityQueue<T>
    {
        public LinkedList<T> _queue0;
        public LinkedList<T> _queue1;

        private static PriorityQueueNode<T> AddFirst(int priority, ref LinkedList<T> queue, T value)
        {
            if (null == queue)
            {
                queue = new LinkedList<T>();
            }
            return new PriorityQueueNode<T>() { _priority = priority, _node = queue.AddFirst(value) };
        }

        private static PriorityQueueNode<T> AddLast(int priority, ref LinkedList<T> queue, T value)
        {
            if (null == queue)
            {
                queue = new LinkedList<T>();
            }
            return new PriorityQueueNode<T>() { _priority = priority, _node = queue.AddLast(value) };
        }

        public PriorityQueueNode<T> AddFirst(int priority, T value)
        {
            switch (priority)
            {
                case 0: return AddFirst(priority, ref _queue0, value);
                case 1: return AddFirst(priority, ref _queue1, value);
                default: return default(PriorityQueueNode<T>);
            }
        }

        public PriorityQueueNode<T> AddLast(int priority, T value)
        {
            switch (priority)
            {
                case 0: return AddLast(priority, ref _queue0, value);
                case 1: return AddLast(priority, ref _queue1, value);
                default: return default(PriorityQueueNode<T>);
            }
        }

        public PriorityQueueNode<T> AddFirst(T value)
        {
            return AddFirst(0, ref _queue0, value);
        }

        public PriorityQueueNode<T> AddLast(T value)
        {
            return AddLast(1, ref _queue1, value);
        }

        public bool Empty
        {
            get
            {
                return 0 == (null == _queue0 ? 0 : _queue0.Count) + (null == _queue1 ? 0 : _queue1.Count);
            }
        }

        public int Count
        {
            get
            {
                return (null == _queue0 ? 0 : _queue0.Count) + (null == _queue1 ? 0 : _queue1.Count);
            }
        }

        public int Count0
        {
            get
            {
                return null == _queue0 ? 0 : _queue0.Count;
            }
        }

        public int Count1
        {
            get
            {
                return null == _queue1 ? 0 : _queue1.Count;
            }
        }

        public PriorityQueueNode<T> First
        {
            get
            {
                if (null != _queue0 && 0 != _queue0.Count)
                {
                    return new PriorityQueueNode<T>() { _priority = 0, _node = _queue0.First };
                }
                else if (null != _queue1 && 0 != _queue1.Count)
                {
                    return new PriorityQueueNode<T>() { _priority = 1, _node = _queue1.First };
                }
                return new PriorityQueueNode<T>();
            }
        }

        public PriorityQueueNode<T> Last
        {
            get
            {
                if (null != _queue1 && 0 != _queue1.Count)
                {
                    return new PriorityQueueNode<T>() { _priority = 1, _node = _queue1.Last };
                }
                else if (null != _queue0 && 0 != _queue0.Count)
                {
                    return new PriorityQueueNode<T>() { _priority = 0, _node = _queue0.Last };
                }
                return new PriorityQueueNode<T>();
            }
        }

        public T RemoveFirst()
        {
            if (null != _queue0 && 0 != _queue0.Count)
            {
                T First = _queue0.First.Value;
                _queue0.RemoveFirst();
                return First;
            }
            else if (null != _queue1 && 0 != _queue1.Count)
            {
                T First = _queue1.First.Value;
                _queue1.RemoveFirst();
                return First;
            }
            return default(T);
        }

        public T RemoveLast()
        {
            if (null != _queue1 && 0 != _queue1.Count)
            {
                T Last = _queue1.Last.Value;
                _queue1.RemoveLast();
                return Last;
            }
            else if (null != _queue0 && 0 != _queue0.Count)
            {
                T Last = _queue0.Last.Value;
                _queue0.RemoveLast();
                return Last;
            }
            return default(T);
        }

        public T Remove(PriorityQueueNode<T> node)
        {
            if (null != node._node)
            {
                switch (node._priority)
                {
                    case 0: _queue0.Remove(node._node); break;
                    case 1: _queue1.Remove(node._node); break;
                }
                return node._node.Value;
            }
            return default(T);
        }
    }

    public struct SelectChannelState
    {
        public bool Failed;
        public bool NextRound;
    }

    internal abstract class SelectChannelBase
    {
        public bool enable;
        public ChannelNotifySign ntfSign = new ChannelNotifySign();

        public Action<ChannelState> NextSelect;
        public bool disabled() { return ntfSign._disable; }
        public abstract void Begin(Generator host);
        public abstract Task<SelectChannelState> Invoke(Func<Task> stepOne = null);
        public abstract ValueTask<bool> ErrInvoke(ChannelState state);
        public abstract Task End();
        public abstract bool IsRead();
        public abstract ChannelBase Channel();
    }

    public abstract class ChannelBase
    {
        private SharedStrand _strand;
        protected bool _mustTick;
        protected bool _closed;
        protected ChannelBase(SharedStrand Strand) { _strand = Strand; _mustTick = false; _closed = false; }
        public abstract ChannelType GetChannelType();
        protected abstract void async_clear_(Action ntf);
        protected abstract void async_close_(Action ntf, bool isClear = false);
        protected abstract void async_cancel_(Action ntf, bool isClear = false);
        protected abstract void async_append_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms);
        protected abstract void async_remove_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign);
        protected abstract void async_append_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms);
        protected abstract void async_remove_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign);
        protected virtual void async_append_recv_notify_(Action<ChannelState> ntf, BroadcastToken token, ChannelNotifySign ntfSign, int ms) { async_append_recv_notify_(ntf, ntfSign, ms); }
        public void Clear() { AsyncClear(NilAction.action); }
        public void Close(bool isClear = false) { AsyncClose(NilAction.action, isClear); }
        public void Cancel(bool isClear = false) { AsyncCancel(NilAction.action, isClear); }
        public bool IsClosed() { return _closed; }
        public SharedStrand SelfStrand() { return _strand; }

        public void AsyncClear(Action ntf)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_clear_(ntf);
                else SelfStrand().AddLast(() => async_clear_(ntf));
            else SelfStrand().Post(() => async_clear_(ntf));
        }

        public void AsyncClose(Action ntf, bool isClear = false)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_close_(ntf);
                else SelfStrand().AddLast(() => async_close_(ntf));
            else SelfStrand().Post(() => async_close_(ntf));
        }

        public void AsyncCancel(Action ntf, bool isClear = false)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_cancel_(ntf, isClear);
                else SelfStrand().AddLast(() => async_cancel_(ntf, isClear));
            else SelfStrand().Post(() => async_cancel_(ntf, isClear));
        }

        public void AsyncAppendRecvNotify(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms = -1)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_append_recv_notify_(ntf, ntfSign, ms);
                else SelfStrand().AddLast(() => async_append_recv_notify_(ntf, ntfSign, ms));
            else SelfStrand().Post(() => async_append_recv_notify_(ntf, ntfSign, ms));
        }

        public void AsyncRemoveRecvNotify(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_remove_recv_notify_(ntf, ntfSign);
                else SelfStrand().AddLast(() => async_remove_recv_notify_(ntf, ntfSign));
            else SelfStrand().Post(() => async_remove_recv_notify_(ntf, ntfSign));
        }

        public void AsyncAppendSendNotify(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms = -1)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_append_send_notify_(ntf, ntfSign, ms);
                else SelfStrand().AddLast(() => async_append_send_notify_(ntf, ntfSign, ms));
            else SelfStrand().Post(() => async_append_send_notify_(ntf, ntfSign, ms));
        }

        public void AsyncRemoveSendNotify(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_remove_send_notify_(ntf, ntfSign);
                else SelfStrand().AddLast(() => async_remove_send_notify_(ntf, ntfSign));
            else SelfStrand().Post(() => async_remove_send_notify_(ntf, ntfSign));
        }

        public void AsyncAppendRecvNotify(Action<ChannelState> ntf, BroadcastToken token, ChannelNotifySign ntfSign, int ms = -1)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_append_recv_notify_(ntf, token, ntfSign, ms);
                else SelfStrand().AddLast(() => async_append_recv_notify_(ntf, token, ntfSign, ms));
            else SelfStrand().Post(() => async_append_recv_notify_(ntf, token, ntfSign, ms));
        }

        static private void QueueCallback(ref PriorityQueue<NotifyPick> queue, ChannelState state)
        {
            LinkedList<NotifyPick> queue0 = queue._queue0;
            LinkedList<NotifyPick> queue1 = queue._queue1;
            if (null != queue0)
            {
                while (0 != queue0.Count)
                {
                    NotifyPick ntf = queue0.First.Value;
                    queue0.RemoveFirst();
                    ntf.Invoke(state);
                }
            }
            if (null != queue1)
            {
                while (0 != queue1.Count)
                {
                    NotifyPick ntf = queue1.First.Value;
                    queue1.RemoveFirst();
                    ntf.Invoke(state);
                }
            }
        }

        internal void SafeCallback(ref PriorityQueue<NotifyPick> queue, ChannelState state)
        {
            _mustTick = true;
            QueueCallback(ref queue, state);
            _mustTick = false;
        }

        internal void SafeCallback(ref PriorityQueue<NotifyPick> queue1, ref PriorityQueue<NotifyPick> queue2, ChannelState state)
        {
            _mustTick = true;
            QueueCallback(ref queue1, state);
            QueueCallback(ref queue2, state);
            _mustTick = false;
        }
    }

    public abstract class Channel<T> : ChannelBase
    {
        internal class SelectChannelReader : SelectChannelBase
        {
            public BroadcastToken _token = BroadcastToken._defToken;
            public Channel<T> _chan;
            public Func<T, Task> _handler;
            public Func<ChannelState, Task<bool>> _errHandler;
            public ChannelLostMsg<T> _lostMsg;
            public int _chanTimeout = -1;
            AsyncResultWrap<ChannelReceiveWrap<T>> _tryRecvRes;
            Generator _host;

            public override void Begin(Generator host)
            {
                ntfSign._disable = false;
                _host = host;
                _tryRecvRes = new AsyncResultWrap<ChannelReceiveWrap<T>> { value1 = ChannelReceiveWrap<T>.def };
                if (enable)
                {
                    _chan.AsyncAppendRecvNotify(NextSelect, ntfSign, _chanTimeout);
                }
            }

            public override async Task<SelectChannelState> Invoke(Func<Task> stepOne)
            {
                try
                {
                    _tryRecvRes.value1 = ChannelReceiveWrap<T>.def;
                    _chan.AsyncTryRecvAndAppendNotify(_host.unsafe_async_result(_tryRecvRes), NextSelect, _token, ntfSign, _chanTimeout);
                    await _host.async_wait();
                }
                catch (Generator.StopException)
                {
                    _chan.AsyncRemoveRecvNotify(_host.unsafe_async_ignore<ChannelState>(), ntfSign);
                    await _host.async_wait();
                    if (ChannelState.ok == _tryRecvRes.value1.state)
                    {
                        _lostMsg?.set(_tryRecvRes.value1.msg);
                    }
                    throw;
                }
                SelectChannelState chanState = new SelectChannelState() { Failed = false, NextRound = true };
                if (ChannelState.ok == _tryRecvRes.value1.state)
                {
                    bool invoked = false;
                    try
                    {
                        if (null != stepOne)
                        {
                            await stepOne();
                        }
                        await _host.unlock_suspend_();
                        invoked = true;
                        await _handler(_tryRecvRes.value1.msg);
                    }
                    catch (Generator.StopException)
                    {
                        if (!invoked)
                        {
                            _lostMsg?.set(_tryRecvRes.value1.msg);
                        }
                        throw;
                    }
                    finally
                    {
                        _host.lock_suspend_();
                    }
                }
                else if (ChannelState.closed == _tryRecvRes.value1.state)
                {
                    await End();
                    chanState.Failed = true;
                }
                else
                {
                    chanState.Failed = true;
                }
                chanState.NextRound = !ntfSign._disable;
                return chanState;
            }

            private async Task<bool> errInvoke_(ChannelState state)
            {
                try
                {
                    await _host.unlock_suspend_();
                    if (!await _errHandler(state) && ChannelState.closed != state)
                    {
                        _chan.AsyncAppendRecvNotify(NextSelect, ntfSign, _chanTimeout);
                        return false;
                    }
                }
                finally
                {
                    _host.lock_suspend_();
                }
                return true;
            }

            public override ValueTask<bool> ErrInvoke(ChannelState state)
            {
                if (null != _errHandler)
                {
                    return new ValueTask<bool>(errInvoke_(state));
                }
                return new ValueTask<bool>(true);
            }

            public override Task End()
            {
                ntfSign._disable = true;
                _chan.AsyncRemoveRecvNotify(_host.unsafe_async_ignore<ChannelState>(), ntfSign);
                return _host.async_wait();
            }

            public override bool IsRead()
            {
                return true;
            }

            public override ChannelBase Channel()
            {
                return _chan;
            }
        }

        internal class SelectChannelWriter : SelectChannelBase
        {
            public Channel<T> _chan;
            public AsyncResultWrap<T> _msg;
            public Func<Task> _handler;
            public Func<ChannelState, Task<bool>> _errHandler;
            public ChannelLostMsg<T> _lostMsg;
            public int _chanTimeout = -1;
            AsyncResultWrap<ChannelSendWrap> _trySendRes;
            Generator _host;

            public override void Begin(Generator host)
            {
                ntfSign._disable = false;
                _host = host;
                _trySendRes = new AsyncResultWrap<ChannelSendWrap> { value1 = ChannelSendWrap.def };
                if (enable)
                {
                    _chan.AsyncAppendSendNotify(NextSelect, ntfSign, _chanTimeout);
                }
            }

            public override async Task<SelectChannelState> Invoke(Func<Task> stepOne)
            {
                try
                {
                    _trySendRes.value1 = ChannelSendWrap.def;
                    _chan.AsyncTrySendAndAppendNotify(_host.unsafe_async_result(_trySendRes), NextSelect, ntfSign, _msg.value1, _chanTimeout);
                    await _host.async_wait();
                }
                catch (Generator.StopException)
                {
                    _chan.AsyncRemoveSendNotify(_host.unsafe_async_ignore<ChannelState>(), ntfSign);
                    await _host.async_wait();
                    if (ChannelState.ok != _trySendRes.value1.state)
                    {
                        _lostMsg?.set(_msg.value1);
                    }
                    throw;
                }
                SelectChannelState chanState = new SelectChannelState() { Failed = false, NextRound = true };
                if (ChannelState.ok == _trySendRes.value1.state)
                {
                    try
                    {
                        if (null != stepOne)
                        {
                            await stepOne();
                        }
                        await _host.unlock_suspend_();
                        await _handler();
                    }
                    finally
                    {
                        _host.lock_suspend_();
                    }
                }
                else if (ChannelState.closed == _trySendRes.value1.state)
                {
                    await End();
                    chanState.Failed = true;
                }
                else
                {
                    chanState.Failed = true;
                }
                chanState.NextRound = !ntfSign._disable;
                return chanState;
            }

            private async Task<bool> errInvoke_(ChannelState state)
            {
                try
                {
                    await _host.unlock_suspend_();
                    if (!await _errHandler(state) && ChannelState.closed != state)
                    {
                        _chan.AsyncAppendSendNotify(NextSelect, ntfSign, _chanTimeout);
                        return false;
                    }
                }
                finally
                {
                    _host.lock_suspend_();
                }
                return true;
            }

            public override ValueTask<bool> ErrInvoke(ChannelState state)
            {
                if (null != _errHandler)
                {
                    return new ValueTask<bool>(errInvoke_(state));
                }
                return new ValueTask<bool>(true);
            }

            public override Task End()
            {
                ntfSign._disable = true;
                _chan.AsyncRemoveSendNotify(_host.unsafe_async_ignore<ChannelState>(), ntfSign);
                return _host.async_wait();
            }

            public override bool IsRead()
            {
                return false;
            }

            public override ChannelBase Channel()
            {
                return _chan;
            }
        }
        protected Channel(SharedStrand Strand) : base(Strand) { }
        protected abstract void async_send_(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign);
        protected abstract void async_recv_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign);
        protected abstract void async_try_send_(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign);
        protected abstract void async_try_recv_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign);
        protected abstract void async_timed_send_(int ms, Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign);
        protected abstract void async_timed_recv_(int ms, Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign);
        protected abstract void async_try_recv_and_append_notify_(Action<ChannelReceiveWrap<T>> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, int ms);
        protected abstract void async_try_send_and_append_notify_(Action<ChannelSendWrap> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, T msg, int ms);

        public void AsyncSend(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_send_(ntf, msg, ntfSign);
                else SelfStrand().AddLast(() => async_send_(ntf, msg, ntfSign));
            else SelfStrand().Post(() => async_send_(ntf, msg, ntfSign));
        }

        public void AsyncRecv(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_recv_(ntf, ntfSign);
                else SelfStrand().AddLast(() => async_recv_(ntf, ntfSign));
            else SelfStrand().Post(() => async_recv_(ntf, ntfSign));
        }

        public void AsyncTrySend(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_send_(ntf, msg, ntfSign);
                else SelfStrand().AddLast(() => async_try_send_(ntf, msg, ntfSign));
            else SelfStrand().Post(() => async_try_send_(ntf, msg, ntfSign));
        }

        public void AsyncTryRecv(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_recv_(ntf, ntfSign);
                else SelfStrand().AddLast(() => async_try_recv_(ntf, ntfSign));
            else SelfStrand().Post(() => async_try_recv_(ntf, ntfSign));
        }

        public void AsyncTimedSend(int ms, Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_timed_send_(ms, ntf, msg, ntfSign);
                else SelfStrand().AddLast(() => async_timed_send_(ms, ntf, msg, ntfSign));
            else SelfStrand().Post(() => async_timed_send_(ms, ntf, msg, ntfSign));
        }

        public void AsyncTimedRecv(int ms, Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_timed_recv_(ms, ntf, ntfSign);
                else SelfStrand().AddLast(() => async_timed_recv_(ms, ntf, ntfSign));
            else SelfStrand().Post(() => async_timed_recv_(ms, ntf, ntfSign));
        }

        public void AsyncTryRecvAndAppendNotify(Action<ChannelReceiveWrap<T>> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, int ms = -1)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_recv_and_append_notify_(cb, msgNtf, ntfSign, ms);
                else SelfStrand().AddLast(() => async_try_recv_and_append_notify_(cb, msgNtf, ntfSign, ms));
            else SelfStrand().Post(() => async_try_recv_and_append_notify_(cb, msgNtf, ntfSign, ms));
        }

        public void AsyncTrySendAndAppendNotify(Action<ChannelSendWrap> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, T msg, int ms = -1)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_send_and_append_notify_(cb, msgNtf, ntfSign, msg, ms);
                else SelfStrand().AddLast(() => async_try_send_and_append_notify_(cb, msgNtf, ntfSign, msg, ms));
            else SelfStrand().Post(() => async_try_send_and_append_notify_(cb, msgNtf, ntfSign, msg, ms));
        }

        public Task UnsafeSend(AsyncResultWrap<ChannelSendWrap> res, T msg)
        {
            return Generator.unsafe_chan_send(res, this, msg);
        }

        public ValueTask<ChannelSendWrap> Send(T msg, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_send(this, msg, lostMsg);
        }

        public Task UnsafeReceive(AsyncResultWrap<ChannelReceiveWrap<T>> res)
        {
            return Generator.unsafe_chan_receive(res, this);
        }

        public ValueTask<ChannelReceiveWrap<T>> Receive(ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_receive(this, lostMsg);
        }

        public Task UnsafeTrySend(AsyncResultWrap<ChannelSendWrap> res, T msg)
        {
            return Generator.unsafe_chan_try_send(res, this, msg);
        }

        public ValueTask<ChannelSendWrap> TrySend(T msg, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_try_send(this, msg, lostMsg);
        }

        public Task UnsafeTryReceive(AsyncResultWrap<ChannelReceiveWrap<T>> res)
        {
            return Generator.unsafe_chan_try_receive(res, this);
        }

        public ValueTask<ChannelReceiveWrap<T>> TryReceive(ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_try_receive(this, lostMsg);
        }

        public Task UnsafeTimedSend(AsyncResultWrap<ChannelSendWrap> res, int ms, T msg)
        {
            return Generator.unsafe_chan_timed_send(res, this, ms, msg);
        }

        public ValueTask<ChannelSendWrap> TimedSend(int ms, T msg, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_timed_send(this, ms, msg, lostMsg);
        }

        public Task UnsafeTimedReceive(AsyncResultWrap<ChannelReceiveWrap<T>> res, int ms)
        {
            return Generator.unsafe_chan_timed_receive(res, this, ms);
        }

        public ValueTask<ChannelReceiveWrap<T>> TimedReceive(int ms, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_timed_receive(this, ms, lostMsg);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Strand"></param>
        /// <param name="len">限制通道数，0为只有单通道；否则是限制通道；负值为非限制通道</param>
        /// <returns></returns>
        static public Channel<T> Make(SharedStrand Strand, int len)
        {
            if (0 == len)
            {
                return new NilChannel<T>(Strand);
            }
            else if (0 < len)
            {
                return new LimitChannel<T>(Strand, len);
            }
            return new UnlimitChannel<T>(Strand);
        }

        static public Channel<T> Make(int len)
        {
            return Make(SharedStrand.DefaultStrand(), len);
        }

        public void Post(T msg)
        {
            AsyncSend(NilAction<ChannelSendWrap>.action, msg, null);
        }

        public void TryPost(T msg)
        {
            AsyncTrySend(NilAction<ChannelSendWrap>.action, msg, null);
        }

        public void TimedPost(int ms, T msg)
        {
            AsyncTimedSend(ms, NilAction<ChannelSendWrap>.action, msg, null);
        }

        public void Discard()
        {
            AsyncRecv(NilAction<ChannelReceiveWrap<T>>.action, null);
        }

        public void TryDiscard()
        {
            AsyncTryRecv(NilAction<ChannelReceiveWrap<T>>.action, null);
        }

        public void TimedDiscard(int ms)
        {
            AsyncTimedRecv(ms, NilAction<ChannelReceiveWrap<T>>.action, null);
        }

        public Action<T> Wrap()
        {
            return Post;
        }

        public Action<T> WrapTry()
        {
            return TryPost;
        }

        public Action<int, T> WrapTimed()
        {
            return TimedPost;
        }

        public Action<T> WrapTimed(int ms)
        {
            return (T p) => TimedPost(ms, p);
        }

        public Action WrapDefault()
        {
            return () => Post(default(T));
        }

        public Action WrapTryDefault()
        {
            return () => TryPost(default(T));
        }

        public Action<int> WrapTimedDefault()
        {
            return (int ms) => TimedPost(ms, default(T));
        }

        public Action WrapTimedDefault(int ms)
        {
            return () => TimedPost(ms, default(T));
        }

        public Action WrapDiscard()
        {
            return Discard;
        }

        public Action WrapTryDiscard()
        {
            return TryDiscard;
        }

        public Action<int> WrapTimedDiscard()
        {
            return TimedDiscard;
        }

        public Action WrapTimedDiscard(int ms)
        {
            return () => TimedDiscard(ms);
        }

        internal SelectChannelBase MakeSelectReader(Func<T, Task> handler, ChannelLostMsg<T> lostMsg)
        {
            return new SelectChannelReader() { _chan = this, _handler = handler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectReader(Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<T> lostMsg)
        {
            return new SelectChannelReader() { _chan = this, _handler = handler, _errHandler = errHandler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectReader(int ms, Func<T, Task> handler, ChannelLostMsg<T> lostMsg)
        {
            return new SelectChannelReader() { _chanTimeout = ms, _chan = this, _handler = handler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectReader(int ms, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<T> lostMsg)
        {
            return new SelectChannelReader() { _chanTimeout = ms, _chan = this, _handler = handler, _errHandler = errHandler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectWriter(AsyncResultWrap<T> msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<T> lostMsg)
        {
            return new SelectChannelWriter() { _chan = this, _msg = msg, _handler = handler, _errHandler = errHandler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectWriter(T msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<T> lostMsg)
        {
            return MakeSelectWriter(new AsyncResultWrap<T> { value1 = msg }, handler, errHandler, lostMsg);
        }

        internal SelectChannelBase MakeSelectWriter(int ms, AsyncResultWrap<T> msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<T> lostMsg)
        {
            return new SelectChannelWriter() { _chanTimeout = ms, _chan = this, _msg = msg, _handler = handler, _errHandler = errHandler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectWriter(int ms, T msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<T> lostMsg)
        {
            return MakeSelectWriter(ms, new AsyncResultWrap<T> { value1 = msg }, handler, errHandler, lostMsg);
        }

        protected virtual void async_recv_(Action<ChannelReceiveWrap<T>> ntf, BroadcastToken token, ChannelNotifySign ntfSign = null)
        {
            async_recv_(ntf, ntfSign);
        }

        protected virtual void async_try_recv_(Action<ChannelReceiveWrap<T>> ntf, BroadcastToken token, ChannelNotifySign ntfSign = null)
        {
            async_try_recv_(ntf, ntfSign);
        }

        protected virtual void async_timed_recv_(int ms, Action<ChannelReceiveWrap<T>> ntf, BroadcastToken token, ChannelNotifySign ntfSign = null)
        {
            async_timed_recv_(ms, ntf, ntfSign);
        }

        protected virtual void async_try_recv_and_append_notify_(Action<ChannelReceiveWrap<T>> cb, Action<ChannelState> msgNtf, BroadcastToken token, ChannelNotifySign ntfSign, int ms = -1)
        {
            async_try_recv_and_append_notify_(cb, msgNtf, ntfSign, ms);
        }

        public void AsyncRecv(Action<ChannelReceiveWrap<T>> ntf, BroadcastToken token, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_recv_(ntf, token, ntfSign);
                else SelfStrand().AddLast(() => async_recv_(ntf, token, ntfSign));
            else SelfStrand().Post(() => async_recv_(ntf, token, ntfSign));
        }

        public void AsyncTryRecv(Action<ChannelReceiveWrap<T>> ntf, BroadcastToken token, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_recv_(ntf, token, ntfSign);
                else SelfStrand().AddLast(() => async_try_recv_(ntf, token, ntfSign));
            else SelfStrand().Post(() => async_try_recv_(ntf, token, ntfSign));
        }

        public void AsyncTimedRecv(int ms, Action<ChannelReceiveWrap<T>> ntf, BroadcastToken token, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_timed_recv_(ms, ntf, token, ntfSign);
                else SelfStrand().AddLast(() => async_timed_recv_(ms, ntf, token, ntfSign));
            else SelfStrand().Post(() => async_timed_recv_(ms, ntf, token, ntfSign));
        }

        public void AsyncTryRecvAndAppendNotify(Action<ChannelReceiveWrap<T>> cb, Action<ChannelState> msgNtf, BroadcastToken token, ChannelNotifySign ntfSign, int ms = -1)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_recv_and_append_notify_(cb, msgNtf, token, ntfSign, ms);
                else SelfStrand().AddLast(() => async_try_recv_and_append_notify_(cb, msgNtf, token, ntfSign, ms));
            else SelfStrand().Post(() => async_try_recv_and_append_notify_(cb, msgNtf, token, ntfSign, ms));
        }

        public Task UnsafeReceive(AsyncResultWrap<ChannelReceiveWrap<T>> res, BroadcastToken token)
        {
            return Generator.unsafe_chan_receive(res, this, token);
        }

        public ValueTask<ChannelReceiveWrap<T>> Receive(BroadcastToken token, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_receive(this, token, lostMsg);
        }

        public Task UnsafeTryReceive(AsyncResultWrap<ChannelReceiveWrap<T>> res, BroadcastToken token)
        {
            return Generator.unsafe_chan_try_receive(res, this, token);
        }

        public ValueTask<ChannelReceiveWrap<T>> TryReceive(BroadcastToken token, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_try_receive(this, token, lostMsg);
        }

        public Task UnsafeTimedReceive(AsyncResultWrap<ChannelReceiveWrap<T>> res, int ms, BroadcastToken token)
        {
            return Generator.unsafe_chan_timed_receive(res, this, ms, token);
        }

        public ValueTask<ChannelReceiveWrap<T>> TimedReceive(int ms, BroadcastToken token, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_timed_receive(this, ms, token, lostMsg);
        }

        internal virtual SelectChannelBase MakeSelectReader(Func<T, Task> handler, BroadcastToken token, ChannelLostMsg<T> lostMsg)
        {
            return MakeSelectReader(handler, lostMsg);
        }

        internal virtual SelectChannelBase MakeSelectReader(Func<T, Task> handler, BroadcastToken token, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<T> lostMsg)
        {
            return MakeSelectReader(handler, errHandler, lostMsg);
        }

        internal virtual SelectChannelBase MakeSelectReader(int ms, Func<T, Task> handler, BroadcastToken token, ChannelLostMsg<T> lostMsg)
        {
            return MakeSelectReader(ms, handler, lostMsg);
        }

        internal virtual SelectChannelBase MakeSelectReader(int ms, Func<T, Task> handler, BroadcastToken token, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<T> lostMsg)
        {
            return MakeSelectReader(ms, handler, errHandler, lostMsg);
        }
    }

    abstract class MsgQueueAbs<T>
    {
        public abstract void AddLast(T msg);
        public abstract T RemoveFirst();
        public abstract T First();
        public abstract T Last();
        public abstract int Count { get; }
        public abstract bool Empty { get; }
        public abstract void Clear();
    }

    class NoVoidMsgQueue<T> : MsgQueueAbs<T>
    {
        MsgQueue<T> _msgBuff;

        public NoVoidMsgQueue()
        {
            _msgBuff = new MsgQueue<T>();
        }

        public override void AddLast(T msg)
        {
            _msgBuff.AddLast(msg);
        }

        public override T RemoveFirst()
        {
            T First = _msgBuff.First.Value;
            _msgBuff.RemoveFirst();
            return First;
        }

        public override T First()
        {
            return _msgBuff.First.Value;
        }

        public override T Last()
        {
            return _msgBuff.Last.Value;
        }

        public override int Count
        {
            get
            {
                return _msgBuff.Count;
            }
        }

        public override bool Empty
        {
            get
            {
                return 0 == _msgBuff.Count;
            }
        }

        public override void Clear()
        {
            _msgBuff.Clear();
        }
    }

    class VoidMsgQueue<T> : MsgQueueAbs<T>
    {
        int _count;

        public VoidMsgQueue()
        {
            _count = 0;
        }

        public override void AddLast(T msg)
        {
            _count++;
        }

        public override T RemoveFirst()
        {
            _count--;
            return default(T);
        }

        public override T First()
        {
            return default(T);
        }

        public override T Last()
        {
            return default(T);
        }

        public override int Count
        {
            get
            {
                return _count;
            }
        }

        public override bool Empty
        {
            get
            {
                return 0 == _count;
            }
        }

        public override void Clear()
        {
            _count = 0;
        }
    }

    public class UnlimitChannel<T> : Channel<T>
    {
        MsgQueueAbs<T> _msgQueue;
        PriorityQueue<NotifyPick> _recvQueue;

        public UnlimitChannel(SharedStrand Strand) : base(Strand)
        {
            _msgQueue = default(T) is VoidType ? (MsgQueueAbs<T>)new VoidMsgQueue<T>() : new NoVoidMsgQueue<T>();
            _recvQueue = new PriorityQueue<NotifyPick>();
        }

        public UnlimitChannel() : this(SharedStrand.DefaultStrand()) { }

        public override ChannelType GetChannelType()
        {
            return ChannelType.unlimit;
        }

        private void async_first_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.First();
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        private void async_last_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.Last();
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        public void AsyncFirst(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_first_(ntf, ntfSign);
                else SelfStrand().AddLast(() => async_first_(ntf, ntfSign));
            else SelfStrand().Post(() => async_first_(ntf, ntfSign));
        }

        public void AsyncLast(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_last_(ntf, ntfSign);
                else SelfStrand().AddLast(() => async_last_(ntf, ntfSign));
            else SelfStrand().Post(() => async_last_(ntf, ntfSign));
        }

        public Task UnsafeFirst(AsyncResultWrap<ChannelReceiveWrap<T>> res)
        {
            return Generator.unsafe_chan_first(res, this);
        }

        public ValueTask<ChannelReceiveWrap<T>> First()
        {
            return Generator.chan_first(this);
        }

        public Task UnsafeLast(AsyncResultWrap<ChannelReceiveWrap<T>> res)
        {
            return Generator.unsafe_chan_last(res, this);
        }

        public ValueTask<ChannelReceiveWrap<T>> Last()
        {
            return Generator.chan_last(this);
        }

        protected override void async_send_(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            _msgQueue.AddLast(msg);
            _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            ntf(new ChannelSendWrap { state = ChannelState.ok });
        }

        protected override void async_recv_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.RemoveFirst();
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                }));
            }
        }

        protected override void async_try_send_(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            async_send_(ntf, msg, ntfSign);
        }

        protected override void async_try_recv_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.RemoveFirst();
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        protected override void async_timed_send_(int ms, Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            async_send_(ntf, msg, ntfSign);
        }

        protected override void async_timed_recv_(int ms, Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.RemoveFirst();
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                PriorityQueueNode<NotifyPick> node = _recvQueue.AddLast(0, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        timer.Cancel();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                });
                ntfSign?.SetNode(node);
                timer.Timeout(ms, delegate ()
                {
                    _recvQueue.Remove(node).Invoke(ChannelState.overtime);
                });
            }
            else
            {
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                }));
            }
        }

        protected override void async_append_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms)
        {
            if (!_msgQueue.Empty)
            {
                ntfSign._success = true;
                ntf(ChannelState.ok);
            }
            else if (_closed)
            {
                ntf(ChannelState.closed);
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                ntfSign.SetNode(_recvQueue.AddLast(1, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        timer.Cancel();
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                timer.Timeout(ms, delegate ()
                {
                    _recvQueue.Remove(ntfSign._ntfNode).Invoke(ChannelState.overtime);
                });
            }
            else
            {
                ntfSign.SetNode(_recvQueue.AddLast(1, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
            }
        }

        protected override void async_try_recv_and_append_notify_(Action<ChannelReceiveWrap<T>> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, int ms)
        {
            ntfSign.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.RemoveFirst();
                if (!ntfSign._selectOnce)
                {
                    async_append_recv_notify_(msgNtf, ntfSign, ms);
                }
                cb(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                msgNtf(ChannelState.closed);
                cb(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                async_append_recv_notify_(msgNtf, ntfSign, ms);
                cb(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        protected override void async_remove_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            bool Effect = ntfSign._ntfNode.Effect;
            bool success = ntfSign.ResetSuccess();
            if (Effect)
            {
                _recvQueue.Remove(ntfSign._ntfNode).CancelTimer();
                ntfSign.ResetNode();
            }
            else if (success && !_msgQueue.Empty)
            {
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            ntf(Effect ? ChannelState.ok : ChannelState.Fail);
        }

        protected override void async_append_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms)
        {
            if (_closed)
            {
                ntf(ChannelState.closed);
                return;
            }
            ntfSign._success = true;
            ntf(ChannelState.ok);
        }

        protected override void async_try_send_and_append_notify_(Action<ChannelSendWrap> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, T msg, int ms)
        {
            ntfSign.ResetSuccess();
            if (_closed)
            {
                msgNtf(ChannelState.closed);
                cb(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            _msgQueue.AddLast(msg);
            _recvQueue.RemoveFirst();
            if (!ntfSign._selectOnce)
            {
                async_append_send_notify_(msgNtf, ntfSign, ms);
            }
            cb(new ChannelSendWrap { state = ChannelState.ok });
        }

        protected override void async_remove_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            ntf(ChannelState.Fail);
        }

        protected override void async_clear_(Action ntf)
        {
            _msgQueue.Clear();
            ntf();
        }

        protected override void async_close_(Action ntf, bool isClear = false)
        {
            _closed = true;
            if (isClear)
            {
                _msgQueue.Clear();
            }
            SafeCallback(ref _recvQueue, ChannelState.closed);
            ntf();
        }

        protected override void async_cancel_(Action ntf, bool isClear = false)
        {
            if (isClear)
            {
                _msgQueue.Clear();
            }
            SafeCallback(ref _recvQueue, ChannelState.Cancel);
            ntf();
        }
    }

    public class LimitChannel<T> : Channel<T>
    {
        MsgQueueAbs<T> _msgQueue;
        PriorityQueue<NotifyPick> _sendQueue;
        PriorityQueue<NotifyPick> _recvQueue;
        readonly int _maxCount;

        public LimitChannel(SharedStrand Strand, int len) : base(Strand)
        {
            Debug.Assert(len > 0, string.Format("LimitChannel<{0}>长度必须大于0!", typeof(T).Name));
            _msgQueue = default(T) is VoidType ? (MsgQueueAbs<T>)new VoidMsgQueue<T>() : new NoVoidMsgQueue<T>();
            _sendQueue = new PriorityQueue<NotifyPick>();
            _recvQueue = new PriorityQueue<NotifyPick>();
            _maxCount = len;
        }

        public LimitChannel(int len) : this(SharedStrand.DefaultStrand(), len) { }

        public override ChannelType GetChannelType()
        {
            return ChannelType.limit;
        }

        public void ForcePost(T msg)
        {
            AsyncForceSend(NilAction<ChannelState, bool, T>.action, msg);
        }

        public Action<T> WrapForce()
        {
            return ForcePost;
        }

        public Action WrapForceDefault()
        {
            return () => ForcePost(default(T));
        }

        protected override void async_send_(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            if (_msgQueue.Count == _maxCount)
            {
                ChannelNotifySign.SetNode(ntfSign, _sendQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_send_(ntf, msg, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelSendWrap { state = state });
                        }
                    }
                }));
            }
            else
            {
                _msgQueue.AddLast(msg);
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new ChannelSendWrap { state = ChannelState.ok });
            }
        }

        private void async_force_send_(Action<ChannelState, bool, T> ntf, T msg)
        {
            if (_closed)
            {
                ntf(ChannelState.closed, false, default(T));
                return;
            }
            bool hasOut = false;
            T outMsg = default(T);
            if (_msgQueue.Count == _maxCount)
            {
                hasOut = true;
                outMsg = _msgQueue.RemoveFirst();
            }
            _msgQueue.AddLast(msg);
            _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            if (hasOut)
            {
                ntf(ChannelState.ok, true, outMsg);
            }
            else
            {
                ntf(ChannelState.ok, false, default(T));
            }
        }

        private void async_timed_force_send_(int ms, Action<ChannelState, bool, T> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(ChannelState.closed, false, default(T));
                return;
            }
            if (_msgQueue.Count == _maxCount)
            {
                if (ms >= 0)
                {
                    AsyncTimer timer = new AsyncTimer(SelfStrand());
                    PriorityQueueNode<NotifyPick> node = _sendQueue.AddLast(0, new NotifyPick()
                    {
                        timer = timer,
                        ntf = delegate (ChannelState state)
                        {
                            ntfSign?.ResetNode();
                            timer.Cancel();
                            if (ChannelState.ok == state)
                            {
                                async_force_send_(ntf, msg);
                            }
                            else
                            {
                                ntf(state, false, default(T));
                            }
                        }
                    });
                    ntfSign?.SetNode(node);
                    timer.Timeout(ms, delegate ()
                    {
                        _sendQueue.Remove(node).Invoke(ChannelState.ok);
                    });
                }
                else
                {
                    ChannelNotifySign.SetNode(ntfSign, _sendQueue.AddLast(0, new NotifyPick()
                    {
                        ntf = delegate (ChannelState state)
                        {
                            ntfSign?.ResetNode();
                            if (ChannelState.ok == state)
                            {
                                async_force_send_(ntf, msg);
                            }
                            else
                            {
                                ntf(state, false, default(T));
                            }
                        }
                    }));
                }
            }
            else
            {
                _msgQueue.AddLast(msg);
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(ChannelState.ok, false, default(T));
            }
        }

        public void AsyncForceSend(Action<ChannelState, bool, T> ntf, T msg)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_force_send_(ntf, msg);
                else SelfStrand().AddLast(() => async_force_send_(ntf, msg));
            else SelfStrand().Post(() => async_force_send_(ntf, msg));
        }

        public void AsyncTimedForceSend(int ms, Action<ChannelState, bool, T> ntf, T msg, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_timed_force_send_(ms, ntf, msg, ntfSign);
                else SelfStrand().AddLast(() => async_timed_force_send_(ms, ntf, msg, ntfSign));
            else SelfStrand().Post(() => async_timed_force_send_(ms, ntf, msg, ntfSign));
        }

        public Task UnsafeForceSend(AsyncResultWrap<ChannelSendWrap> res, T msg, ChannelLostMsg<T> outMsg = null)
        {
            return Generator.unsafe_chan_force_send(res, this, msg, outMsg);
        }

        public ValueTask<ChannelSendWrap> ForceSend(T msg, ChannelLostMsg<T> outMsg = null, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_force_send(this, msg, outMsg, lostMsg);
        }

        public Task UnsafeTimedForceSend(int ms, AsyncResultWrap<ChannelSendWrap> res, T msg, ChannelLostMsg<T> outMsg = null)
        {
            return Generator.unsafe_chan_timed_force_send(ms, res, this, msg, outMsg);
        }

        public ValueTask<ChannelSendWrap> TimedForceSend(int ms, T msg, ChannelLostMsg<T> outMsg = null, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.chan_timed_force_send(ms, this, msg, outMsg, lostMsg);
        }

        private void async_first_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.First();
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        private void async_last_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.Last();
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        public void AsyncFirst(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_first_(ntf, ntfSign);
                else SelfStrand().AddLast(() => async_first_(ntf, ntfSign));
            else SelfStrand().Post(() => async_first_(ntf, ntfSign));
        }

        public void AsyncLast(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_last_(ntf, ntfSign);
                else SelfStrand().AddLast(() => async_last_(ntf, ntfSign));
            else SelfStrand().Post(() => async_last_(ntf, ntfSign));
        }

        public Task UnsafeFirst(AsyncResultWrap<ChannelReceiveWrap<T>> res)
        {
            return Generator.unsafe_chan_first(res, this);
        }

        public ValueTask<ChannelReceiveWrap<T>> First()
        {
            return Generator.chan_first(this);
        }

        public Task UnsafeLast(AsyncResultWrap<ChannelReceiveWrap<T>> res)
        {
            return Generator.unsafe_chan_last(res, this);
        }

        public ValueTask<ChannelReceiveWrap<T>> Last()
        {
            return Generator.chan_last(this);
        }

        protected override void async_recv_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.RemoveFirst();
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                }));
            }
        }

        protected override void async_try_send_(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            if (_msgQueue.Count == _maxCount)
            {
                ntf(new ChannelSendWrap { state = ChannelState.Fail });
            }
            else
            {
                _msgQueue.AddLast(msg);
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new ChannelSendWrap { state = ChannelState.ok });
            }
        }

        protected override void async_try_recv_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.RemoveFirst();
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        protected override void async_timed_send_(int ms, Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            if (_msgQueue.Count == _maxCount)
            {
                if (ms >= 0)
                {
                    AsyncTimer timer = new AsyncTimer(SelfStrand());
                    PriorityQueueNode<NotifyPick> node = _sendQueue.AddLast(0, new NotifyPick()
                    {
                        timer = timer,
                        ntf = delegate (ChannelState state)
                        {
                            ntfSign?.ResetNode();
                            timer.Cancel();
                            if (ChannelState.ok == state)
                            {
                                async_send_(ntf, msg, ntfSign);
                            }
                            else
                            {
                                ntf(new ChannelSendWrap { state = state });
                            }
                        }
                    });
                    ntfSign?.SetNode(node);
                    timer.Timeout(ms, delegate ()
                    {
                        _sendQueue.Remove(node).Invoke(ChannelState.overtime);
                    });
                }
                else
                {
                    ChannelNotifySign.SetNode(ntfSign, _sendQueue.AddLast(0, new NotifyPick()
                    {
                        ntf = delegate (ChannelState state)
                        {
                            ntfSign?.ResetNode();
                            if (ChannelState.ok == state)
                            {
                                async_send_(ntf, msg, ntfSign);
                            }
                            else
                            {
                                ntf(new ChannelSendWrap { state = state });
                            }
                        }
                    }));
                }
            }
            else
            {
                _msgQueue.AddLast(msg);
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new ChannelSendWrap { state = ChannelState.ok });
            }
        }

        protected override void async_timed_recv_(int ms, Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.RemoveFirst();
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                PriorityQueueNode<NotifyPick> node = _recvQueue.AddLast(0, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        timer.Cancel();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                });
                ntfSign?.SetNode(node);
                timer.Timeout(ms, delegate ()
                {
                    _recvQueue.Remove(node).Invoke(ChannelState.overtime);
                });
            }
            else
            {
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                }));
            }
        }

        protected override void async_append_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms)
        {
            if (!_msgQueue.Empty)
            {
                ntfSign._success = true;
                ntf(ChannelState.ok);
            }
            else if (_closed)
            {
                ntf(ChannelState.closed);
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                ntfSign.SetNode(_recvQueue.AddLast(1, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        timer.Cancel();
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                timer.Timeout(ms, delegate ()
                {
                    _recvQueue.Remove(ntfSign._ntfNode).Invoke(ChannelState.overtime);
                });
            }
            else
            {
                ntfSign.SetNode(_recvQueue.AddLast(1, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
            }
        }

        protected override void async_try_recv_and_append_notify_(Action<ChannelReceiveWrap<T>> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, int ms)
        {
            ntfSign.ResetSuccess();
            if (!_msgQueue.Empty)
            {
                T msg = _msgQueue.RemoveFirst();
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                if (!ntfSign._selectOnce)
                {
                    async_append_recv_notify_(msgNtf, ntfSign, ms);
                }
                cb(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                msgNtf(ChannelState.closed);
                cb(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                async_append_recv_notify_(msgNtf, ntfSign, ms);
                cb(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        protected override void async_remove_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            bool Effect = ntfSign._ntfNode.Effect;
            bool success = ntfSign.ResetSuccess();
            if (Effect)
            {
                _recvQueue.Remove(ntfSign._ntfNode).CancelTimer();
                ntfSign.ResetNode();
            }
            else if (success && !_msgQueue.Empty)
            {
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            ntf(Effect ? ChannelState.ok : ChannelState.Fail);
        }

        protected override void async_append_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms)
        {
            if (_closed)
            {
                ntf(ChannelState.closed);
                return;
            }
            if (_msgQueue.Count != _maxCount)
            {
                ntfSign._success = true;
                ntf(ChannelState.ok);
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                ntfSign.SetNode(_sendQueue.AddLast(1, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        timer.Cancel();
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                timer.Timeout(ms, delegate ()
                {
                    _sendQueue.Remove(ntfSign._ntfNode).Invoke(ChannelState.overtime);
                });
            }
            else
            {
                ntfSign.SetNode(_sendQueue.AddLast(1, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
            }
        }

        protected override void async_try_send_and_append_notify_(Action<ChannelSendWrap> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, T msg, int ms)
        {
            ntfSign.ResetSuccess();
            if (_closed)
            {
                msgNtf(ChannelState.closed);
                cb(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            if (_msgQueue.Count != _maxCount)
            {
                _msgQueue.AddLast(msg);
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
                if (!ntfSign._selectOnce)
                {
                    async_append_send_notify_(msgNtf, ntfSign, ms);
                }
                cb(new ChannelSendWrap { state = ChannelState.ok });
            }
            else
            {
                async_append_send_notify_(msgNtf, ntfSign, ms);
                cb(new ChannelSendWrap { state = ChannelState.Fail });
            }
        }

        protected override void async_remove_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            bool Effect = ntfSign._ntfNode.Effect;
            bool success = ntfSign.ResetSuccess();
            if (Effect)
            {
                _sendQueue.Remove(ntfSign._ntfNode).CancelTimer();
                ntfSign.ResetNode();
            }
            else if (success && _msgQueue.Count != _maxCount)
            {
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            ntf(Effect ? ChannelState.ok : ChannelState.Fail);
        }

        protected override void async_clear_(Action ntf)
        {
            _msgQueue.Clear();
            SafeCallback(ref _sendQueue, ChannelState.Fail);
            ntf();
        }

        protected override void async_close_(Action ntf, bool isClear = false)
        {
            _closed = true;
            if (isClear)
            {
                _msgQueue.Clear();
            }
            SafeCallback(ref _recvQueue, ref _sendQueue, ChannelState.closed);
            ntf();
        }

        protected override void async_cancel_(Action ntf, bool isClear = false)
        {
            if (isClear)
            {
                _msgQueue.Clear();
            }
            SafeCallback(ref _recvQueue, ref _sendQueue, ChannelState.Cancel);
            ntf();
        }
    }

    public class NilChannel<T> : Channel<T>
    {
        PriorityQueue<NotifyPick> _sendQueue;
        PriorityQueue<NotifyPick> _recvQueue;
        T _tempMsg;
        bool _isTrySend;
        bool _isTryRecv;
        bool _has;

        public NilChannel(SharedStrand Strand) : base(Strand)
        {
            _sendQueue = new PriorityQueue<NotifyPick>();
            _recvQueue = new PriorityQueue<NotifyPick>();
            _isTrySend = false;
            _isTryRecv = false;
            _has = false;
        }

        public NilChannel() : this(SharedStrand.DefaultStrand()) { }

        public override ChannelType GetChannelType()
        {
            return ChannelType.nil;
        }

        protected override void async_send_(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            if (_has || _recvQueue.Empty)
            {
                ChannelNotifySign.SetNode(ntfSign, _sendQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_send_(ntf, msg, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelSendWrap { state = state });
                        }
                    }
                }));
            }
            else
            {
                _tempMsg = msg;
                _has = true;
                ChannelNotifySign.SetNode(ntfSign, _sendQueue.AddFirst(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        ntf(new ChannelSendWrap { state = state });
                    }
                }));
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        protected override void async_recv_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_has)
            {
                T msg = _tempMsg;
                _has = false;
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                }));
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        protected override void async_try_send_(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            if (_has || _recvQueue.Empty)
            {
                ntf(new ChannelSendWrap { state = ChannelState.Fail });
            }
            else
            {
                _tempMsg = msg;
                _has = true;
                _isTrySend = true;
                ChannelNotifySign.SetNode(ntfSign, _sendQueue.AddFirst(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        _isTrySend = false;
                        ntfSign?.ResetNode();
                        ntf(new ChannelSendWrap { state = state });
                    }
                }));
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        protected override void async_try_recv_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_has)
            {
                T msg = _tempMsg;
                _has = false;
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else if (!_sendQueue.Empty && _recvQueue.Empty)
            {
                _isTryRecv = true;
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddFirst(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        _isTryRecv = false;
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                }));
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        protected override void async_timed_send_(int ms, Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            if (_has || _recvQueue.Empty)
            {
                if (ms >= 0)
                {
                    AsyncTimer timer = new AsyncTimer(SelfStrand());
                    PriorityQueueNode<NotifyPick> node = _sendQueue.AddLast(0, new NotifyPick()
                    {
                        timer = timer,
                        ntf = delegate (ChannelState state)
                        {
                            ntfSign?.ResetNode();
                            timer.Cancel();
                            if (ChannelState.ok == state)
                            {
                                async_send_(ntf, msg, ntfSign);
                            }
                            else
                            {
                                ntf(new ChannelSendWrap { state = state });
                            }
                        }
                    });
                    ntfSign?.SetNode(node);
                    timer.Timeout(ms, delegate ()
                    {
                        _sendQueue.Remove(node).Invoke(ChannelState.overtime);
                    });
                }
                else
                {
                    ChannelNotifySign.SetNode(ntfSign, _sendQueue.AddLast(0, new NotifyPick()
                    {
                        ntf = delegate (ChannelState state)
                        {
                            ntfSign?.ResetNode();
                            if (ChannelState.ok == state)
                            {
                                async_send_(ntf, msg, ntfSign);
                            }
                            else
                            {
                                ntf(new ChannelSendWrap { state = state });
                            }
                        }
                    }));
                }
            }
            else if (ms >= 0)
            {
                _tempMsg = msg;
                _has = true;
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                PriorityQueueNode<NotifyPick> node = _sendQueue.AddFirst(0, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        timer.Cancel();
                        ntf(new ChannelSendWrap { state = state });
                    }
                });
                ntfSign?.SetNode(node);
                timer.Timeout(ms, delegate ()
                {
                    _has = false;
                    _sendQueue.Remove(node).Invoke(ChannelState.overtime);
                });
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                _tempMsg = msg;
                _has = true;
                ChannelNotifySign.SetNode(ntfSign, _sendQueue.AddFirst(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        ntf(new ChannelSendWrap { state = state });
                    }
                }));
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        protected override void async_timed_recv_(int ms, Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_has)
            {
                T msg = _tempMsg;
                _has = false;
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                PriorityQueueNode<NotifyPick> node = _recvQueue.AddLast(0, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        timer.Cancel();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                });
                ntfSign?.SetNode(node);
                timer.Timeout(ms, delegate ()
                {
                    _recvQueue.Remove(node).Invoke(ChannelState.overtime);
                });
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                }));
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        protected override void async_append_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms)
        {
            if (_has)
            {
                ntfSign._success = true;
                ntf(ChannelState.ok);
            }
            else if (_closed)
            {
                ntf(ChannelState.closed);
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                ntfSign.SetNode(_recvQueue.AddLast(1, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        timer.Cancel();
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                timer.Timeout(ms, delegate ()
                {
                    _recvQueue.Remove(ntfSign._ntfNode).Invoke(ChannelState.overtime);
                });
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                ntfSign.SetNode(_recvQueue.AddLast(1, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        protected override void async_try_recv_and_append_notify_(Action<ChannelReceiveWrap<T>> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, int ms)
        {
            ntfSign.ResetSuccess();
            if (_has)
            {
                T msg = _tempMsg;
                _has = false;
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                if (!ntfSign._selectOnce)
                {
                    async_append_recv_notify_(msgNtf, ntfSign, ms);
                }
                cb(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = msg });
            }
            else if (_closed)
            {
                msgNtf(ChannelState.closed);
                cb(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else if (!_sendQueue.Empty && _recvQueue.Empty)
            {
                _isTryRecv = true;
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        _isTryRecv = false;
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(cb, ntfSign);
                        }
                        else
                        {
                            cb(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                }));
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                async_append_recv_notify_(msgNtf, ntfSign, ms);
                cb(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        protected override void async_remove_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            bool Effect = ntfSign._ntfNode.Effect;
            bool success = ntfSign.ResetSuccess();
            if (Effect)
            {
                _isTryRecv &= _recvQueue.First._node != ntfSign._ntfNode._node;
                _recvQueue.Remove(ntfSign._ntfNode).CancelTimer();
                ntfSign.ResetNode();
            }
            else if (success && _has)
            {
                if (!_recvQueue.RemoveFirst().Invoke(ChannelState.ok) && _isTrySend)
                {
                    _has = !_sendQueue.RemoveFirst().Invoke(ChannelState.Fail);
                }
            }
            ntf(Effect ? ChannelState.ok : ChannelState.Fail);
        }

        protected override void async_append_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms)
        {
            if (_closed)
            {
                ntf(ChannelState.closed);
                return;
            }
            if (!_recvQueue.Empty)
            {
                ntfSign._success = true;
                ntf(ChannelState.ok);
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                ntfSign.SetNode(_sendQueue.AddLast(1, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        timer.Cancel();
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                timer.Timeout(ms, delegate ()
                {
                    _sendQueue.Remove(ntfSign._ntfNode).Invoke(ChannelState.overtime);
                });
            }
            else
            {
                ntfSign.SetNode(_sendQueue.AddLast(1, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
            }
        }

        protected override void async_try_send_and_append_notify_(Action<ChannelSendWrap> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, T msg, int ms)
        {
            ntfSign.ResetSuccess();
            if (_closed)
            {
                msgNtf(ChannelState.closed);
                cb(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            if (!_has && !_recvQueue.Empty)
            {
                _has = true;
                _tempMsg = msg;
                _isTrySend = true;
                ntfSign.SetNode(_sendQueue.AddFirst(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        _isTrySend = false;
                        ntfSign.ResetNode();
                        if (!ntfSign._selectOnce)
                        {
                            async_append_send_notify_(msgNtf, ntfSign, ms);
                        }
                        cb(new ChannelSendWrap { state = state });
                    }
                }));
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                async_append_send_notify_(msgNtf, ntfSign, ms);
                cb(new ChannelSendWrap { state = ChannelState.Fail });
            }
        }

        protected override void async_remove_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            bool Effect = ntfSign._ntfNode.Effect;
            bool success = ntfSign.ResetSuccess();
            if (Effect)
            {
                if (_sendQueue.First._node == ntfSign._ntfNode._node)
                {
                    _isTrySend = _has = false;
                }
                _sendQueue.Remove(ntfSign._ntfNode).CancelTimer();
                ntfSign.ResetNode();
            }
            else if (success && !_has)
            {
                if (!_sendQueue.RemoveFirst().Invoke(ChannelState.ok) && _isTryRecv)
                {
                    _recvQueue.RemoveFirst().Invoke(ChannelState.Fail);
                }
            }
            ntf(Effect ? ChannelState.ok : ChannelState.Fail);
        }

        protected override void async_clear_(Action ntf)
        {
            _has = false;
            SafeCallback(ref _sendQueue, ChannelState.Fail);
            ntf();
        }

        protected override void async_close_(Action ntf, bool isClear = false)
        {
            _closed = true;
            _has = false;
            SafeCallback(ref _recvQueue, ref _sendQueue, ChannelState.closed);
            ntf();
        }

        protected override void async_cancel_(Action ntf, bool isClear = false)
        {
            _has = false;
            SafeCallback(ref _recvQueue, ref _sendQueue, ChannelState.Cancel);
            ntf();
        }
    }

    public class BroadcastToken
    {
        internal long _lastId = -1;
        internal static readonly BroadcastToken _defToken = new BroadcastToken();

        public void Reset()
        {
            _lastId = -1;
        }

        public bool IsDefault()
        {
            return this == _defToken;
        }
    }

    public class BroadcastChannel<T> : Channel<T>
    {
        PriorityQueue<NotifyPick> _recvQueue;
        T _msg;
        long _pushCount;
        bool _has;

        public BroadcastChannel(SharedStrand Strand) : base(Strand)
        {
            _recvQueue = new PriorityQueue<NotifyPick>();
            _pushCount = 0;
            _has = false;
        }

        public BroadcastChannel() : this(SharedStrand.DefaultStrand()) { }

        internal override SelectChannelBase MakeSelectReader(Func<T, Task> handler, BroadcastToken token, ChannelLostMsg<T> lostMsg)
        {
            return new SelectChannelReader() { _token = null != token ? token : new BroadcastToken(), _chan = this, _handler = handler, _lostMsg = lostMsg };
        }

        internal override SelectChannelBase MakeSelectReader(Func<T, Task> handler, BroadcastToken token, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<T> lostMsg)
        {
            return new SelectChannelReader() { _token = null != token ? token : new BroadcastToken(), _chan = this, _handler = handler, _errHandler = errHandler, _lostMsg = lostMsg };
        }

        internal override SelectChannelBase MakeSelectReader(int ms, Func<T, Task> handler, BroadcastToken token, ChannelLostMsg<T> lostMsg)
        {
            return new SelectChannelReader() { _chanTimeout = ms, _token = null != token ? token : new BroadcastToken(), _chan = this, _handler = handler, _lostMsg = lostMsg };
        }

        internal override SelectChannelBase MakeSelectReader(int ms, Func<T, Task> handler, BroadcastToken token, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<T> lostMsg)
        {
            return new SelectChannelReader() { _chanTimeout = ms, _token = null != token ? token : new BroadcastToken(), _chan = this, _handler = handler, _errHandler = errHandler, _lostMsg = lostMsg };
        }

        public override ChannelType GetChannelType()
        {
            return ChannelType.broadcast;
        }

        protected override void async_send_(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            _pushCount++;
            _msg = msg;
            _has = true;
            SafeCallback(ref _recvQueue, ChannelState.ok);
            ntf(new ChannelSendWrap { state = ChannelState.ok });
        }

        protected override void async_recv_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            async_recv_(ntf, BroadcastToken._defToken, ntfSign);
        }

        protected override void async_recv_(Action<ChannelReceiveWrap<T>> ntf, BroadcastToken token, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_has && token._lastId != _pushCount)
            {
                if (!token.IsDefault())
                {
                    token._lastId = _pushCount;
                }
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = _msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, token, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                }));
            }
        }

        protected override void async_try_send_(Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            async_send_(ntf, msg, ntfSign);
        }

        protected override void async_try_recv_(Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            async_try_recv_(ntf, BroadcastToken._defToken, ntfSign);
        }

        protected override void async_try_recv_(Action<ChannelReceiveWrap<T>> ntf, BroadcastToken token, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_has && token._lastId != _pushCount)
            {
                if (!token.IsDefault())
                {
                    token._lastId = _pushCount;
                }
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = _msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        protected override void async_timed_send_(int ms, Action<ChannelSendWrap> ntf, T msg, ChannelNotifySign ntfSign)
        {
            async_send_(ntf, msg, ntfSign);
        }

        protected override void async_timed_recv_(int ms, Action<ChannelReceiveWrap<T>> ntf, ChannelNotifySign ntfSign)
        {
            async_timed_recv_(ms, ntf, BroadcastToken._defToken, ntfSign);
        }

        protected override void async_timed_recv_(int ms, Action<ChannelReceiveWrap<T>> ntf, BroadcastToken token, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            _timed_check_pop(SystemTick.GetTickMs() + ms, ntf, token, ntfSign);
        }

        void _timed_check_pop(long deadms, Action<ChannelReceiveWrap<T>> ntf, BroadcastToken token, ChannelNotifySign ntfSign)
        {
            if (_has && token._lastId != _pushCount)
            {
                if (!token.IsDefault())
                {
                    token._lastId = _pushCount;
                }
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = _msg });
            }
            else if (_closed)
            {
                ntf(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                PriorityQueueNode<NotifyPick> node = _recvQueue.AddLast(0, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        timer.Cancel();
                        if (ChannelState.ok == state)
                        {
                            _timed_check_pop(deadms, ntf, token, ntfSign);
                        }
                        else
                        {
                            ntf(new ChannelReceiveWrap<T> { state = state });
                        }
                    }
                });
                ntfSign?.SetNode(node);
                timer.Deadline(deadms, delegate ()
                {
                    _recvQueue.Remove(node).Invoke(ChannelState.overtime);
                });
            }
        }

        protected override void async_append_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms)
        {
            async_append_recv_notify_(ntf, BroadcastToken._defToken, ntfSign, ms);
        }

        protected override void async_append_recv_notify_(Action<ChannelState> ntf, BroadcastToken token, ChannelNotifySign ntfSign, int ms)
        {
            _append_recv_notify(ntf, token, ntfSign, ms);
        }

        bool _append_recv_notify(Action<ChannelState> ntf, BroadcastToken token, ChannelNotifySign ntfSign, int ms)
        {
            if (_has && token._lastId != _pushCount)
            {
                if (!token.IsDefault())
                {
                    token._lastId = _pushCount;
                }
                ntfSign._success = true;
                ntf(ChannelState.ok);
                return true;
            }
            else if (_closed)
            {
                return false;
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                ntfSign.SetNode(_recvQueue.AddLast(1, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        timer.Cancel();
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                timer.Timeout(ms, delegate ()
                {
                    _recvQueue.Remove(ntfSign._ntfNode).Invoke(ChannelState.overtime);
                });
                return false;
            }
            else
            {
                ntfSign.SetNode(_recvQueue.AddLast(1, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                return false;
            }
        }

        protected override void async_try_recv_and_append_notify_(Action<ChannelReceiveWrap<T>> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, int ms)
        {
            async_try_recv_and_append_notify_(cb, msgNtf, BroadcastToken._defToken, ntfSign, ms);
        }

        protected override void async_try_recv_and_append_notify_(Action<ChannelReceiveWrap<T>> cb, Action<ChannelState> msgNtf, BroadcastToken token, ChannelNotifySign ntfSign, int ms = -1)
        {
            ntfSign.ResetSuccess();
            if (_append_recv_notify(msgNtf, token, ntfSign, ms))
            {
                cb(new ChannelReceiveWrap<T> { state = ChannelState.ok, msg = _msg });
            }
            else if (_closed)
            {
                msgNtf(ChannelState.closed);
                cb(new ChannelReceiveWrap<T> { state = ChannelState.closed });
            }
            else
            {
                cb(new ChannelReceiveWrap<T> { state = ChannelState.Fail });
            }
        }

        protected override void async_remove_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            bool Effect = ntfSign._ntfNode.Effect;
            bool success = ntfSign.ResetSuccess();
            if (Effect)
            {
                _recvQueue.Remove(ntfSign._ntfNode).CancelTimer();
                ntfSign.ResetNode();
            }
            else if (success && _has)
            {
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            ntf(Effect ? ChannelState.ok : ChannelState.Fail);
        }

        protected override void async_append_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms)
        {
            if (_closed)
            {
                ntf(ChannelState.closed);
                return;
            }
            ntfSign._success = true;
            ntf(ChannelState.ok);
        }

        protected override void async_try_send_and_append_notify_(Action<ChannelSendWrap> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, T msg, int ms)
        {
            ntfSign.ResetSuccess();
            if (_closed)
            {
                msgNtf(ChannelState.closed);
                cb(new ChannelSendWrap { state = ChannelState.closed });
                return;
            }
            _pushCount++;
            _msg = msg;
            _has = true;
            msgNtf(ChannelState.ok);
            cb(new ChannelSendWrap { state = ChannelState.ok });
        }

        protected override void async_remove_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            ntf(ChannelState.Fail);
        }

        protected override void async_clear_(Action ntf)
        {
            _has = false;
            ntf();
        }

        protected override void async_close_(Action ntf, bool isClear = false)
        {
            _closed = true;
            _has &= !isClear;
            SafeCallback(ref _recvQueue, ChannelState.closed);
            ntf();
        }

        protected override void async_cancel_(Action ntf, bool isClear = false)
        {
            _has &= !isClear;
            SafeCallback(ref _recvQueue, ChannelState.Cancel);
            ntf();
        }
    }

    public class CspChannel<R, T> : ChannelBase
    {
        struct SendPick
        {
            public Action<CspInvokeWrap<R>> _notify;
            public T _msg;
            public bool _has;
            public bool _isTryMsg;
            public int _invokeMs;
            AsyncTimer _timer;

            public void Set(Action<CspInvokeWrap<R>> ntf, T msg, AsyncTimer timer, int ms = -1)
            {
                _notify = ntf;
                _msg = msg;
                _has = true;
                _invokeMs = ms;
                _timer = timer;
            }

            public void Set(Action<CspInvokeWrap<R>> ntf, T msg, int ms = -1)
            {
                _notify = ntf;
                _msg = msg;
                _has = true;
                _invokeMs = ms;
                _timer = null;
            }

            public Action<CspInvokeWrap<R>> Cancel()
            {
                _isTryMsg = _has = false;
                _timer?.Cancel();
                return _notify;
            }
        }

        public class CspResult
        {
            internal int _invokeMs;
            internal Action<CspInvokeWrap<R>> _notify;
            AsyncTimer _invokeTimer;
#if DEBUG
            SharedStrand _hostStrand;
#endif

            internal CspResult(int ms, Action<CspInvokeWrap<R>> notify)
            {
                _invokeMs = ms;
                _notify = notify;
                _invokeTimer = null;
            }

            internal void StartInvokeTimer(Generator host)
            {
#if DEBUG
                _hostStrand = host.Strand;
#endif
                if (_invokeMs >= 0)
                {
                    _invokeTimer = new AsyncTimer(host.Strand);
                    _invokeTimer.Timeout(_invokeMs, Fail);
                }
            }

            public bool Complete(R res)
            {
#if DEBUG
                if (null != _hostStrand)
                {
                    Debug.Assert(_hostStrand.RunningInThisThread(), "不正确的 Complete 调用!");
                }
#endif
                _invokeTimer?.Cancel();
                _invokeTimer = null;
                if (null != _notify)
                {
                    Action<CspInvokeWrap<R>> ntf = _notify;
                    _notify = null;
                    ntf.Invoke(new CspInvokeWrap<R> { state = ChannelState.ok, result = res });
                    return true;
                }
                return false;
            }

            public void Fail()
            {
#if DEBUG
                if (null != _hostStrand)
                {
                    Debug.Assert(_hostStrand.RunningInThisThread(), "不正确的 Fail 调用!");
                }
#endif
                _invokeTimer?.Cancel();
                _invokeTimer = null;
                Action<CspInvokeWrap<R>> ntf = _notify;
                _notify = null;
                ntf?.Invoke(new CspInvokeWrap<R> { state = ChannelState.csp_fail });
            }
        }

        internal class SelectCspReader : SelectChannelBase
        {
            public CspChannel<R, T> _chan;
            public Func<T, Task<R>> _handler;
            public Func<T, ValueTask<R>> _gohandler;
            public Func<ChannelState, Task<bool>> _errHandler;
            public ChannelLostMsg<CspWaitWrap<R, T>> _lostMsg;
            public int _chanTimeout = -1;
            AsyncResultWrap<CspWaitWrap<R, T>> _tryRecvRes;
            Generator _host;

            public override void Begin(Generator host)
            {
                ntfSign._disable = false;
                _host = host;
                _tryRecvRes = new AsyncResultWrap<CspWaitWrap<R, T>> { value1 = CspWaitWrap<R, T>.def };
                if (enable)
                {
                    _chan.AsyncAppendRecvNotify(NextSelect, ntfSign, _chanTimeout);
                }
            }

            public override async Task<SelectChannelState> Invoke(Func<Task> stepOne)
            {
                try
                {
                    _tryRecvRes.value1 = CspWaitWrap<R, T>.def;
                    _chan.AsyncTryRecvAndAppendNotify(_host.unsafe_async_result(_tryRecvRes), NextSelect, ntfSign, _chanTimeout);
                    await _host.async_wait();
                }
                catch (Generator.StopException)
                {
                    _chan.AsyncRemoveRecvNotify(_host.unsafe_async_ignore<ChannelState>(), ntfSign);
                    await _host.async_wait();
                    if (ChannelState.ok == _tryRecvRes.value1.state)
                    {
                        if (null != _lostMsg)
                        {
                            _lostMsg.set(_tryRecvRes.value1);
                        }
                        else
                        {
                            _tryRecvRes.value1.Fail();
                        }
                    }
                    throw;
                }
                SelectChannelState chanState = new SelectChannelState() { Failed = false, NextRound = true };
                if (ChannelState.ok == _tryRecvRes.value1.state)
                {
                    bool invoked = false;
                    try
                    {
                        if (null != stepOne)
                        {
                            await stepOne();
                        }
                        _tryRecvRes.value1.result.StartInvokeTimer(_host);
                        await _host.unlock_suspend_();
                        invoked = true;
                        _tryRecvRes.value1.Complete(null != _handler ? await _handler(_tryRecvRes.value1.msg) : await _gohandler(_tryRecvRes.value1.msg));
                    }
                    catch (CspFailException)
                    {
                        _tryRecvRes.value1.Fail();
                    }
                    catch (Generator.SelectStopAllException)
                    {
                        _tryRecvRes.value1.Fail();
                        throw;
                    }
                    catch (Generator.SelectStopCurrentException)
                    {
                        _tryRecvRes.value1.Fail();
                        throw;
                    }
                    catch (Generator.StopException)
                    {
                        if (!invoked && null != _lostMsg)
                        {
                            _lostMsg.set(_tryRecvRes.value1);
                        }
                        else
                        {
                            _tryRecvRes.value1.Fail();
                        }
                        throw;
                    }
                    catch (System.Exception)
                    {
                        _tryRecvRes.value1.Fail();
                        throw;
                    }
                    finally
                    {
                        _host.lock_suspend_();
                    }
                }
                else if (ChannelState.closed == _tryRecvRes.value1.state)
                {
                    await End();
                    chanState.Failed = true;
                }
                else
                {
                    chanState.Failed = true;
                }
                chanState.NextRound = !ntfSign._disable;
                return chanState;
            }

            private async Task<bool> errInvoke_(ChannelState state)
            {
                try
                {
                    await _host.unlock_suspend_();
                    if (!await _errHandler(state) && ChannelState.closed != state)
                    {
                        _chan.AsyncAppendRecvNotify(NextSelect, ntfSign, _chanTimeout);
                        return false;
                    }
                }
                finally
                {
                    _host.lock_suspend_();
                }
                return true;
            }

            public override ValueTask<bool> ErrInvoke(ChannelState state)
            {
                if (null != _errHandler)
                {
                    return new ValueTask<bool>(errInvoke_(state));
                }
                return new ValueTask<bool>(true);
            }

            public override Task End()
            {
                ntfSign._disable = true;
                _chan.AsyncRemoveRecvNotify(_host.unsafe_async_ignore<ChannelState>(), ntfSign);
                return _host.async_wait();
            }

            public override bool IsRead()
            {
                return true;
            }

            public override ChannelBase Channel()
            {
                return _chan;
            }
        }

        internal class SelectCspWriter : SelectChannelBase
        {
            public CspChannel<R, T> _chan;
            public AsyncResultWrap<T> _msg;
            public Func<R, Task> _handler;
            public Func<ChannelState, Task<bool>> _errHandler;
            public Action<CspInvokeWrap<R>> _lostRes;
            public ChannelLostMsg<T> _lostMsg;
            public int _chanTimeout = -1;
            AsyncResultWrap<CspInvokeWrap<R>> _trySendRes;
            Generator _host;

            public override void Begin(Generator host)
            {
                ntfSign._disable = false;
                _host = host;
                _trySendRes = new AsyncResultWrap<CspInvokeWrap<R>> { value1 = CspInvokeWrap<R>.def };
                if (enable)
                {
                    _chan.AsyncAppendSendNotify(NextSelect, ntfSign, _chanTimeout);
                }
            }

            public override async Task<SelectChannelState> Invoke(Func<Task> stepOne)
            {
                try
                {
                    _trySendRes.value1 = CspInvokeWrap<R>.def;
                    _chan.AsyncTrySendAndAppendNotify(null == _lostRes ? _host.unsafe_async_result(_trySendRes) : _host.async_result(_trySendRes, _lostRes), NextSelect, ntfSign, _msg.value1, _chanTimeout);
                    await _host.async_wait();
                }
                catch (Generator.StopException)
                {
                    ChannelState rmState = ChannelState.undefined;
                    _chan.AsyncRemoveSendNotify(null == _lostMsg ? _host.unsafe_async_ignore<ChannelState>() : _host.unsafe_async_callback((ChannelState state) => rmState = state), ntfSign);
                    await _host.async_wait();
                    if (ChannelState.ok == rmState)
                    {
                        _lostMsg?.set(_msg.value1);
                    }
                    throw;
                }
                SelectChannelState chanState = new SelectChannelState() { Failed = false, NextRound = true };
                if (ChannelState.ok == _trySendRes.value1.state)
                {
                    bool invoked = false;
                    try
                    {
                        if (null != stepOne)
                        {
                            await stepOne();
                        }
                        await _host.unlock_suspend_();
                        invoked = true;
                        await _handler(_trySendRes.value1.result);
                    }
                    catch (Generator.StopException)
                    {
                        if (!invoked)
                        {
                            _lostRes?.Invoke(_trySendRes.value1);
                        }
                        throw;
                    }
                    finally
                    {
                        _host.lock_suspend_();
                    }
                }
                else if (ChannelState.closed == _trySendRes.value1.state)
                {
                    await End();
                    chanState.Failed = true;
                }
                else
                {
                    chanState.Failed = true;
                }
                chanState.NextRound = !ntfSign._disable;
                return chanState;
            }

            private async Task<bool> errInvoke_(ChannelState state)
            {
                try
                {
                    await _host.unlock_suspend_();
                    if (!await _errHandler(state) && ChannelState.closed != state)
                    {
                        _chan.AsyncAppendSendNotify(NextSelect, ntfSign, _chanTimeout);
                        return false;
                    }
                }
                finally
                {
                    _host.lock_suspend_();
                }
                return true;
            }

            public override ValueTask<bool> ErrInvoke(ChannelState state)
            {
                if (null != _errHandler)
                {
                    return new ValueTask<bool>(errInvoke_(state));
                }
                return new ValueTask<bool>(true);
            }

            public override Task End()
            {
                ntfSign._disable = true;
                _chan.AsyncRemoveSendNotify(_host.unsafe_async_ignore<ChannelState>(), ntfSign);
                return _host.async_wait();
            }

            public override bool IsRead()
            {
                return false;
            }

            public override ChannelBase Channel()
            {
                return _chan;
            }
        }

        PriorityQueue<NotifyPick> _sendQueue;
        PriorityQueue<NotifyPick> _recvQueue;
        SendPick _tempMsg;
        bool _isTryRecv;

        public CspChannel(SharedStrand Strand) : base(Strand)
        {
            _sendQueue = new PriorityQueue<NotifyPick>();
            _recvQueue = new PriorityQueue<NotifyPick>();
            _tempMsg.Cancel();
            _isTryRecv = false;
        }

        public CspChannel() : this(SharedStrand.DefaultStrand()) { }

        internal SelectChannelBase MakeSelectReader(Func<T, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return new SelectCspReader() { _chan = this, _handler = handler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectReader(Func<T, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return new SelectCspReader() { _chan = this, _handler = handler, _errHandler = errHandler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectReader(int ms, Func<T, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return new SelectCspReader() { _chanTimeout = ms, _chan = this, _handler = handler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectReader(int ms, Func<T, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return new SelectCspReader() { _chanTimeout = ms, _chan = this, _handler = handler, _errHandler = errHandler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectReader(Func<T, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return new SelectCspReader() { _chan = this, _gohandler = handler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectReader(Func<T, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return new SelectCspReader() { _chan = this, _gohandler = handler, _errHandler = errHandler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectReader(int ms, Func<T, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return new SelectCspReader() { _chanTimeout = ms, _chan = this, _gohandler = handler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectReader(int ms, Func<T, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return new SelectCspReader() { _chanTimeout = ms, _chan = this, _gohandler = handler, _errHandler = errHandler, _lostMsg = lostMsg };
        }

        internal SelectChannelBase MakeSelectWriter(int ms, AsyncResultWrap<T> msg, Func<R, Task> handler, Func<ChannelState, Task<bool>> errHandler, Action<R> lostRes, ChannelLostMsg<T> lostMsg)
        {
            return new SelectCspWriter()
            {
                _chanTimeout = ms,
                _chan = this,
                _msg = msg,
                _handler = handler,
                _errHandler = errHandler,
                _lostMsg = lostMsg,
                _lostRes = null == lostRes ? (Action<CspInvokeWrap<R>>)null : delegate (CspInvokeWrap<R> cspRes)
                {
                    if (ChannelState.ok == cspRes.state)
                    {
                        lostRes(cspRes.result);
                    }
                }
            };
        }

        internal SelectChannelBase MakeSelectWriter(AsyncResultWrap<T> msg, Func<R, Task> handler, Func<ChannelState, Task<bool>> errHandler, Action<R> lostRes, ChannelLostMsg<T> lostMsg)
        {
            return MakeSelectWriter(-1, msg, handler, errHandler, lostRes, lostMsg);
        }

        internal SelectChannelBase MakeSelectWriter(T msg, Func<R, Task> handler, Func<ChannelState, Task<bool>> errHandler, Action<R> lostRes, ChannelLostMsg<T> lostMsg)
        {
            return MakeSelectWriter(-1, new AsyncResultWrap<T> { value1 = msg }, handler, errHandler, lostRes, lostMsg);
        }

        internal SelectChannelBase MakeSelectWriter(int ms, T msg, Func<R, Task> handler, Func<ChannelState, Task<bool>> errHandler, Action<R> lostRes, ChannelLostMsg<T> lostMsg)
        {
            return MakeSelectWriter(ms, new AsyncResultWrap<T> { value1 = msg }, handler, errHandler, lostRes, lostMsg);
        }

        public override ChannelType GetChannelType()
        {
            return ChannelType.csp;
        }

        public void AsyncSend(Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_send_(ntf, msg, ntfSign);
                else SelfStrand().AddLast(() => async_send_(ntf, msg, ntfSign));
            else SelfStrand().Post(() => async_send_(ntf, msg, ntfSign));
        }

        public void AsyncSend(int invokeMs, Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_send_(invokeMs, ntf, msg, ntfSign);
                else SelfStrand().AddLast(() => async_send_(invokeMs, ntf, msg, ntfSign));
            else SelfStrand().Post(() => async_send_(invokeMs, ntf, msg, ntfSign));
        }

        public void AsyncRecv(Action<CspWaitWrap<R, T>> ntf, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_recv_(ntf, ntfSign);
                else SelfStrand().AddLast(() => async_recv_(ntf, ntfSign));
            else SelfStrand().Post(() => async_recv_(ntf, ntfSign));
        }

        public void AsyncTrySend(Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_send_(ntf, msg, ntfSign);
                else SelfStrand().AddLast(() => async_try_send_(ntf, msg, ntfSign));
            else SelfStrand().Post(() => async_try_send_(ntf, msg, ntfSign));
        }

        public void AsyncTrySend(int invokeMs, Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_send_(invokeMs, ntf, msg, ntfSign);
                else SelfStrand().AddLast(() => async_try_send_(invokeMs, ntf, msg, ntfSign));
            else SelfStrand().Post(() => async_try_send_(invokeMs, ntf, msg, ntfSign));
        }

        public void AsyncTryRecv(Action<CspWaitWrap<R, T>> ntf, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_recv_(ntf, ntfSign);
                else SelfStrand().AddLast(() => async_try_recv_(ntf, ntfSign));
            else SelfStrand().Post(() => async_try_recv_(ntf, ntfSign));
        }

        public void AsyncTimedSend(int ms, Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_timed_send_(ms, ntf, msg, ntfSign);
                else SelfStrand().AddLast(() => async_timed_send_(ms, ntf, msg, ntfSign));
            else SelfStrand().Post(() => async_timed_send_(ms, ntf, msg, ntfSign));
        }

        public void AsyncTimedSend(int ms, int invokeMs, Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_timed_send_(ms, invokeMs, ntf, msg, ntfSign);
                else SelfStrand().AddLast(() => async_timed_send_(ms, invokeMs, ntf, msg, ntfSign));
            else SelfStrand().Post(() => async_timed_send_(ms, invokeMs, ntf, msg, ntfSign));
        }

        public void AsyncTimedRecv(int ms, Action<CspWaitWrap<R, T>> ntf, ChannelNotifySign ntfSign = null)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_timed_recv_(ms, ntf, ntfSign);
                else SelfStrand().AddLast(() => async_timed_recv_(ms, ntf, ntfSign));
            else SelfStrand().Post(() => async_timed_recv_(ms, ntf, ntfSign));
        }

        public void AsyncTryRecvAndAppendNotify(Action<CspWaitWrap<R, T>> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, int ms = -1)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_recv_and_append_notify_(cb, msgNtf, ntfSign, ms);
                else SelfStrand().AddLast(() => async_try_recv_and_append_notify_(cb, msgNtf, ntfSign, ms));
            else SelfStrand().Post(() => async_try_recv_and_append_notify_(cb, msgNtf, ntfSign, ms));
        }

        public void AsyncTrySendAndAppendNotify(Action<CspInvokeWrap<R>> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, T msg, int ms = -1)
        {
            if (SelfStrand().RunningInThisThread())
                if (!_mustTick) async_try_send_and_append_notify_(cb, msgNtf, ntfSign, msg, ms);
                else SelfStrand().AddLast(() => async_try_send_and_append_notify_(cb, msgNtf, ntfSign, msg, ms));
            else SelfStrand().Post(() => async_try_send_and_append_notify_(cb, msgNtf, ntfSign, msg, ms));
        }

        public Task UnsafeInvoke(AsyncResultWrap<CspInvokeWrap<R>> res, T msg, int invokeMs = -1)
        {
            return Generator.unsafe_csp_invoke(res, this, msg, invokeMs);
        }

        public ValueTask<CspInvokeWrap<R>> Invoke(T msg, int invokeMs = -1, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.csp_invoke(this, msg, invokeMs, lostRes, lostMsg);
        }

        public DelayCsp<R> WrapInvoke(T msg, int invokeMs = -1, Action<R> lostRes = null)
        {
            return Generator.wrap_csp_invoke(this, msg, invokeMs, lostRes);
        }

        public Task UnsafeWait(AsyncResultWrap<CspWaitWrap<R, T>> res)
        {
            return Generator.unsafe_csp_wait(res, this);
        }

        public ValueTask<CspWaitWrap<R, T>> Wait(ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return Generator.csp_wait(this, lostMsg);
        }

        public ValueTask<ChannelState> Wait(Func<T, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return Generator.csp_wait(this, handler, lostMsg);
        }

        public ValueTask<ChannelState> Wait(Func<T, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return Generator.csp_wait(this, handler, lostMsg);
        }

        public Task UnsafeTryInvoke(AsyncResultWrap<CspInvokeWrap<R>> res, T msg, int invokeMs = -1)
        {
            return Generator.unsafe_csp_try_invoke(res, this, msg, invokeMs);
        }

        public ValueTask<CspInvokeWrap<R>> TryInvoke(T msg, int invokeMs = -1, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.csp_try_invoke(this, msg, invokeMs, lostRes, lostMsg);
        }

        public DelayCsp<R> WrapTryInvoke(T msg, int invokeMs = -1, Action<R> lostRes = null)
        {
            return Generator.wrap_csp_try_invoke(this, msg, invokeMs, lostRes);
        }

        public Task UnsafeTryWait(AsyncResultWrap<CspWaitWrap<R, T>> res)
        {
            return Generator.unsafe_csp_try_wait(res, this);
        }

        public ValueTask<CspWaitWrap<R, T>> TryWait(ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return Generator.csp_try_wait(this, lostMsg);
        }

        public ValueTask<ChannelState> TryWait(Func<T, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return Generator.csp_try_wait(this, handler, lostMsg);
        }

        public ValueTask<ChannelState> TryWait(Func<T, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return Generator.csp_try_wait(this, handler, lostMsg);
        }

        public Task UnsafeTimedInvoke(AsyncResultWrap<CspInvokeWrap<R>> res, TupleEx<int, int> ms, T msg)
        {
            return Generator.unsafe_csp_timed_invoke(res, this, ms, msg);
        }

        public ValueTask<CspInvokeWrap<R>> TimedInvoke(TupleEx<int, int> ms, T msg, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.csp_timed_invoke(this, ms, msg, lostRes, lostMsg);
        }

        public DelayCsp<R> WrapTimedInvoke(TupleEx<int, int> ms, T msg, Action<R> lostRes = null)
        {
            return Generator.wrap_csp_timed_invoke(this, ms, msg, lostRes);
        }

        public Task UnsafeTimedInvoke(AsyncResultWrap<CspInvokeWrap<R>> res, int ms, T msg)
        {
            return Generator.unsafe_csp_timed_invoke(res, this, ms, msg);
        }

        public ValueTask<CspInvokeWrap<R>> TimedInvoke(int ms, T msg, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
        {
            return Generator.csp_timed_invoke(this, ms, msg, lostRes, lostMsg);
        }

        public DelayCsp<R> WrapTimedInvoke(int ms, T msg, Action<R> lostRes = null)
        {
            return Generator.wrap_csp_timed_invoke(this, ms, msg, lostRes);
        }

        public Task UnsafeTimedWait(AsyncResultWrap<CspWaitWrap<R, T>> res, int ms)
        {
            return Generator.unsafe_csp_timed_wait(res, this, ms);
        }

        public ValueTask<CspWaitWrap<R, T>> TimedWait(int ms, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return Generator.csp_timed_wait(this, ms, lostMsg);
        }

        public ValueTask<ChannelState> TimedWait(int ms, Func<T, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return Generator.csp_timed_wait(this, ms, handler, lostMsg);
        }

        public ValueTask<ChannelState> TimedWait(int ms, Func<T, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return Generator.csp_timed_wait(this, ms, handler, lostMsg);
        }

        private void async_send_(Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign)
        {
            async_send_(-1, ntf, msg, ntfSign);
        }

        private void async_send_(int invokeMs, Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new CspInvokeWrap<R> { state = ChannelState.closed });
                return;
            }
            if (_tempMsg._has || _recvQueue.Empty)
            {
                ChannelNotifySign.SetNode(ntfSign, _sendQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_send_(invokeMs, ntf, msg, ntfSign);
                        }
                        else
                        {
                            ntf(new CspInvokeWrap<R> { state = state });
                        }
                    }
                }));
            }
            else
            {
                _tempMsg.Set(ntf, msg, invokeMs);
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        private void async_recv_(Action<CspWaitWrap<R, T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_tempMsg._has)
            {
                SendPick msg = _tempMsg;
                _tempMsg.Cancel();
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new CspWaitWrap<R, T> { state = ChannelState.ok, msg = msg._msg, result = new CspResult(msg._invokeMs, msg._notify) });
            }
            else if (_closed)
            {
                ntf(new CspWaitWrap<R, T> { state = ChannelState.closed });
            }
            else
            {
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new CspWaitWrap<R, T> { state = state });
                        }
                    }
                }));
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        private void async_try_send_(Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign)
        {
            async_try_send_(-1, ntf, msg, ntfSign);
        }

        private void async_try_send_(int invokeMs, Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new CspInvokeWrap<R> { state = ChannelState.closed });
                return;
            }
            if (_tempMsg._has || _recvQueue.Empty)
            {
                ntf(new CspInvokeWrap<R> { state = ChannelState.Fail });
            }
            else
            {
                _tempMsg.Set(ntf, msg, invokeMs);
                _tempMsg._isTryMsg = true;
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        private void async_try_recv_(Action<CspWaitWrap<R, T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_tempMsg._has)
            {
                SendPick msg = _tempMsg;
                _tempMsg.Cancel();
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new CspWaitWrap<R, T> { state = ChannelState.ok, msg = msg._msg, result = new CspResult(msg._invokeMs, msg._notify) });
            }
            else if (_closed)
            {
                ntf(new CspWaitWrap<R, T> { state = ChannelState.closed });
            }
            else if (!_sendQueue.Empty && _recvQueue.Empty)
            {
                _isTryRecv = true;
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddFirst(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        _isTryRecv = false;
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new CspWaitWrap<R, T> { state = state });
                        }
                    }
                }));
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                ntf(new CspWaitWrap<R, T> { state = ChannelState.Fail });
            }
        }

        private void async_timed_send_(int ms, Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign)
        {
            async_timed_send_(ms, -1, ntf, msg, ntfSign);
        }

        private void async_timed_send_(int ms, int invokeMs, Action<CspInvokeWrap<R>> ntf, T msg, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_closed)
            {
                ntf(new CspInvokeWrap<R> { state = ChannelState.closed });
                return;
            }
            if (_tempMsg._has || _recvQueue.Empty)
            {
                if (ms >= 0)
                {
                    AsyncTimer timer = new AsyncTimer(SelfStrand());
                    PriorityQueueNode<NotifyPick> node = _sendQueue.AddLast(0, new NotifyPick()
                    {
                        timer = timer,
                        ntf = delegate (ChannelState state)
                        {
                            ntfSign?.ResetNode();
                            timer.Cancel();
                            if (ChannelState.ok == state)
                            {
                                async_send_(invokeMs, ntf, msg, ntfSign);
                            }
                            else
                            {
                                ntf(new CspInvokeWrap<R> { state = state });
                            }
                        }
                    });
                    ntfSign?.SetNode(node);
                    timer.Timeout(ms, delegate ()
                    {
                        _sendQueue.Remove(node).Invoke(ChannelState.overtime);
                    });
                }
                else
                {
                    ChannelNotifySign.SetNode(ntfSign, _sendQueue.AddLast(0, new NotifyPick()
                    {
                        ntf = delegate (ChannelState state)
                        {
                            ntfSign?.ResetNode();
                            if (ChannelState.ok == state)
                            {
                                async_send_(invokeMs, ntf, msg, ntfSign);
                            }
                            else
                            {
                                ntf(new CspInvokeWrap<R> { state = state });
                            }
                        }
                    }));
                }
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                _tempMsg.Set(ntf, msg, timer, invokeMs);
                timer.Timeout(ms, delegate ()
                {
                    _tempMsg.Cancel();
                    ntf(new CspInvokeWrap<R> { state = ChannelState.overtime });
                });
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                _tempMsg.Set(ntf, msg, invokeMs);
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        private void async_timed_recv_(int ms, Action<CspWaitWrap<R, T>> ntf, ChannelNotifySign ntfSign)
        {
            ntfSign?.ResetSuccess();
            if (_tempMsg._has)
            {
                SendPick msg = _tempMsg;
                _tempMsg.Cancel();
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                ntf(new CspWaitWrap<R, T> { state = ChannelState.ok, msg = msg._msg, result = new CspResult(msg._invokeMs, msg._notify) });
            }
            else if (_closed)
            {
                ntf(new CspWaitWrap<R, T> { state = ChannelState.closed });
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                PriorityQueueNode<NotifyPick> node = _recvQueue.AddLast(0, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        timer.Cancel();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new CspWaitWrap<R, T> { state = state });
                        }
                    }
                });
                ntfSign?.SetNode(node);
                timer.Timeout(ms, delegate ()
                {
                    _recvQueue.Remove(node).Invoke(ChannelState.overtime);
                });
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(ntf, ntfSign);
                        }
                        else
                        {
                            ntf(new CspWaitWrap<R, T> { state = state });
                        }
                    }
                }));
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        protected override void async_append_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms)
        {
            if (_tempMsg._has)
            {
                ntfSign._success = true;
                ntf(ChannelState.ok);
            }
            else if (_closed)
            {
                ntf(ChannelState.closed);
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                ntfSign.SetNode(_recvQueue.AddLast(1, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        timer.Cancel();
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                timer.Timeout(ms, delegate ()
                {
                    _recvQueue.Remove(ntfSign._ntfNode).Invoke(ChannelState.overtime);
                });
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                ntfSign.SetNode(_recvQueue.AddLast(1, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
        }

        private void async_try_recv_and_append_notify_(Action<CspWaitWrap<R, T>> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, int ms)
        {
            ntfSign.ResetSuccess();
            if (_tempMsg._has)
            {
                SendPick msg = _tempMsg;
                _tempMsg.Cancel();
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
                if (!ntfSign._selectOnce)
                {
                    async_append_recv_notify_(msgNtf, ntfSign, ms);
                }
                cb(new CspWaitWrap<R, T> { state = ChannelState.ok, msg = msg._msg, result = new CspResult(msg._invokeMs, msg._notify) });
            }
            else if (_closed)
            {
                msgNtf(ChannelState.closed);
                cb(new CspWaitWrap<R, T> { state = ChannelState.closed });
            }
            else if (!_sendQueue.Empty && _recvQueue.Empty)
            {
                _isTryRecv = true;
                ChannelNotifySign.SetNode(ntfSign, _recvQueue.AddLast(0, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        _isTryRecv = false;
                        ntfSign?.ResetNode();
                        if (ChannelState.ok == state)
                        {
                            async_recv_(cb, ntfSign);
                        }
                        else
                        {
                            cb(new CspWaitWrap<R, T> { state = state });
                        }
                    }
                }));
                _sendQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                async_append_recv_notify_(msgNtf, ntfSign, ms);
                cb(new CspWaitWrap<R, T> { state = ChannelState.Fail });
            }
        }

        protected override void async_remove_recv_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            bool Effect = ntfSign._ntfNode.Effect;
            bool success = ntfSign.ResetSuccess();
            if (Effect)
            {
                _isTryRecv &= _recvQueue.First._node != ntfSign._ntfNode._node;
                _recvQueue.Remove(ntfSign._ntfNode).CancelTimer();
                ntfSign.ResetNode();
            }
            else if (success && _tempMsg._has)
            {
                if (!_recvQueue.RemoveFirst().Invoke(ChannelState.ok) && _tempMsg._isTryMsg)
                {
                    _tempMsg.Cancel().Invoke(new CspInvokeWrap<R> { state = ChannelState.Fail });
                }
            }
            ntf(Effect ? ChannelState.ok : ChannelState.Fail);
        }

        protected override void async_append_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign, int ms)
        {
            if (_closed)
            {
                ntf(ChannelState.closed);
                return;
            }
            if (!_recvQueue.Empty)
            {
                ntfSign._success = true;
                ntf(ChannelState.ok);
            }
            else if (ms >= 0)
            {
                AsyncTimer timer = new AsyncTimer(SelfStrand());
                ntfSign.SetNode(_sendQueue.AddLast(1, new NotifyPick()
                {
                    timer = timer,
                    ntf = delegate (ChannelState state)
                    {
                        timer.Cancel();
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
                timer.Timeout(ms, delegate ()
                {
                    _sendQueue.Remove(ntfSign._ntfNode).Invoke(ChannelState.overtime);
                });
            }
            else
            {
                ntfSign.SetNode(_sendQueue.AddLast(1, new NotifyPick()
                {
                    ntf = delegate (ChannelState state)
                    {
                        ntfSign.ResetNode();
                        ntfSign._success = ChannelState.ok == state;
                        ntf(state);
                    }
                }));
            }
        }

        private void async_try_send_and_append_notify_(Action<CspInvokeWrap<R>> cb, Action<ChannelState> msgNtf, ChannelNotifySign ntfSign, T msg, int ms)
        {
            ntfSign.ResetSuccess();
            if (_closed)
            {
                msgNtf(ChannelState.closed);
                cb(new CspInvokeWrap<R> { state = ChannelState.closed });
                return;
            }
            if (!_tempMsg._has && !_recvQueue.Empty)
            {
                _tempMsg.Set(cb, msg);
                _tempMsg._isTryMsg = true;
                if (!ntfSign._selectOnce)
                {
                    async_append_send_notify_(msgNtf, ntfSign, ms);
                }
                _recvQueue.RemoveFirst().Invoke(ChannelState.ok);
            }
            else
            {
                async_append_send_notify_(msgNtf, ntfSign, ms);
                cb(new CspInvokeWrap<R> { state = ChannelState.Fail });
            }
        }

        protected override void async_remove_send_notify_(Action<ChannelState> ntf, ChannelNotifySign ntfSign)
        {
            bool Effect = ntfSign._ntfNode.Effect;
            bool success = ntfSign.ResetSuccess();
            if (Effect)
            {
                _sendQueue.Remove(ntfSign._ntfNode).CancelTimer();
                ntfSign.ResetNode();
            }
            else if (success && !_tempMsg._has)
            {
                if (!_sendQueue.RemoveFirst().Invoke(ChannelState.ok) && _isTryRecv)
                {
                    _recvQueue.RemoveFirst().Invoke(ChannelState.Fail);
                }
            }
            ntf(Effect ? ChannelState.ok : ChannelState.Fail);
        }

        protected override void async_clear_(Action ntf)
        {
            _tempMsg.Cancel();
            SafeCallback(ref _sendQueue, ChannelState.Fail);
            ntf();
        }

        protected override void async_close_(Action ntf, bool isClear = false)
        {
            _closed = true;
            Action<CspInvokeWrap<R>> hasMsg = null;
            if (_tempMsg._has)
            {
                hasMsg = _tempMsg.Cancel();
            }
            SafeCallback(ref _sendQueue, ref _recvQueue, ChannelState.closed);
            hasMsg?.Invoke(new CspInvokeWrap<R> { state = ChannelState.closed });
            ntf();
        }

        protected override void async_cancel_(Action ntf, bool isClear = false)
        {
            Action<CspInvokeWrap<R>> hasMsg = null;
            if (_tempMsg._has)
            {
                hasMsg = _tempMsg.Cancel();
            }
            SafeCallback(ref _sendQueue, ref _recvQueue, ChannelState.Cancel);
            hasMsg?.Invoke(new CspInvokeWrap<R> { state = ChannelState.Cancel });
            ntf();
        }
    }
}
