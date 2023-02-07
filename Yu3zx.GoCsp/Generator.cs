using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Yu3zx.GoCsp
{
    struct MultiCheck
    {
        public bool callbacked;
        public bool beginQuit;

        public bool Check()
        {
            bool t = callbacked;
            callbacked = true;
            return t;
        }
    }

    public class AsyncResultWrap<T1>
    {
        public T1 value1;

        public void Clear()
        {
            value1 = default(T1);
        }
    }

    public class AsyncResultWrap<T1, T2>
    {
        public T1 value1;
        public T2 value2;

        public void Clear()
        {
            value1 = default(T1);
            value2 = default(T2);
        }
    }

    public class AsyncResultWrap<T1, T2, T3>
    {
        public T1 value1;
        public T2 value2;
        public T3 value3;

        public void Clear()
        {
            value1 = default(T1);
            value2 = default(T2);
            value3 = default(T3);
        }
    }

    public static class AsyncResultIgnoreWrap<T1>
    {
        static readonly public AsyncResultWrap<T1> value = new AsyncResultWrap<T1>();
    }

    public static class AsyncResultIgnoreWrap<T1, T2>
    {
        static readonly public AsyncResultWrap<T1, T2> value = new AsyncResultWrap<T1, T2>();
    }

    public static class AsyncResultIgnoreWrap<T1, T2, T3>
    {
        static readonly public AsyncResultWrap<T1, T2, T3> value = new AsyncResultWrap<T1, T2, T3>();
    }

    public class ChannelLostMsg<T>
    {
        T _msg;
        bool _has = false;

        virtual internal void set(T m)
        {
            _has = true;
            _msg = m;
        }

        public void Clear()
        {
            _has = false;
            _msg = default(T);
        }

        public bool has
        {
            get
            {
                return _has;
            }
        }

        public T msg
        {
            get
            {
                return _msg;
            }
        }
    }

    public class ChannelLostMsg<T1, T2> : ChannelLostMsg<TupleEx<T1, T2>> { }
    public class ChannelLostMsg<T1, T2, T3> : ChannelLostMsg<TupleEx<T1, T2, T3>> { }

    public class ChannelLostDocker<T> : ChannelLostMsg<T>
    {
        readonly Channel<T> _docker;
        readonly bool _tryDocker;

        public ChannelLostDocker(Channel<T> docker, bool tryDocker = true)
        {
            _docker = docker;
            _tryDocker = tryDocker;
        }

        override internal void set(T m)
        {
            base.set(m);
            if (_tryDocker)
            {
                _docker.TryPost(m);
            }
            else
            {
                _docker.Post(m);
            }
        }

        public Channel<T> docker
        {
            get
            {
                return _docker;
            }
        }
    }

    public class ChannelLostDocker<T1, T2> : ChannelLostDocker<TupleEx<T1, T2>>
    {
        public ChannelLostDocker(Channel<TupleEx<T1, T2>> docker, bool tryDocker = true) : base(docker, tryDocker) { }
    }

    public class ChannelLostDocker<T1, T2, T3> : ChannelLostDocker<TupleEx<T1, T2, T3>>
    {
        public ChannelLostDocker(Channel<TupleEx<T1, T2, T3>> docker, bool tryDocker = true) : base(docker, tryDocker) { }
    }

    public class ChannelException : System.Exception
    {
        public readonly ChannelState state;

        internal ChannelException(ChannelState st)
        {
            state = st;
        }
    }

    public class CspFailException : System.Exception
    {
        internal CspFailException() { }
    }

    public struct ChannelReceiveWrap<T>
    {
        public ChannelState state;
        public T msg;

        public static implicit operator T(ChannelReceiveWrap<T> rval)
        {
            if (ChannelState.ok != rval.state)
            {
                throw new ChannelException(rval.state);
            }
            return rval.msg;
        }

        public static implicit operator ChannelState(ChannelReceiveWrap<T> rval)
        {
            return rval.state;
        }

        public static ChannelReceiveWrap<T> def
        {
            get
            {
                return new ChannelReceiveWrap<T> { state = ChannelState.undefined };
            }
        }

        public void Check()
        {
            if (ChannelState.ok != state)
            {
                throw new ChannelException(state);
            }
        }

        public override string ToString()
        {
            return ChannelState.ok == state ?
                string.Format("ChannelReceiveWrap<{0}>.msg={1}", typeof(T).Name, msg) :
                string.Format("ChannelReceiveWrap<{0}>.state={1}", typeof(T).Name, state);
        }
    }

    public struct ChannelSendWrap
    {
        public ChannelState state;

        public static implicit operator ChannelState(ChannelSendWrap rval)
        {
            return rval.state;
        }

        public static ChannelSendWrap def
        {
            get
            {
                return new ChannelSendWrap { state = ChannelState.undefined };
            }
        }

        public void Check()
        {
            if (ChannelState.ok != state)
            {
                throw new ChannelException(state);
            }
        }

        public override string ToString()
        {
            return state.ToString();
        }
    }

    public struct CspInvokeWrap<T>
    {
        public ChannelState state;
        public T result;

        public static implicit operator T(CspInvokeWrap<T> rval)
        {
            if (ChannelState.ok != rval.state)
            {
                throw new ChannelException(rval.state);
            }
            return rval.result;
        }

        public static implicit operator ChannelState(CspInvokeWrap<T> rval)
        {
            return rval.state;
        }

        public static CspInvokeWrap<T> def
        {
            get
            {
                return new CspInvokeWrap<T> { state = ChannelState.undefined };
            }
        }

        public void Check()
        {
            if (ChannelState.ok != state)
            {
                throw new ChannelException(state);
            }
        }

        public override string ToString()
        {
            return ChannelState.ok == state ?
                string.Format("CspInvokeWrap<{0}>.result={1}", typeof(T).Name, result) :
                string.Format("CspInvokeWrap<{0}>.state={1}", typeof(T).Name, state);
        }
    }

    public struct CspWaitWrap<R, T>
    {
        public CspChannel<R, T>.CspResult result;
        public ChannelState state;
        public T msg;

        public static implicit operator T(CspWaitWrap<R, T> rval)
        {
            if (ChannelState.ok != rval.state)
            {
                throw new ChannelException(rval.state);
            }
            return rval.msg;
        }

        public static implicit operator ChannelState(CspWaitWrap<R, T> rval)
        {
            return rval.state;
        }

        public static CspWaitWrap<R, T> def
        {
            get
            {
                return new CspWaitWrap<R, T> { state = ChannelState.undefined };
            }
        }

        public void Check()
        {
            if (ChannelState.ok != state)
            {
                throw new ChannelException(state);
            }
        }

        public bool Complete(R res)
        {
            return result.Complete(res);
        }

        public void Fail()
        {
            result.Fail();
        }

        public bool empty()
        {
            return null == result;
        }

        public override string ToString()
        {
            return ChannelState.ok == state ?
                string.Format("CspWaitWrap<{0},{1}>.msg={2}", typeof(R).Name, typeof(T).Name, msg) :
                string.Format("CspWaitWrap<{0},{1}>.state={2}", typeof(R).Name, typeof(T).Name, state);
        }
    }

#if !NETCORE
    public struct ValueTask<T> : ICriticalNotifyCompletion
    {
        T value;
        Task<T> task;

        public ValueTask(T result)
        {
            value = result;
            task = null;
        }

        public ValueTask(Task<T> result)
        {
            value = default(T);
            task = result;
        }

        public ValueTask<T> GetAwaiter()
        {
            return this;
        }

        public T GetResult()
        {
            return null == task ? value : task.GetAwaiter().GetResult();
        }

        public void OnCompleted(Action continuation)
        {
            task.GetAwaiter().OnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            task.GetAwaiter().UnsafeOnCompleted(continuation);
        }

        public Task<T> AsTask()
        {
            return task;
        }

        public T Result
        {
            get
            {
                return null == task ? value : task.Result;
            }
        }

        public bool IsCanceled
        {
            get
            {
                return null == task ? false : task.IsCanceled;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return null == task ? true : task.IsCompleted;
            }
        }

        public bool IsFaulted
        {
            get
            {
                return null == task ? false : task.IsFaulted;
            }
        }
    }
#endif

    public class DelayCsp<R>
    {
        CspInvokeWrap<R> _result;
        bool _taken;
        bool _completed;
        Action<CspInvokeWrap<R>> _lostRes;
        Action _continuation;

        internal DelayCsp(Action<CspInvokeWrap<R>> lostRes)
        {
            _taken = false;
            _completed = false;
            _lostRes = lostRes;
        }

        ~DelayCsp()
        {
            if (_completed && !_taken && null != _lostRes)
            {
                Task.Run(() => _lostRes(_result));
            }
        }

        internal CspInvokeWrap<R> result
        {
            set
            {
                _result = value;
                Action continuation;
                lock (this)
                {
                    _completed = true;
                    continuation = _continuation;
                    _continuation = null;
                }
                Functional.CatchInvoke(continuation);
            }
            get
            {
                _taken = true;
                return _result;
            }
        }

        internal void on_completed(Action continuation)
        {
            bool completed;
            Action oldContinuation = null;
            lock (this)
            {
                if (completed = _completed)
                {
                    if (null != _continuation)
                    {
                        oldContinuation = _continuation;
                        _continuation = null;
                    }
                }
                else
                {
                    if (null == _continuation)
                    {
                        _continuation = continuation;
                    }
                    else
                    {
                        oldContinuation = _continuation;
                        _continuation = delegate ()
                        {
                            Functional.CatchInvoke(oldContinuation);
                            Functional.CatchInvoke(continuation);
                        };
                    }
                }
            }
            if (completed)
            {
                Functional.CatchInvoke(oldContinuation);
                Functional.CatchInvoke(continuation);
            }
        }

        internal bool is_completed
        {
            get
            {
                return _completed;
            }
        }
    }

    public class Generator
    {
        public class LocalValueException : System.Exception
        {
            internal LocalValueException() { }
        }

        public abstract class InnerException : System.Exception { }

        public class StopException : InnerException
        {
            internal StopException() { }
        }

        public class SelectStopCurrentException : InnerException
        {
            internal SelectStopCurrentException() { }
        }

        public class SelectStopAllException : InnerException
        {
            internal SelectStopAllException() { }
        }

        public class MessageStopCurrentException : InnerException
        {
            internal MessageStopCurrentException() { }
        }

        public class MessageStopAllException : InnerException
        {
            internal MessageStopAllException() { }
        }

        class TypeHash<T>
        {
            public static readonly int code = Interlocked.Increment(ref _hashCount);
        }

        class MailPick
        {
            public ChannelBase mailbox;
            public child agentAction;

            public MailPick(ChannelBase mb)
            {
                mailbox = mb;
            }
        }

        abstract class LocalWrap { }
        class LocalWrap<T> : LocalWrap
        {
            internal T value;
        }

        public struct NotifyToken
        {
            internal Generator host;
            internal LinkedListNode<Action<Generator>> token;
        }

        public struct SuspendToken
        {
            internal LinkedListNode<Action<bool>> token;
        }

        public struct StopCount
        {
            internal int _count;
        }

        public struct SuspendCount
        {
            internal int _count;
        }

        public struct ShieldCount
        {
            internal int _lockCount;
            internal int _suspendCount;
        }

        public class Local<T>
        {
            readonly long _id = Interlocked.Increment(ref Generator._idCount);

            public T value
            {
                get
                {
                    Generator host = self;
                    if (null == host || null == host._genLocal)
                    {
                        throw new LocalValueException();
                    }
                    LocalWrap localWrap;
                    if (!host._genLocal.TryGetValue(_id, out localWrap))
                    {
                        throw new LocalValueException();
                    }
                    return ((LocalWrap<T>)localWrap).value;
                }
                set
                {
                    Generator host = self;
                    if (null == host)
                    {
                        throw new LocalValueException();
                    }
                    if (null == host._genLocal)
                    {
                        host._genLocal = new Dictionary<long, LocalWrap>();
                    }
                    LocalWrap localWrap;
                    if (!host._genLocal.TryGetValue(_id, out localWrap))
                    {
                        host._genLocal.Add(_id, new LocalWrap<T> { value = value });
                    }
                    else
                    {
                        ((LocalWrap<T>)localWrap).value = value;
                    }
                }
            }

            public void Remove()
            {
                Generator host = self;
                if (null == host)
                {
                    throw new LocalValueException();
                }
                host._genLocal?.Remove(_id);
            }
        }

        class pull_task : ICriticalNotifyCompletion
        {
            bool _completed = false;
            bool _activated = false;
            Action _continuation;

            public pull_task GetAwaiter()
            {
                return this;
            }

            public void GetResult()
            {
            }

            public void OnCompleted(Action continuation)
            {
                _continuation = continuation;
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                _continuation = continuation;
            }

            public bool IsCompleted
            {
                get
                {
                    return _completed;
                }
            }

            public bool IsAwaiting()
            {
                return null != _continuation;
            }

            public bool Activated
            {
                get
                {
                    return _activated;
                }
                set
                {
                    _activated = value;
                }
            }

            public void NewTask()
            {
                Debug.Assert(_completed && _activated, "不对称的推入操作!");
                _completed = false;
                _activated = false;
            }

            public void AheadComplete()
            {
                Debug.Assert(!_completed, "不对称的拉取操作!");
                _completed = true;
            }

            public void Complete()
            {
                Debug.Assert(!_completed, "不对称的拉取操作!");
                _completed = true;
                Action continuation = _continuation;
                _continuation = null;
                continuation();
            }
        }

        public class LockStopGuard : IDisposable
        {
            static internal LockStopGuard guard = new LockStopGuard();

            private LockStopGuard() { }

            public void Dispose()
            {
                unlock_stop();
            }
        }

#if CHECK_STEP_TIMEOUT
        public class CallStackInfo
        {
            public readonly string time;
            public readonly string file;
            public readonly int line;

            public CallStackInfo(string t, string f, int l)
            {
                time = t;
                file = f;
                line = l;
            }

            public override string ToString()
            {
                return string.Format("<file>{0} <line>{1} <time>{2}", file, line, time);
            }
        }
        LinkedList<CallStackInfo[]> _makeStack;
        long _beginStepTick;
        static readonly int _stepMaxCycle = 100;
#endif

        static int _hashCount = 0;
        static long _idCount = 0;
        static Task _nilTask = Functional.Init(() => { Task task = new Task(NilAction.action); task.RunSynchronously(); return task; });
        static ReaderWriterLockSlim _nameMutex = new ReaderWriterLockSlim();
        static Dictionary<string, Generator> _nameGens = new Dictionary<string, Generator>();
        static Action<System.Exception> _hookException = null;

        Dictionary<long, MailPick> _mailboxMap;
        Dictionary<long, LocalWrap> _genLocal;
        LinkedList<LinkedList<SelectChannelBase>> _topSelectChans;
        LinkedList<Action<bool>> _suspendHandler;
        LinkedList<children> _children;
        LinkedList<Action<Generator>> _callbacks;
        ChannelNotifySign _ioSign;
        System.Exception _excep;
        pull_task _pullTask;
        children _agentMng;
        AsyncTimer _timer;
        string _name;
        long _lastTm;
        long _yieldCount;
        long _lastYieldCount;
        long _id;
        int _lockCount;
        int _lockSuspendCount;
        bool _disableTopStop;
        bool _beginQuit;
        bool _isSuspend;
        bool _holdSuspend;
        bool _hasBlock;
        bool _overtime;
        bool _isForce;
        bool _isStop;
        bool _isRun;
        bool _mustTick;

        public delegate Task action();

        Generator() { }

        static public Generator Make(SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
        {
            return (new Generator()).Init(Strand, generatorAction, completedHandler, suspendHandler);
        }

        static public void Go(SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
        {
            Make(Strand, generatorAction, completedHandler, suspendHandler).Run();
        }

        static public void Go(out Generator newGen, SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
        {
            newGen = Make(Strand, generatorAction, completedHandler, suspendHandler);
            newGen.Run();
        }

        static public Generator Tgo(SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
        {
            Generator newGen = Make(Strand, generatorAction, completedHandler, suspendHandler);
            newGen.Trun();
            return newGen;
        }

        static public Generator Make(string name, SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
        {
            Generator newGen = Make(Strand, generatorAction, completedHandler, suspendHandler);
            newGen._name = name;
            try
            {
                _nameMutex.EnterWriteLock();
                _nameGens.Add(name, newGen);
                _nameMutex.ExitWriteLock();
            }
            catch (System.ArgumentException)
            {
                _nameMutex.ExitWriteLock();
#if DEBUG
                Trace.Fail(string.Format("Generator {0}重名", name));
#endif
            }
            return newGen;
        }

        static public void Go(string name, SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
        {
            Make(name, Strand, generatorAction, completedHandler, suspendHandler).Run();
        }

        static public void Go(out Generator newGen, string name, SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
        {
            newGen = Make(name, Strand, generatorAction, completedHandler, suspendHandler);
            newGen.Run();
        }

        static public Generator Tgo(string name, SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
        {
            Generator newGen = Make(name, Strand, generatorAction, completedHandler, suspendHandler);
            newGen.Trun();
            return newGen;
        }

        static public Generator Find(string name)
        {
            Generator gen = null;
            _nameMutex.EnterReadLock();
            _nameGens.TryGetValue(name, out gen);
            _nameMutex.ExitReadLock();
            return gen;
        }

        public string Name()
        {
            return _name;
        }

#if CHECK_STEP_TIMEOUT
        static void UpStackFrame(LinkedList<CallStackInfo[]> callStack, int offset = 0, int Count = 1)
        {
            offset += 2;
            StackFrame[] sts = (new StackTrace(true)).GetFrames();
            string time = DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff");
            CallStackInfo[] snap = new CallStackInfo[Count];
            callStack.AddFirst(snap);
            for (int i = 0; i < Count; ++i, ++offset)
            {
                if (offset < sts.Length)
                {
                    snap[i] = new CallStackInfo(time, sts[offset].GetFileName(), sts[offset].GetFileLineNumber());
                }
                else
                {
                    snap[i] = new CallStackInfo(time, "null", -1);
                }
            }
        }
#endif

#if CHECK_STEP_TIMEOUT
        Generator Init(SharedStrand Strand, action generatorAction, Action completedHandler, Action<bool> suspendHandler, LinkedList<CallStackInfo[]> makeStack = null)
        {
            if (null != makeStack)
            {
                _makeStack = makeStack;
            }
            else
            {
                _makeStack = new LinkedList<CallStackInfo[]>();
                UpStackFrame(_makeStack, 1, 6);
            }
            _beginStepTick = SystemTick.GetTickMs();
#else
        Generator Init(SharedStrand Strand, action generatorAction, Action completedHandler, Action<bool> suspendHandler)
        {
#endif
            _id = Interlocked.Increment(ref _idCount);
            _mustTick = true;
            _isForce = false;
            _isStop = false;
            _isRun = false;
            _overtime = false;
            _isSuspend = false;
            _holdSuspend = false;
            _hasBlock = false;
            _beginQuit = false;
            _disableTopStop = false;
            _lockCount = 0;
            _lockSuspendCount = 0;
            _lastTm = 0;
            _yieldCount = 0;
            _lastYieldCount = 0;
            _pullTask = new pull_task();
            _ioSign = new ChannelNotifySign();
            _timer = new AsyncTimer(Strand);
            if (null != suspendHandler)
            {
                _suspendHandler = new LinkedList<Action<bool>>();
                _suspendHandler.AddLast(suspendHandler);
            }
            Strand.HoldWork();
            Strand.Dispatched(async delegate ()
            {
                try
                {
                    try
                    {
                        _mustTick = false;
                        await async_wait();
                        await generatorAction();
                    }
                    finally
                    {
                        _lockSuspendCount = -1;
                        if (null != _mailboxMap)
                        {
                            foreach (KeyValuePair<long, MailPick> ele in _mailboxMap)
                            {
                                ele.Value.mailbox.Close();
                            }
                        }
                    }
                    if (!_isForce && null != _children)
                    {
                        while (0 != _children.Count)
                        {
                            await _children.First.Value.wait_all();
                        }
                    }
                }
                catch (StopException) { }
                catch (System.Exception ec)
                {
                    if (null == _hookException)
                    {
                        string source = ec.Source;
                        string message = ec.Message;
                        string stacktrace = ec.StackTrace;
                        System.Exception inner = ec;
                        while (null != (inner = inner.InnerException))
                        {
                            source = string.Format("{0}\n{1}", inner.Source, source);
                            message = string.Format("{0}\n{1}", inner.Message, message);
                            stacktrace = string.Format("{0}\n{1}", inner.StackTrace, stacktrace);
                        }
#if NETCORE
                        Debug.WriteLine(string.Format("{0}\n{1}\n{2}\n{3}", "Generator 内部未捕获的异常!", message, source, stacktrace));
#else
                        Task.Run(() => System.Windows.Forms.MessageBox.Show(string.Format("{0}\n{1}\n{2}", message, source, stacktrace), "Generator 内部未捕获的异常!", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error)).Wait();
#endif
                    }
                    else
                    {
                        Functional.CatchInvoke(_hookException, ec);
                    }
                    _excep = ec;
                    _lockCount++;
                }
                finally
                {
                    if (_isForce || null != _excep)
                    {
                        _timer.Cancel();
                        if (null != _children && 0 != _children.Count)
                        {
                            await children.Stop(_children);
                        }
                    }
                }
                if (null != _name)
                {
                    _nameMutex.EnterWriteLock();
                    _nameGens.Remove(_name);
                    _nameMutex.ExitWriteLock();
                }
                _isStop = true;
                _suspendHandler = null;
                Strand.currSelf = null;
                Functional.CatchInvoke(completedHandler);
                if (null != _callbacks)
                {
                    while (0 != _callbacks.Count)
                    {
                        Action<Generator> ntf = _callbacks.First.Value;
                        _callbacks.RemoveFirst();
                        Functional.CatchInvoke(ntf, this);
                    }
                }
                Strand.ReleaseWork();
            });
            return this;
        }

        void NoCheckNext()
        {
            if (_isSuspend)
            {
                _hasBlock = true;
            }
            else if (!_pullTask.IsAwaiting())
            {
                _pullTask.AheadComplete();
            }
            else
            {
                _yieldCount++;
                Generator oldGen = Strand.currSelf;
                if (null == oldGen || !oldGen._pullTask.Activated)
                {
                    Strand.currSelf = this;
                    _pullTask.Complete();
                    Strand.currSelf = oldGen;
                }
                else
                {
                    oldGen._pullTask.Activated = false;
                    Strand.currSelf = this;
                    _pullTask.Complete();
                    Strand.currSelf = oldGen;
                    oldGen._pullTask.Activated = true;
                }
            }
        }

        void Next(bool beginQuit)
        {
            if (!_isStop && _beginQuit == beginQuit)
            {
                NoCheckNext();
            }
        }

        void QuitNext()
        {
            Next(true);
        }

        void NoQuitNext()
        {
            Next(false);
        }

        MultiCheck NewMultiCheck()
        {
            _pullTask.NewTask();
            return new MultiCheck { callbacked = false, beginQuit = _beginQuit };
        }

        public void Run()
        {
            if (Strand.RunningInThisThread() && !_mustTick)
            {
                if (!_isRun && !_isStop)
                {
                    _isRun = true;
                    NoCheckNext();
                }
            }
            else
            {
                Trun();
            }
        }

        public void Trun()
        {
            Strand.Post(delegate ()
            {
                if (!_isRun && !_isStop)
                {
                    _isRun = true;
                    NoCheckNext();
                }
            });
        }

        private void _suspend_cb(bool isSuspend, Action cb = null, bool canSuspendCb = true)
        {
            if (null != _children && 0 != _children.Count)
            {
                int Count = _children.Count;
                Action handler = delegate ()
                {
                    if (0 == --Count)
                    {
                        if (canSuspendCb && null != _suspendHandler)
                        {
                            for (LinkedListNode<Action<bool>> it = _suspendHandler.First; null != it; it = it.Next)
                            {
                                Functional.CatchInvoke(it.Value, isSuspend);
                            }
                        }
                        Functional.CatchInvoke(cb);
                    }
                };
                _mustTick = true;
                for (LinkedListNode<children> it = _children.First; null != it; it = it.Next)
                {
                    it.Value.Suspend(isSuspend, handler);
                }
                _mustTick = false;
            }
            else
            {
                if (canSuspendCb && null != _suspendHandler)
                {
                    _mustTick = true;
                    for (LinkedListNode<Action<bool>> it = _suspendHandler.First; null != it; it = it.Next)
                    {
                        Functional.CatchInvoke(it.Value, isSuspend);
                    }
                    _mustTick = false;
                }
                Functional.CatchInvoke(cb);
            }
        }

        private void _suspend(Action cb = null)
        {
            if (!_isStop && !_beginQuit && !_isSuspend)
            {
                if (0 == _lockSuspendCount)
                {
                    _isSuspend = true;
                    if (0 != _lastTm)
                    {
                        _lastTm -= SystemTick.GetTickUs() - _timer.Cancel();
                        if (_lastTm <= 0)
                        {
                            _lastTm = 0;
                            _hasBlock = true;
                        }
                    }
                    _suspend_cb(true, cb);
                }
                else
                {
                    _holdSuspend = true;
                    _suspend_cb(true, cb, false);
                }
            }
            else
            {
                Functional.CatchInvoke(cb);
            }
        }

        public void Tsuspend(Action cb = null)
        {
            Strand.Post(() => _suspend(cb));
        }

        public void Suspend(Action cb = null)
        {
            if (Strand.RunningInThisThread() && !_mustTick)
            {
                _suspend(cb);
            }
            else
            {
                Tsuspend(cb);
            }
        }

        private void _resume(Action cb = null)
        {
            if (!_isStop && !_beginQuit)
            {
                if (_isSuspend)
                {
                    _isSuspend = false;
                    _suspend_cb(false, cb);
                    if (_hasBlock)
                    {
                        _hasBlock = false;
                        NoQuitNext();
                    }
                    else if (0 != _lastTm)
                    {
                        _timer.TimeoutUs(_lastTm, NoCheckNext);
                    }
                }
                else
                {
                    _holdSuspend = false;
                    _suspend_cb(false, cb, false);
                }
            }
            else
            {
                Functional.CatchInvoke(cb);
            }
        }

        public void Tresume(Action cb = null)
        {
            Strand.Post(() => _resume(cb));
        }

        public void Resume(Action cb = null)
        {
            if (Strand.RunningInThisThread() && !_mustTick)
            {
                _resume(cb);
            }
            else
            {
                Tresume(cb);
            }
        }

        private void _stop()
        {
            _isForce = true;
            if (0 == _lockCount)
            {
                if (!_disableTopStop && _pullTask.Activated)
                {
                    _suspendHandler = null;
                    _lockSuspendCount = 0;
                    _holdSuspend = false;
                    _isSuspend = false;
                    _beginQuit = true;
                    _timer.Cancel();
                    throw new StopException();
                }
                else if (_pullTask.IsAwaiting())
                {
                    _isSuspend = false;
                    NoQuitNext();
                }
                else
                {
                    Tstop();
                }
            }
        }

        static public void DisableTopStop(bool disable = true)
        {
            Generator this_ = self;
            this_._disableTopStop = disable;
        }

        public void Tstop()
        {
            Strand.Post(delegate ()
            {
                if (!_isStop)
                {
                    _stop();
                }
            });
        }

        public void Stop()
        {
            if (Strand.RunningInThisThread() && !_mustTick)
            {
                if (!_isStop)
                {
                    _stop();
                }
            }
            else
            {
                Tstop();
            }
        }

        public void Tstop(Action<Generator> continuation)
        {
            Strand.Post(delegate ()
            {
                if (!_isStop)
                {
                    if (null == _callbacks)
                    {
                        _callbacks = new LinkedList<Action<Generator>>();
                    }
                    _callbacks.AddLast(continuation);
                    _stop();
                }
                else
                {
                    Functional.CatchInvoke(continuation, this);
                }
            });
        }

        public void Tstop(Action continuation)
        {
            Tstop((Generator _) => Functional.CatchInvoke(continuation));
        }

        public void Stop(Action<Generator> continuation)
        {
            if (Strand.RunningInThisThread() && !_mustTick)
            {
                if (!_isStop)
                {
                    if (null == _callbacks)
                    {
                        _callbacks = new LinkedList<Action<Generator>>();
                    }
                    _callbacks.AddLast(continuation);
                    _stop();
                }
                else
                {
                    Functional.CatchInvoke(continuation, this);
                }
            }
            else
            {
                Tstop(continuation);
            }
        }

        public void Stop(Action continuation)
        {
            Stop((Generator _) => Functional.CatchInvoke(continuation));
        }

        public void append_stop_callback(Action<Generator> continuation, Action<NotifyToken> removeCb = null)
        {
            if (Strand.RunningInThisThread())
            {
                if (!_isStop)
                {
                    if (null == _callbacks)
                    {
                        _callbacks = new LinkedList<Action<Generator>>();
                    }
                    Functional.CatchInvoke(removeCb, new NotifyToken { host = this, token = _callbacks.AddLast(continuation) });
                }
                else
                {
                    Functional.CatchInvoke(continuation, this);
                    Functional.CatchInvoke(removeCb, new NotifyToken { host = this });
                }
            }
            else
            {
                Strand.Post((Action)delegate ()
                {
                    if (!_isStop)
                    {
                        if (null == _callbacks)
                        {
                            _callbacks = new LinkedList<Action<Generator>>();
                        }
                        Functional.CatchInvoke(removeCb, new NotifyToken { host = this, token = _callbacks.AddLast(continuation) });
                    }
                    else
                    {
                        Functional.CatchInvoke(continuation, this);
                        Functional.CatchInvoke(removeCb, new NotifyToken { host = this });
                    }
                });
            }
        }

        public void append_stop_callback(Action continuation, Action<NotifyToken> removeCb = null)
        {
            append_stop_callback((Generator _) => Functional.CatchInvoke(continuation), removeCb);
        }

        public void remove_stop_callback(NotifyToken cancelToken, Action cb = null)
        {
            Debug.Assert(this == cancelToken.host, "异常的 remove_stop_callback 调用!");
            if (Strand.RunningInThisThread())
            {
                if (null != cancelToken.token && null != cancelToken.token.List)
                {
                    _callbacks.Remove(cancelToken.token);
                }
                Functional.CatchInvoke(cb);
            }
            else
            {
                Strand.Post(delegate ()
                {
                    if (null != cancelToken.token && null != cancelToken.token.List)
                    {
                        _callbacks.Remove(cancelToken.token);
                    }
                    Functional.CatchInvoke(cb);
                });
            }
        }

        static public SuspendToken append_suspend_handler(Action<bool> handler)
        {
            Generator this_ = self;
            if (null == this_._suspendHandler)
            {
                this_._suspendHandler = new LinkedList<Action<bool>>();
            }
            return new SuspendToken { token = this_._suspendHandler.AddLast(handler) };
        }

        static public bool remove_suspend_handler(SuspendToken token)
        {
            Generator this_ = self;
            if (null != this_._suspendHandler && null != token.token && null != token.token.List)
            {
                this_._suspendHandler.Remove(token.token);
                return true;
            }
            return false;
        }

        public bool is_force()
        {
            Debug.Assert(_isStop, "不正确的 is_force 调用，Generator 还没有结束!");
            return _isForce;
        }

        public bool has_excep()
        {
            Debug.Assert(_isStop, "不正确的 has_excep 调用，Generator 还没有结束!");
            return null != _excep;
        }

        public System.Exception excep
        {
            get
            {
                Debug.Assert(_isStop, "不正确的 excep 调用，Generator 还没有结束!");
                return _excep;
            }
        }

        public void check_excep()
        {
            Debug.Assert(_isStop, "不正确的 check_excep 调用，Generator 还没有结束!");
            if (null != _excep)
            {
                throw excep;
            }
        }

        public bool is_completed()
        {
            return _isStop;
        }

        static public bool begin_quit()
        {
            Generator this_ = self;
            return this_._beginQuit;
        }

        public bool check_quit()
        {
            return _isForce;
        }

        public bool check_suspend()
        {
            return _holdSuspend;
        }

        public void sync_stop()
        {
            Debug.Assert(Strand.WaitSafe(), "不正确的 sync_stop 调用!");
            WaitGroup wg = new WaitGroup(1);
            Stop(wg.done);
            wg.sync_wait();
        }

        public void sync_wait_stop()
        {
            Debug.Assert(Strand.WaitSafe(), "不正确的 sync_wait_stop 调用!");
            WaitGroup wg = new WaitGroup(1);
            append_stop_callback(wg.done);
            wg.sync_wait();
        }

        public bool sync_timed_wait_stop(int ms)
        {
            Debug.Assert(Strand.WaitSafe(), "不正确的 sync_timed_wait_stop 调用!");
            WaitGroup wg = new WaitGroup(1);
            WaitGroup wgCancelId = new WaitGroup(1);
            NotifyToken cancelToken = default(NotifyToken);
            append_stop_callback(wg.done, delegate (NotifyToken ct)
            {
                cancelToken = ct;
                wgCancelId.done();
            });
            if (!wg.sync_timed_wait(ms))
            {
                wgCancelId.sync_wait();
                if (null != cancelToken.token)
                {
                    wgCancelId.add(1);
                    remove_stop_callback(cancelToken, wgCancelId.done);
                    wgCancelId.sync_wait();
                    return false;
                }
            }
            return true;
        }

        static public R sync_go<R>(SharedStrand Strand, Func<Task<R>> handler)
        {
            Debug.Assert(Strand.WaitSafe(), "不正确的 sync_go 调用!");
            R res = default(R);
            System.Exception hasExcep = null;
            WaitGroup wg = new WaitGroup(1);
            Go(Strand, async delegate ()
            {
                try
                {
                    res = await handler();
                }
                catch (StopException)
                {
                    throw;
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
            }, wg.done);
            wg.sync_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
            return res;
        }

        static public R sync_go<R>(SharedStrand Strand, Func<ValueTask<R>> handler)
        {
            Debug.Assert(Strand.WaitSafe(), "不正确的 sync_go 调用!");
            R res = default(R);
            System.Exception hasExcep = null;
            WaitGroup wg = new WaitGroup(1);
            Go(Strand, async delegate ()
            {
                try
                {
                    res = await handler();
                }
                catch (StopException)
                {
                    throw;
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
            }, wg.done);
            wg.sync_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
            return res;
        }

        static public void sync_go(SharedStrand Strand, action handler)
        {
            Debug.Assert(Strand.WaitSafe(), "不正确的 sync_go 调用!");
            System.Exception hasExcep = null;
            WaitGroup wg = new WaitGroup(1);
            Go(Strand, async delegate ()
            {
                try
                {
                    await handler();
                }
                catch (StopException)
                {
                    throw;
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
            }, wg.done);
            wg.sync_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
        }

        static public Task hold()
        {
            Generator this_ = self;
            this_._pullTask.NewTask();
            return this_.async_wait();
        }

        static public Task pause_self()
        {
            Generator this_ = self;
            if (!this_._beginQuit)
            {
                if (0 == this_._lockSuspendCount)
                {
                    this_._isSuspend = true;
                    this_._suspend_cb(true);
                    this_._hasBlock = true;
                    this_._pullTask.NewTask();
                    return this_.async_wait();
                }
                else
                {
                    this_._holdSuspend = true;
                }
            }
            return non_async();
        }

        static public Task halt_self()
        {
            Generator this_ = self;
            this_.Stop();
            return non_async();
        }

        internal StopCount lock_stop_()
        {
            if (!_beginQuit)
            {
                _lockCount++;
            }
            return new StopCount { _count = _lockCount };
        }

        internal void unlock_stop_(StopCount stopCount = default(StopCount))
        {
            if (stopCount._count > 0)
            {
                _lockCount = stopCount._count;
            }
            Debug.Assert(_beginQuit || _lockCount > 0, "unlock_stop 不匹配!");
            if (!_beginQuit && 0 == --_lockCount && _isForce)
            {
                _lockSuspendCount = 0;
                _holdSuspend = false;
                _beginQuit = true;
                _suspendHandler = null;
                _timer.Cancel();
                throw new StopException();
            }
        }

        static public LockStopGuard using_lock
        {
            get
            {
                lock_stop();
                return LockStopGuard.guard;
            }
        }

        static private ValueTask<T> to_vtask<T>(T task)
        {
            return new ValueTask<T>(task);
        }

        static private ValueTask<T> to_vtask<T>(Task<T> task)
        {
            return new ValueTask<T>(task);
        }

        internal SuspendCount lock_suspend_()
        {
            if (!_beginQuit)
            {
                _lockSuspendCount++;
            }
            return new SuspendCount { _count = _lockSuspendCount };
        }

        internal Task unlock_suspend_(SuspendCount suspendCount = default(SuspendCount))
        {
            if (suspendCount._count > 0)
            {
                _lockSuspendCount = suspendCount._count;
            }
            Debug.Assert(_beginQuit || _lockSuspendCount > 0, "unlock_suspend 不匹配!");
            if (!_beginQuit && 0 == --_lockSuspendCount && _holdSuspend)
            {
                _holdSuspend = false;
                _isSuspend = true;
                _suspend_cb(true);
                _hasBlock = true;
                _pullTask.NewTask();
                return async_wait();
            }
            return non_async();
        }

        internal ShieldCount lock_suspend_and_stop_()
        {
            if (!_beginQuit)
            {
                _lockSuspendCount++;
                _lockCount++;
            }
            return new ShieldCount { _lockCount = _lockCount, _suspendCount = _lockSuspendCount };
        }

        internal Task unlock_suspend_and_stop_(ShieldCount shieldCount = default(ShieldCount))
        {
            if (shieldCount._lockCount > 0 && shieldCount._suspendCount > 0)
            {
                _lockCount = shieldCount._lockCount;
                _lockSuspendCount = shieldCount._suspendCount;
            }
            Debug.Assert(_beginQuit || _lockCount > 0, "unlock_stop 不匹配!");
            Debug.Assert(_beginQuit || _lockSuspendCount > 0, "unlock_suspend 不匹配!");
            if (!_beginQuit && 0 == --_lockCount && _isForce)
            {
                _lockSuspendCount = 0;
                _holdSuspend = false;
                _beginQuit = true;
                _suspendHandler = null;
                _timer.Cancel();
                throw new StopException();
            }
            if (!_beginQuit && 0 == --_lockSuspendCount && _holdSuspend)
            {
                _holdSuspend = false;
                _isSuspend = true;
                _suspend_cb(true);
                _hasBlock = true;
                _pullTask.NewTask();
                return async_wait();
            }
            return non_async();
        }

        static public StopCount lock_stop()
        {
            Generator this_ = self;
            return this_.lock_stop_();
        }

        static public void unlock_stop(StopCount stopCount = default(StopCount))
        {
            Generator this_ = self;
            this_.unlock_stop_(stopCount);
        }

        static public SuspendCount lock_suspend()
        {
            Generator this_ = self;
            return this_.lock_suspend_();
        }

        static public Task unlock_suspend(SuspendCount suspendCount = default(SuspendCount))
        {
            Generator this_ = self;
            return this_.unlock_suspend_(suspendCount);
        }

        static public ShieldCount lock_suspend_and_stop()
        {
            Generator this_ = self;
            return this_.lock_suspend_and_stop_();
        }

        static public Task unlock_suspend_and_stop(ShieldCount shieldCount = default(ShieldCount))
        {
            Generator this_ = self;
            return this_.unlock_suspend_and_stop_(shieldCount);
        }

        static public ShieldCount lock_shield()
        {
            return lock_suspend_and_stop();
        }

        static public Task unlock_shield(ShieldCount shieldCount = default(ShieldCount))
        {
            return unlock_suspend_and_stop(shieldCount);
        }

        private void enter_push()
        {
#if CHECK_STEP_TIMEOUT
            Debug.Assert(Strand.RunningInThisThread(), "异常的 await 调用!");
            if (!SystemTick.CheckStepDebugging() && SystemTick.GetTickMs() - _beginStepTick > _stepMaxCycle)
            {
                CallStackInfo[] stackHead = _makeStack.Last.Value;
                Debug.WriteLine(string.Format("单步超时:\n{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n",
                    stackHead[0], stackHead[1], stackHead[2], stackHead[3], stackHead[4], stackHead[5], new StackTrace(true)));
            }
#endif
        }

        private void leave_push()
        {
#if CHECK_STEP_TIMEOUT
            _beginStepTick = SystemTick.GetTickMs();
#endif
            _lastTm = 0;
            _pullTask.Activated = true;
            if (_isForce && !_beginQuit && 0 == _lockCount)
            {
                _lockSuspendCount = 0;
                _holdSuspend = false;
                _beginQuit = true;
                _suspendHandler = null;
                _timer.Cancel();
                throw new StopException();
            }
        }

        private async Task push_task()
        {
            enter_push();
            await _pullTask;
            leave_push();
        }

        private async Task<T> push_task<T>(AsyncResultWrap<T> res)
        {
            enter_push();
            await _pullTask;
            leave_push();
            return res.value1;
        }

        private async Task<TupleEx<T1, T2>> push_task<T1, T2>(AsyncResultWrap<T1, T2> res)
        {
            enter_push();
            await _pullTask;
            leave_push();
            return TupleEx.Make(res.value1, res.value2);
        }

        private async Task<TupleEx<T1, T2, T3>> push_task<T1, T2, T3>(AsyncResultWrap<T1, T2, T3> res)
        {
            enter_push();
            await _pullTask;
            leave_push();
            return TupleEx.Make(res.value1, res.value2, res.value3);
        }

        private bool new_task_completed()
        {
            if (_pullTask.IsCompleted)
            {
                enter_push();
                leave_push();
                return true;
            }
            return false;
        }

        public Task async_wait()
        {
            if (!new_task_completed())
            {
                return push_task();
            }
            return non_async();
        }

        public ValueTask<T> async_wait<T>(AsyncResultWrap<T> res)
        {
            if (!new_task_completed())
            {
                return to_vtask(push_task(res));
            }
            return to_vtask(res.value1);
        }

        public ValueTask<TupleEx<T1, T2>> async_wait<T1, T2>(AsyncResultWrap<T1, T2> res)
        {
            if (!new_task_completed())
            {
                return to_vtask(push_task(res));
            }
            return to_vtask(TupleEx.Make(res.value1, res.value2));
        }

        public ValueTask<TupleEx<T1, T2, T3>> async_wait<T1, T2, T3>(AsyncResultWrap<T1, T2, T3> res)
        {
            if (!new_task_completed())
            {
                return to_vtask(push_task(res));
            }
            return to_vtask(TupleEx.Make(res.value1, res.value2, res.value3));
        }

        private async Task<bool> timed_push_task()
        {
            enter_push();
            await _pullTask;
            leave_push();
            return !_overtime;
        }

        private async Task<TupleEx<bool, T>> timed_push_task<T>(AsyncResultWrap<T> res)
        {
            enter_push();
            await _pullTask;
            leave_push();
            return TupleEx.Make(!_overtime, res.value1);
        }

        private async Task<TupleEx<bool, TupleEx<T1, T2>>> timed_push_task<T1, T2>(AsyncResultWrap<T1, T2> res)
        {
            enter_push();
            await _pullTask;
            leave_push();
            return TupleEx.Make(!_overtime, TupleEx.Make(res.value1, res.value2));
        }

        private async Task<TupleEx<bool, TupleEx<T1, T2, T3>>> timed_push_task<T1, T2, T3>(AsyncResultWrap<T1, T2, T3> res)
        {
            enter_push();
            await _pullTask;
            leave_push();
            return TupleEx.Make(!_overtime, TupleEx.Make(res.value1, res.value2, res.value3));
        }

        public ValueTask<bool> timed_async_wait()
        {
            if (!new_task_completed())
            {
                return to_vtask(timed_push_task());
            }
            return to_vtask(!_overtime);
        }

        public ValueTask<TupleEx<bool, T>> timed_async_wait<T>(AsyncResultWrap<T> res)
        {
            if (!new_task_completed())
            {
                return to_vtask(timed_push_task(res));
            }
            return to_vtask(TupleEx.Make(!_overtime, res.value1));
        }

        public ValueTask<TupleEx<bool, TupleEx<T1, T2>>> timed_async_wait<T1, T2>(AsyncResultWrap<T1, T2> res)
        {
            if (!new_task_completed())
            {
                return to_vtask(timed_push_task(res));
            }
            return to_vtask(TupleEx.Make(!_overtime, TupleEx.Make(res.value1, res.value2)));
        }

        public ValueTask<TupleEx<bool, TupleEx<T1, T2, T3>>> timed_async_wait<T1, T2, T3>(AsyncResultWrap<T1, T2, T3> res)
        {
            if (!new_task_completed())
            {
                return to_vtask(timed_push_task(res));
            }
            return to_vtask(TupleEx.Make(!_overtime, TupleEx.Make(res.value1, res.value2, res.value3)));
        }

        public Action unsafe_async_callback(Action handler)
        {
            _pullTask.NewTask();
            return _beginQuit ? (Action)delegate ()
            {
                handler();
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    QuitNext();
                }
                else
                {
                    Strand.Post(QuitNext);
                }
            }
            : delegate ()
            {
                handler();
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    NoQuitNext();
                }
                else
                {
                    Strand.Post(NoQuitNext);
                }
            };
        }

        public Action<T1> unsafe_async_callback<T1>(Action<T1> handler)
        {
            _pullTask.NewTask();
            return _beginQuit ? (Action<T1>)delegate (T1 p1)
            {
                handler(p1);
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    QuitNext();
                }
                else
                {
                    Strand.Post(QuitNext);
                }
            }
            : delegate (T1 p1)
            {
                handler(p1);
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    NoQuitNext();
                }
                else
                {
                    Strand.Post(NoQuitNext);
                }
            };
        }

        public Action<T1, T2> unsafe_async_callback<T1, T2>(Action<T1, T2> handler)
        {
            _pullTask.NewTask();
            return _beginQuit ? (Action<T1, T2>)delegate (T1 p1, T2 p2)
            {
                handler(p1, p2);
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    QuitNext();
                }
                else
                {
                    Strand.Post(QuitNext);
                }
            }
            : delegate (T1 p1, T2 p2)
            {
                handler(p1, p2);
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    NoQuitNext();
                }
                else
                {
                    Strand.Post(NoQuitNext);
                }
            };
        }

        public Action<T1, T2, T3> unsafe_async_callback<T1, T2, T3>(Action<T1, T2, T3> handler)
        {
            _pullTask.NewTask();
            return _beginQuit ? (Action<T1, T2, T3>)delegate (T1 p1, T2 p2, T3 p3)
            {
                handler(p1, p2, p3);
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    QuitNext();
                }
                else
                {
                    Strand.Post(QuitNext);
                }
            }
            : delegate (T1 p1, T2 p2, T3 p3)
            {
                handler(p1, p2, p3);
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    NoQuitNext();
                }
                else
                {
                    Strand.Post(NoQuitNext);
                }
            };
        }

        public Action timed_async_callback(int ms, Action handler, Action timedHandler = null, Action lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    if (null != timedHandler)
                    {
                        timedHandler();
                    }
                    else if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate ()
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke();
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        handler();
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke();
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            handler();
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke();
                    });
                }
            };
        }

        public Action timed_async_callback2(int ms, Action handler, Action timedHandler = null, Action lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    Functional.CatchInvoke(timedHandler);
                    if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate ()
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke();
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        handler();
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke();
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            handler();
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke();
                    });
                }
            };
        }

        public Action<T1> timed_async_callback<T1>(int ms, Action<T1> handler, Action timedHandler = null, Action<T1> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    if (null != timedHandler)
                    {
                        timedHandler();
                    }
                    else if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        handler(p1);
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            handler(p1);
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1);
                    });
                }
            };
        }

        public Action<T1> timed_async_callback2<T1>(int ms, Action<T1> handler, Action timedHandler = null, Action<T1> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    Functional.CatchInvoke(timedHandler);
                    if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        handler(p1);
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            handler(p1);
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1);
                    });
                }
            };
        }

        public Action<T1, T2> timed_async_callback<T1, T2>(int ms, Action<T1, T2> handler, Action timedHandler = null, Action<T1, T2> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    if (null != timedHandler)
                    {
                        timedHandler();
                    }
                    else if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1, T2 p2)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        handler(p1, p2);
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            handler(p1, p2);
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2);
                    });
                }
            };
        }

        public Action<T1, T2> timed_async_callback2<T1, T2>(int ms, Action<T1, T2> handler, Action timedHandler = null, Action<T1, T2> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    Functional.CatchInvoke(timedHandler);
                    if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1, T2 p2)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        handler(p1, p2);
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            handler(p1, p2);
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2);
                    });
                }
            };
        }

        public Action<T1, T2, T3> timed_async_callback<T1, T2, T3>(int ms, Action<T1, T2, T3> handler, Action timedHandler = null, Action<T1, T2, T3> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    if (null != timedHandler)
                    {
                        timedHandler();
                    }
                    else if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1, T2 p2, T3 p3)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2, p3);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        handler(p1, p2, p3);
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2, p3);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            handler(p1, p2, p3);
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2, p3);
                    });
                }
            };
        }

        public Action<T1, T2, T3> timed_async_callback2<T1, T2, T3>(int ms, Action<T1, T2, T3> handler, Action timedHandler = null, Action<T1, T2, T3> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    Functional.CatchInvoke(timedHandler);
                    if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1, T2 p2, T3 p3)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2, p3);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        handler(p1, p2, p3);
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2, p3);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            handler(p1, p2, p3);
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2, p3);
                    });
                }
            };
        }

        public Action async_callback(Action handler, Action lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            return delegate ()
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke();
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        handler();
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke();
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            handler();
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke();
                    });
                }
            };
        }

        public Action<T1> async_callback<T1>(Action<T1> handler, Action<T1> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            return delegate (T1 p1)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        handler(p1);
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            handler(p1);
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1);
                    });
                }
            };
        }

        public Action<T1, T2> async_callback<T1, T2>(Action<T1, T2> handler, Action<T1, T2> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            return delegate (T1 p1, T2 p2)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        handler(p1, p2);
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            handler(p1, p2);
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2);
                    });
                }
            };
        }

        public Action<T1, T2, T3> async_callback<T1, T2, T3>(Action<T1, T2, T3> handler, Action<T1, T2, T3> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            return delegate (T1 p1, T2 p2, T3 p3)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2, p3);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        handler(p1, p2, p3);
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2, p3);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            handler(p1, p2, p3);
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2, p3);
                    });
                }
            };
        }

        private Action _async_result()
        {
            _pullTask.NewTask();
            return _beginQuit ? (Action)QuitNext : NoQuitNext;
        }

        public Action unsafe_async_result()
        {
            _pullTask.NewTask();
            return _beginQuit ? (Action)delegate ()
            {
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    QuitNext();
                }
                else
                {
                    Strand.Post(QuitNext);
                }
            }
            : delegate ()
            {
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    NoQuitNext();
                }
                else
                {
                    Strand.Post(NoQuitNext);
                }
            };
        }

        public Action<T1> unsafe_async_result<T1>(AsyncResultWrap<T1> res)
        {
            _pullTask.NewTask();
            return _beginQuit ? (Action<T1>)delegate (T1 p1)
            {
                res.value1 = p1;
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    QuitNext();
                }
                else
                {
                    Strand.Post(QuitNext);
                }
            }
            : delegate (T1 p1)
            {
                res.value1 = p1;
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    NoQuitNext();
                }
                else
                {
                    Strand.Post(NoQuitNext);
                }
            };
        }

        public Action<T1, T2> unsafe_async_result<T1, T2>(AsyncResultWrap<T1, T2> res)
        {
            _pullTask.NewTask();
            return _beginQuit ? (Action<T1, T2>)delegate (T1 p1, T2 p2)
            {
                res.value1 = p1;
                res.value2 = p2;
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    QuitNext();
                }
                else
                {
                    Strand.Post(QuitNext);
                }
            }
            : delegate (T1 p1, T2 p2)
            {
                res.value1 = p1;
                res.value2 = p2;
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    NoQuitNext();
                }
                else
                {
                    Strand.Post(NoQuitNext);
                }
            };
        }

        public Action<T1, T2, T3> unsafe_async_result<T1, T2, T3>(AsyncResultWrap<T1, T2, T3> res)
        {
            _pullTask.NewTask();
            return _beginQuit ? (Action<T1, T2, T3>)delegate (T1 p1, T2 p2, T3 p3)
            {
                res.value1 = p1;
                res.value2 = p2;
                res.value3 = p3;
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    QuitNext();
                }
                else
                {
                    Strand.Post(QuitNext);
                }
            }
            : delegate (T1 p1, T2 p2, T3 p3)
            {
                res.value1 = p1;
                res.value2 = p2;
                res.value3 = p3;
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    NoQuitNext();
                }
                else
                {
                    Strand.Post(NoQuitNext);
                }
            };
        }

        public Action<T1> unsafe_async_ignore<T1>()
        {
            return unsafe_async_result(AsyncResultIgnoreWrap<T1>.value);
        }

        public Action<T1, T2> unsafe_async_ignore<T1, T2>()
        {
            return unsafe_async_result(AsyncResultIgnoreWrap<T1, T2>.value);
        }

        public Action<T1, T2, T3> unsafe_async_ignore<T1, T2, T3>()
        {
            return unsafe_async_result(AsyncResultIgnoreWrap<T1, T2, T3>.value);
        }

        public Action async_result(Action lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            return delegate ()
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke();
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                    else lostHandler?.Invoke();
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check())
                        {
                            Next(multiCheck.beginQuit);
                        }
                        else lostHandler?.Invoke();
                    });
                }
            };
        }

        public Action<T1> async_result<T1>(AsyncResultWrap<T1> res, Action<T1> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            return delegate (T1 p1)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        res.value1 = p1;
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            res.value1 = p1;
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1);
                    });
                }
            };
        }

        public Action<T1, T2> async_result<T1, T2>(AsyncResultWrap<T1, T2> res, Action<T1, T2> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            return delegate (T1 p1, T2 p2)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        res.value1 = p1;
                        res.value2 = p2;
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            res.value1 = p1;
                            res.value2 = p2;
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2);
                    });
                }
            };
        }

        public Action<T1, T2, T3> async_result<T1, T2, T3>(AsyncResultWrap<T1, T2, T3> res, Action<T1, T2, T3> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            return delegate (T1 p1, T2 p2, T3 p3)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2, p3);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        res.value1 = p1;
                        res.value2 = p2;
                        res.value3 = p3;
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2, p3);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            res.value1 = p1;
                            res.value2 = p2;
                            res.value3 = p3;
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2, p3);
                    });
                }
            };
        }

        public Action<T1> async_ignore<T1>()
        {
            return async_result(AsyncResultIgnoreWrap<T1>.value);
        }

        public Action<T1, T2> async_ignore<T1, T2>()
        {
            return async_result(AsyncResultIgnoreWrap<T1, T2>.value);
        }

        public Action<T1, T2, T3> async_ignore<T1, T2, T3>()
        {
            return async_result(AsyncResultIgnoreWrap<T1, T2, T3>.value);
        }

        public Action timed_async_result(int ms, Action timedHandler = null, Action lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    if (null != timedHandler)
                    {
                        timedHandler();
                    }
                    else if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate ()
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke();
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke();
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke();
                    });
                }
            };
        }

        public Action<T1> timed_async_result<T1>(int ms, AsyncResultWrap<T1> res, Action timedHandler = null, Action<T1> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    if (null != timedHandler)
                    {
                        timedHandler();
                    }
                    else if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        res.value1 = p1;
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            res.value1 = p1;
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1);
                    });
                }
            };
        }

        public Action<T1, T2> timed_async_result<T1, T2>(int ms, AsyncResultWrap<T1, T2> res, Action timedHandler = null, Action<T1, T2> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    if (null != timedHandler)
                    {
                        timedHandler();
                    }
                    else if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1, T2 p2)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        res.value1 = p1;
                        res.value2 = p2;
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            res.value1 = p1;
                            res.value2 = p2;
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2);
                    });
                }
            };
        }

        public Action<T1, T2, T3> timed_async_result<T1, T2, T3>(int ms, AsyncResultWrap<T1, T2, T3> res, Action timedHandler = null, Action<T1, T2, T3> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    if (null != timedHandler)
                    {
                        timedHandler();
                    }
                    else if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1, T2 p2, T3 p3)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2, p3);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        res.value1 = p1;
                        res.value2 = p2;
                        res.value3 = p3;
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2, p3);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            res.value1 = p1;
                            res.value2 = p2;
                            res.value3 = p3;
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2, p3);
                    });
                }
            };
        }

        public Action timed_async_result2(int ms, Action timedHandler = null, Action lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    Functional.CatchInvoke(timedHandler);
                    if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate ()
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke();
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke();
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke();
                    });
                }
            };
        }

        public Action<T1> timed_async_result2<T1>(int ms, AsyncResultWrap<T1> res, Action timedHandler = null, Action<T1> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    Functional.CatchInvoke(timedHandler);
                    if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        res.value1 = p1;
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            res.value1 = p1;
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1);
                    });
                }
            };
        }

        public Action<T1, T2> timed_async_result2<T1, T2>(int ms, AsyncResultWrap<T1, T2> res, Action timedHandler = null, Action<T1, T2> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    Functional.CatchInvoke(timedHandler);
                    if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1, T2 p2)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        res.value1 = p1;
                        res.value2 = p2;
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            res.value1 = p1;
                            res.value2 = p2;
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2);
                    });
                }
            };
        }

        public Action<T1, T2, T3> timed_async_result2<T1, T2, T3>(int ms, AsyncResultWrap<T1, T2, T3> res, Action timedHandler = null, Action<T1, T2, T3> lostHandler = null)
        {
            MultiCheck multiCheck = NewMultiCheck();
            _overtime = false;
            if (ms >= 0)
            {
                _timer.Timeout(ms, delegate ()
                {
                    _overtime = true;
                    Functional.CatchInvoke(timedHandler);
                    if (!multiCheck.Check())
                    {
                        Next(multiCheck.beginQuit);
                    }
                });
            }
            return delegate (T1 p1, T2 p2, T3 p3)
            {
                if (multiCheck.callbacked)
                {
                    lostHandler?.Invoke(p1, p2, p3);
                    return;
                }
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                    {
                        _timer.Cancel();
                        res.value1 = p1;
                        res.value2 = p2;
                        res.value3 = p3;
                        NoCheckNext();
                    }
                    else lostHandler?.Invoke(p1, p2, p3);
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!multiCheck.Check() && !_isStop && _beginQuit == multiCheck.beginQuit)
                        {
                            _timer.Cancel();
                            res.value1 = p1;
                            res.value2 = p2;
                            res.value3 = p3;
                            NoCheckNext();
                        }
                        else lostHandler?.Invoke(p1, p2, p3);
                    });
                }
            };
        }

        public Action<T1> timed_async_ignore<T1>(int ms, Action timedHandler = null)
        {
            return timed_async_result(ms, AsyncResultIgnoreWrap<T1>.value, timedHandler);
        }

        public Action<T1, T2> timed_async_ignore<T1, T2>(int ms, Action timedHandler = null)
        {
            return timed_async_result(ms, AsyncResultIgnoreWrap<T1, T2>.value, timedHandler);
        }

        public Action<T1, T2, T3> timed_async_ignore<T1, T2, T3>(int ms, Action timedHandler = null)
        {
            return timed_async_result(ms, AsyncResultIgnoreWrap<T1, T2, T3>.value, timedHandler);
        }

        public Action<T1> timed_async_ignore2<T1>(int ms, Action timedHandler = null)
        {
            return timed_async_result2(ms, AsyncResultIgnoreWrap<T1>.value, timedHandler);
        }

        public Action<T1, T2> timed_async_ignore2<T1, T2>(int ms, Action timedHandler = null)
        {
            return timed_async_result2(ms, AsyncResultIgnoreWrap<T1, T2>.value, timedHandler);
        }

        public Action<T1, T2, T3> timed_async_ignore2<T1, T2, T3>(int ms, Action timedHandler = null)
        {
            return timed_async_result2(ms, AsyncResultIgnoreWrap<T1, T2, T3>.value, timedHandler);
        }

        static public Task usleep(long us)
        {
            if (us > 0)
            {
                Generator this_ = self;
                this_._lastTm = us;
                this_._timer.TimeoutUs(us, this_._async_result());
                return this_.async_wait();
            }
            else if (us < 0)
            {
                return hold();
            }
            else
            {
                return yield();
            }
        }

        static public Task sleep(int ms)
        {
            return usleep((long)ms * 1000);
        }

        static public Task deadline(long ms)
        {
            Generator this_ = self;
            this_._timer.Deadline(ms, this_._async_result());
            return this_.async_wait();
        }

        static public Task udeadline(long us)
        {
            Generator this_ = self;
            this_._timer.DeadlineUs(us, this_._async_result());
            return this_.async_wait();
        }

        static public Task yield()
        {
            Generator this_ = self;
            this_.Strand.Post(this_._async_result());
            return this_.async_wait();
        }

        static public Task try_yield()
        {
            Generator this_ = self;
            if (this_._lastYieldCount == this_._yieldCount)
            {
                this_._lastYieldCount = this_._yieldCount + 1;
                this_.Strand.Post(this_._async_result());
                return this_.async_wait();
            }
            this_._lastYieldCount = this_._yieldCount;
            return non_async();
        }

        static public Task yield(SharedStrand Strand)
        {
            Generator this_ = self;
            Strand.Post(Strand == this_.Strand ? this_._async_result() : this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task yield(WorkService Service)
        {
            Generator this_ = self;
            Service.Post(this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Generator self
        {
            get
            {
                SharedStrand currStrand = SharedStrand.RunningStrand();
                return null != currStrand ? currStrand.currSelf : null;
            }
        }

        static public SharedStrand SelfStrand()
        {
            Generator this_ = self;
            return null != this_ ? this_.Strand : null;
        }

        static public long self_id()
        {
            Generator this_ = self;
            return null != this_ ? this_._id : 0;
        }

        static public Generator self_parent()
        {
            Generator this_ = self;
            return this_.parent;
        }

        static public long self_count()
        {
            Generator this_ = self;
            return this_._yieldCount;
        }

        public SharedStrand Strand
        {
            get
            {
                return _timer.SelfStrand();
            }
        }

        public virtual Generator parent
        {
            get
            {
                return null;
            }
        }

        public virtual children brothers
        {
            get
            {
                return null;
            }
        }

        public long id
        {
            get
            {
                return _id;
            }
        }

        public long Count
        {
            get
            {
                return _yieldCount;
            }
        }

        static public Task suspend_other(Generator otherGen)
        {
            Generator this_ = self;
            otherGen.Suspend(this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task resume_other(Generator otherGen)
        {
            Generator this_ = self;
            otherGen.Resume(this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task chan_clear(ChannelBase Channel)
        {
            Generator this_ = self;
            Channel.AsyncClear(this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task chan_close(ChannelBase Channel, bool isClear = false)
        {
            Generator this_ = self;
            Channel.AsyncClose(this_.unsafe_async_result(), isClear);
            return this_.async_wait();
        }

        static public Task chan_cancel(ChannelBase Channel, bool isClear = false)
        {
            Generator this_ = self;
            Channel.AsyncCancel(this_.unsafe_async_result(), isClear);
            return this_.async_wait();
        }

        private async Task<bool> chan_is_closed_(ChannelBase Channel)
        {
            bool IsClosed = false;
            Action continuation = unsafe_async_result();
            Channel.SelfStrand().Post(delegate ()
            {
                IsClosed = Channel.IsClosed();
                continuation();
            });
            await push_task();
            return IsClosed;
        }

        static public ValueTask<bool> chan_is_closed(ChannelBase Channel)
        {
            Generator this_ = self;
            bool IsClosed = Channel.IsClosed();
            if (!IsClosed && Channel.SelfStrand() != this_.Strand)
            {
                return to_vtask(this_.chan_is_closed_(Channel));
            }
            return to_vtask(IsClosed);
        }

        static public Task unsafe_chan_is_closed(AsyncResultWrap<bool> res, ChannelBase Channel)
        {
            Generator this_ = self;
            res.value1 = Channel.IsClosed();
            if (!res.value1 && Channel.SelfStrand() != this_.Strand)
            {
                Action continuation = this_.unsafe_async_result();
                Channel.SelfStrand().Post(delegate ()
                {
                    res.value1 = Channel.IsClosed();
                    continuation();
                });
                return this_.async_wait();
            }
            return non_async();
        }

        private async Task<ChannelState> chan_wait_free_(AsyncResultWrap<ChannelState> res, ChannelBase Channel)
        {
            try
            {
                await push_task();
                await unlock_suspend_();
                return res.value1;
            }
            catch (StopException)
            {
                Channel.AsyncRemoveSendNotify(unsafe_async_ignore<ChannelState>(), _ioSign);
                await async_wait();
                res.value1 = ChannelState.undefined;
                throw;
            }
        }

        private async Task<ChannelState> wait_task_state_(Task task, AsyncResultWrap<ChannelState> res)
        {
            await task;
            return res.value1;
        }

        static public ValueTask<ChannelState> chan_timed_wait_free(ChannelBase Channel, int ms)
        {
            Generator this_ = self;
            Debug.Assert(!this_._ioSign._ntfNode.Effect && !this_._ioSign._success, "重叠的 chan_timed_wait_free 操作!");
            AsyncResultWrap<ChannelState> result = new AsyncResultWrap<ChannelState> { value1 = ChannelState.undefined };
            this_.lock_suspend_();
            Channel.AsyncAppendSendNotify(this_.unsafe_async_result(result), this_._ioSign, ms);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_wait_free_(result, Channel));
            }
            Task task = this_.unlock_suspend_();
            if (!task.IsCompleted)
            {
                return to_vtask(this_.wait_task_state_(task, result));
            }
            return to_vtask(result.value1);
        }

        static public ValueTask<ChannelState> chan_wait_free(ChannelBase Channel)
        {
            return chan_timed_wait_free(Channel, -1);
        }

        static public ValueTask<ChannelState> chan_timed_wait_has(ChannelBase Channel, int ms, BroadcastToken token)
        {
            Generator this_ = self;
            Debug.Assert(!this_._ioSign._ntfNode.Effect && !this_._ioSign._success, "重叠的 chan_timed_wait_has 操作!");
            AsyncResultWrap<ChannelState> result = new AsyncResultWrap<ChannelState> { value1 = ChannelState.undefined };
            this_.lock_suspend_();
            Channel.AsyncAppendRecvNotify(this_.unsafe_async_result(result), token, this_._ioSign, ms);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_wait_free_(result, Channel));
            }
            Task task = this_.unlock_suspend_();
            if (!task.IsCompleted)
            {
                return to_vtask(this_.wait_task_state_(task, result));
            }
            return to_vtask(result.value1);
        }

        static public ValueTask<ChannelState> chan_timed_wait_has(ChannelBase Channel, int ms)
        {
            return chan_timed_wait_has(Channel, ms, BroadcastToken._defToken);
        }

        static public ValueTask<ChannelState> chan_wait_has(ChannelBase Channel, BroadcastToken token)
        {
            return chan_timed_wait_has(Channel, -1, token);
        }

        static public ValueTask<ChannelState> chan_wait_has(ChannelBase Channel)
        {
            return chan_timed_wait_has(Channel, -1, BroadcastToken._defToken);
        }

        private async Task chan_cancel_wait_()
        {
            await push_task();
            await unlock_suspend_and_stop_();
        }

        static public Task chan_cancel_wait_free(ChannelBase Channel)
        {
            Generator this_ = self;
            this_.lock_suspend_and_stop_();
            Channel.AsyncRemoveSendNotify(this_.unsafe_async_ignore<ChannelState>(), this_._ioSign);
            if (!this_.new_task_completed())
            {
                return this_.chan_cancel_wait_();
            }
            return this_.unlock_suspend_and_stop_();
        }

        static public Task chan_cancel_wait_has(ChannelBase Channel)
        {
            Generator this_ = self;
            this_.lock_suspend_and_stop_();
            Channel.AsyncRemoveRecvNotify(this_.unsafe_async_ignore<ChannelState>(), this_._ioSign);
            if (!this_.new_task_completed())
            {
                return this_.chan_cancel_wait_();
            }
            return this_.unlock_suspend_and_stop_();
        }

        static public Task unsafe_chan_send<T>(AsyncResultWrap<ChannelSendWrap> res, Channel<T> Channel, T msg)
        {
            Generator this_ = self;
            res.value1 = ChannelSendWrap.def;
            Channel.AsyncSend(this_.unsafe_async_result(res), msg);
            return this_.async_wait();
        }

        private async Task<ChannelSendWrap> chan_send_<T>(AsyncResultWrap<ChannelSendWrap> res, Channel<T> Channel, T msg, ChannelLostMsg<T> lostMsg)
        {
            try
            {
                await push_task();
                return res.value1;
            }
            catch (StopException)
            {
                Channel.AsyncRemoveSendNotify(unsafe_async_ignore<ChannelState>(), _ioSign);
                await async_wait();
                if (ChannelState.ok != res.value1.state)
                {
                    lostMsg?.set(msg);
                }
                res.value1 = ChannelSendWrap.def;
                throw;
            }
        }

        static public ValueTask<ChannelSendWrap> chan_send<T>(Channel<T> Channel, T msg, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelSendWrap> result = new AsyncResultWrap<ChannelSendWrap> { value1 = ChannelSendWrap.def };
            Channel.AsyncSend(this_.unsafe_async_result(result), msg, this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_send_(result, Channel, msg, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public Task unsafe_chan_send(AsyncResultWrap<ChannelSendWrap> res, Channel<VoidType> Channel)
        {
            return unsafe_chan_send(res, Channel, default(VoidType));
        }

        static public ValueTask<ChannelSendWrap> chan_send(Channel<VoidType> Channel, ChannelLostMsg<VoidType> lostMsg = null)
        {
            return chan_send(Channel, default(VoidType), lostMsg);
        }

        static public Task unsafe_chan_force_send<T>(AsyncResultWrap<ChannelSendWrap> res, LimitChannel<T> Channel, T msg, ChannelLostMsg<T> outMsg = null)
        {
            Generator this_ = self;
            res.value1 = ChannelSendWrap.def;
            Channel.AsyncForceSend(this_.unsafe_async_callback(delegate (ChannelState state, bool hasOut, T freeMsg)
            {
                res.value1 = new ChannelSendWrap { state = state };
                if (hasOut)
                {
                    outMsg?.set(freeMsg);
                }
            }), msg);
            return this_.async_wait();
        }

        private async Task<ChannelSendWrap> chan_force_send_<T>(AsyncResultWrap<ChannelState> res, LimitChannel<T> Channel, T msg, ChannelLostMsg<T> lostMsg)
        {
            try
            {
                await push_task();
                return new ChannelSendWrap { state = res.value1 };
            }
            catch (StopException)
            {
                Channel.SelfStrand().Dispatched(unsafe_async_result());
                await async_wait();
                if (ChannelState.ok != res.value1)
                {
                    lostMsg?.set(msg);
                }
                res.value1 = ChannelState.undefined;
                throw;
            }
        }

        static public ValueTask<ChannelSendWrap> chan_force_send<T>(LimitChannel<T> Channel, T msg, ChannelLostMsg<T> outMsg = null, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelState> result = new AsyncResultWrap<ChannelState> { value1 = ChannelState.undefined };
            outMsg?.Clear();
            Channel.AsyncForceSend(this_.unsafe_async_callback(delegate (ChannelState state, bool hasOut, T freeMsg)
            {
                result.value1 = state;
                if (hasOut)
                {
                    outMsg?.set(freeMsg);
                }
            }), msg);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_force_send_(result, Channel, msg, lostMsg));
            }
            return to_vtask(new ChannelSendWrap { state = result.value1 });
        }

        static public Task unsafe_chan_timed_force_send<T>(int ms, AsyncResultWrap<ChannelSendWrap> res, LimitChannel<T> Channel, T msg, ChannelLostMsg<T> outMsg = null)
        {
            Generator this_ = self;
            res.value1 = ChannelSendWrap.def;
            Channel.AsyncTimedForceSend(ms, this_.unsafe_async_callback(delegate (ChannelState state, bool hasOut, T freeMsg)
            {
                res.value1 = new ChannelSendWrap { state = state };
                if (hasOut)
                {
                    outMsg?.set(freeMsg);
                }
            }), msg, this_._ioSign);
            return this_.async_wait();
        }

        static public ValueTask<ChannelSendWrap> chan_timed_force_send<T>(int ms, LimitChannel<T> Channel, T msg, ChannelLostMsg<T> outMsg = null, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelState> result = new AsyncResultWrap<ChannelState> { value1 = ChannelState.undefined };
            outMsg?.Clear();
            Channel.AsyncTimedForceSend(ms, this_.unsafe_async_callback(delegate (ChannelState state, bool hasOut, T freeMsg)
            {
                result.value1 = state;
                if (hasOut)
                {
                    outMsg?.set(freeMsg);
                }
            }), msg, this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_force_send_(result, Channel, msg, lostMsg));
            }
            return to_vtask(new ChannelSendWrap { state = result.value1 });
        }

        static public ValueTask<ChannelReceiveWrap<T>> chan_first<T>(LimitChannel<T> Channel)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelReceiveWrap<T>> result = new AsyncResultWrap<ChannelReceiveWrap<T>> { value1 = ChannelReceiveWrap<T>.def };
            Channel.AsyncFirst(this_.unsafe_async_result(result), this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_receive_(result, Channel, null));
            }
            return to_vtask(result.value1);
        }

        static public ValueTask<ChannelReceiveWrap<T>> chan_first<T>(UnlimitChannel<T> Channel)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelReceiveWrap<T>> result = new AsyncResultWrap<ChannelReceiveWrap<T>> { value1 = ChannelReceiveWrap<T>.def };
            Channel.AsyncFirst(this_.unsafe_async_result(result), this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_receive_(result, Channel, null));
            }
            return to_vtask(result.value1);
        }

        static public ValueTask<ChannelReceiveWrap<T>> chan_last<T>(LimitChannel<T> Channel)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelReceiveWrap<T>> result = new AsyncResultWrap<ChannelReceiveWrap<T>> { value1 = ChannelReceiveWrap<T>.def };
            Channel.AsyncLast(this_.unsafe_async_result(result), this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_receive_(result, Channel, null));
            }
            return to_vtask(result.value1);
        }

        static public ValueTask<ChannelReceiveWrap<T>> chan_last<T>(UnlimitChannel<T> Channel)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelReceiveWrap<T>> result = new AsyncResultWrap<ChannelReceiveWrap<T>> { value1 = ChannelReceiveWrap<T>.def };
            Channel.AsyncLast(this_.unsafe_async_result(result), this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_receive_(result, Channel, null));
            }
            return to_vtask(result.value1);
        }

        static public Task unsafe_chan_first<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, LimitChannel<T> Channel)
        {
            Generator this_ = self;
            res.value1 = default(ChannelReceiveWrap<T>);
            Channel.AsyncFirst(this_.unsafe_async_result(res));
            return this_.async_wait();
        }

        static public Task unsafe_chan_first<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, UnlimitChannel<T> Channel)
        {
            Generator this_ = self;
            res.value1 = default(ChannelReceiveWrap<T>);
            Channel.AsyncFirst(this_.unsafe_async_result(res));
            return this_.async_wait();
        }

        static public Task unsafe_chan_last<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, LimitChannel<T> Channel)
        {
            Generator this_ = self;
            res.value1 = default(ChannelReceiveWrap<T>);
            Channel.AsyncLast(this_.unsafe_async_result(res));
            return this_.async_wait();
        }

        static public Task unsafe_chan_last<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, UnlimitChannel<T> Channel)
        {
            Generator this_ = self;
            res.value1 = default(ChannelReceiveWrap<T>);
            Channel.AsyncLast(this_.unsafe_async_result(res));
            return this_.async_wait();
        }

        static public Task unsafe_chan_receive<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, Channel<T> Channel)
        {
            return unsafe_chan_receive(res, Channel, BroadcastToken._defToken);
        }

        static public ValueTask<ChannelReceiveWrap<T>> chan_receive<T>(Channel<T> Channel, ChannelLostMsg<T> lostMsg = null)
        {
            return chan_receive(Channel, BroadcastToken._defToken, lostMsg);
        }

        static public Task unsafe_chan_receive<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, Channel<T> Channel, BroadcastToken token)
        {
            Generator this_ = self;
            res.value1 = default(ChannelReceiveWrap<T>);
            Channel.AsyncRecv(this_.unsafe_async_result(res), token);
            return this_.async_wait();
        }

        private async Task<ChannelReceiveWrap<T>> chan_receive_<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, Channel<T> Channel, ChannelLostMsg<T> lostMsg)
        {
            try
            {
                await push_task();
                return res.value1;
            }
            catch (StopException)
            {
                Channel.AsyncRemoveRecvNotify(unsafe_async_ignore<ChannelState>(), _ioSign);
                await async_wait();
                if (ChannelState.ok == res.value1.state)
                {
                    lostMsg?.set(res.value1.msg);
                }
                res.value1 = ChannelReceiveWrap<T>.def;
                throw;
            }
        }

        static public ValueTask<ChannelReceiveWrap<T>> chan_receive<T>(Channel<T> Channel, BroadcastToken token, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelReceiveWrap<T>> result = new AsyncResultWrap<ChannelReceiveWrap<T>> { value1 = ChannelReceiveWrap<T>.def };
            Channel.AsyncRecv(this_.unsafe_async_result(result), token, this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_receive_(result, Channel, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public Task unsafe_chan_try_send<T>(AsyncResultWrap<ChannelSendWrap> res, Channel<T> Channel, T msg)
        {
            Generator this_ = self;
            res.value1 = ChannelSendWrap.def;
            Channel.AsyncTrySend(this_.unsafe_async_result(res), msg);
            return this_.async_wait();
        }

        static public ValueTask<ChannelSendWrap> chan_try_send<T>(Channel<T> Channel, T msg, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelSendWrap> result = new AsyncResultWrap<ChannelSendWrap> { value1 = ChannelSendWrap.def };
            Channel.AsyncTrySend(this_.unsafe_async_result(result), msg, this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_send_(result, Channel, msg, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public Task unsafe_chan_try_send(AsyncResultWrap<ChannelSendWrap> res, Channel<VoidType> Channel)
        {
            return unsafe_chan_try_send(res, Channel, default(VoidType));
        }

        static public ValueTask<ChannelSendWrap> chan_try_send(Channel<VoidType> Channel, ChannelLostMsg<VoidType> lostMsg = null)
        {
            return chan_try_send(Channel, default(VoidType), lostMsg);
        }

        static public Task unsafe_chan_try_receive<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, Channel<T> Channel)
        {
            return unsafe_chan_try_receive(res, Channel, BroadcastToken._defToken);
        }

        static public ValueTask<ChannelReceiveWrap<T>> chan_try_receive<T>(Channel<T> Channel, ChannelLostMsg<T> lostMsg = null)
        {
            return chan_try_receive(Channel, BroadcastToken._defToken, lostMsg);
        }

        static public Task unsafe_chan_try_receive<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, Channel<T> Channel, BroadcastToken token)
        {
            Generator this_ = self;
            res.value1 = default(ChannelReceiveWrap<T>);
            Channel.AsyncTryRecv(this_.unsafe_async_result(res), token);
            return this_.async_wait();
        }

        static public ValueTask<ChannelReceiveWrap<T>> chan_try_receive<T>(Channel<T> Channel, BroadcastToken token, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelReceiveWrap<T>> result = new AsyncResultWrap<ChannelReceiveWrap<T>> { value1 = ChannelReceiveWrap<T>.def };
            Channel.AsyncTryRecv(this_.unsafe_async_result(result), token, this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_receive_(result, Channel, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public Task unsafe_chan_timed_send<T>(AsyncResultWrap<ChannelSendWrap> res, Channel<T> Channel, int ms, T msg)
        {
            Generator this_ = self;
            res.value1 = ChannelSendWrap.def;
            Channel.AsyncTimedSend(ms, this_.unsafe_async_result(res), msg);
            return this_.async_wait();
        }

        static public ValueTask<ChannelSendWrap> chan_timed_send<T>(Channel<T> Channel, int ms, T msg, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelSendWrap> result = new AsyncResultWrap<ChannelSendWrap> { value1 = ChannelSendWrap.def };
            Channel.AsyncTimedSend(ms, this_.unsafe_async_result(result), msg, this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_send_(result, Channel, msg, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public Task unsafe_chan_timed_send(AsyncResultWrap<ChannelSendWrap> res, Channel<VoidType> Channel, int ms)
        {
            return unsafe_chan_timed_send(res, Channel, ms, default(VoidType));
        }

        static public ValueTask<ChannelSendWrap> chan_timed_send(Channel<VoidType> Channel, int ms, ChannelLostMsg<VoidType> lostMsg = null)
        {
            return chan_timed_send(Channel, ms, default(VoidType), lostMsg);
        }

        static public Task unsafe_chan_timed_receive<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, Channel<T> Channel, int ms)
        {
            return unsafe_chan_timed_receive(res, Channel, ms, BroadcastToken._defToken);
        }

        static public ValueTask<ChannelReceiveWrap<T>> chan_timed_receive<T>(Channel<T> Channel, int ms, ChannelLostMsg<T> lostMsg = null)
        {
            return chan_timed_receive(Channel, ms, BroadcastToken._defToken, lostMsg);
        }

        static public Task unsafe_chan_timed_receive<T>(AsyncResultWrap<ChannelReceiveWrap<T>> res, Channel<T> Channel, int ms, BroadcastToken token)
        {
            Generator this_ = self;
            res.value1 = default(ChannelReceiveWrap<T>);
            Channel.AsyncTimedRecv(ms, this_.unsafe_async_result(res), token);
            return this_.async_wait();
        }

        static public ValueTask<ChannelReceiveWrap<T>> chan_timed_receive<T>(Channel<T> Channel, int ms, BroadcastToken token, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<ChannelReceiveWrap<T>> result = new AsyncResultWrap<ChannelReceiveWrap<T>> { value1 = ChannelReceiveWrap<T>.def };
            Channel.AsyncTimedRecv(ms, this_.unsafe_async_result(result), token, this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.chan_receive_(result, Channel, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public Task unsafe_csp_invoke<R, T>(AsyncResultWrap<CspInvokeWrap<R>> res, CspChannel<R, T> Channel, T msg, int invokeMs = -1)
        {
            Generator this_ = self;
            res.value1 = CspInvokeWrap<R>.def;
            Channel.AsyncSend(invokeMs, this_.unsafe_async_result(res), msg);
            return this_.async_wait();
        }

        private Action<CspInvokeWrap<R>> async_csp_invoke<R>(AsyncResultWrap<CspInvokeWrap<R>> res, Action<R> lostRes)
        {
            _pullTask.NewTask();
            bool beginQuit = _beginQuit;
            return delegate (CspInvokeWrap<R> cspRes)
            {
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!_isStop && _beginQuit == beginQuit)
                    {
                        res.value1 = cspRes;
                        NoCheckNext();
                    }
                    else if (ChannelState.ok == cspRes.state)
                    {
                        Functional.CatchInvoke(lostRes, cspRes.result);
                    }
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!_isStop && _beginQuit == beginQuit)
                        {
                            res.value1 = cspRes;
                            NoCheckNext();
                        }
                        else if (ChannelState.ok == cspRes.state)
                        {
                            Functional.CatchInvoke(lostRes, cspRes.result);
                        }
                    });
                }
            };
        }

        private async Task<CspInvokeWrap<R>> csp_invoke_<R, T>(AsyncResultWrap<CspInvokeWrap<R>> res, CspChannel<R, T> Channel, T msg, ChannelLostMsg<T> lostMsg)
        {
            try
            {
                await push_task();
                return res.value1;
            }
            catch (StopException)
            {
                ChannelState rmState = ChannelState.undefined;
                Channel.AsyncRemoveSendNotify(null == lostMsg ? unsafe_async_ignore<ChannelState>() : unsafe_async_callback((ChannelState state) => rmState = state), _ioSign);
                await async_wait();
                if (ChannelState.ok == rmState)
                {
                    lostMsg?.set(msg);
                }
                res.value1 = CspInvokeWrap<R>.def;
                throw;
            }
        }

        static public ValueTask<CspInvokeWrap<R>> csp_invoke<R, T>(CspChannel<R, T> Channel, T msg, int invokeMs = -1, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<CspInvokeWrap<R>> result = new AsyncResultWrap<CspInvokeWrap<R>> { value1 = CspInvokeWrap<R>.def };
            Channel.AsyncSend(invokeMs, null == lostRes ? this_.unsafe_async_result(result) : this_.async_csp_invoke(result, lostRes), msg, this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.csp_invoke_(result, Channel, msg, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static private Action<CspInvokeWrap<R>> wrap_lost_handler<R>(Action<R> lostRes)
        {
            Action<CspInvokeWrap<R>> lostHandler = null;
            if (null != lostRes)
            {
                lostHandler = delegate (CspInvokeWrap<R> lost)
                {
                    if (ChannelState.ok == lost.state)
                    {
                        Functional.CatchInvoke(lostRes, lost.result);
                    }
                };
            }
            return lostHandler;
        }

        static public DelayCsp<R> wrap_csp_invoke<R, T>(CspChannel<R, T> Channel, T msg, int invokeMs = -1, Action<R> lostRes = null)
        {
            DelayCsp<R> result = new DelayCsp<R>(wrap_lost_handler(lostRes));
            Channel.AsyncSend(invokeMs, (CspInvokeWrap<R> res) => result.result = res, msg);
            return result;
        }

        static public Task unsafe_csp_invoke<R>(AsyncResultWrap<CspInvokeWrap<R>> res, CspChannel<R, VoidType> Channel, int invokeMs = -1)
        {
            return unsafe_csp_invoke(res, Channel, default(VoidType), invokeMs);
        }

        static public ValueTask<CspInvokeWrap<R>> csp_invoke<R>(CspChannel<R, VoidType> Channel, int invokeMs = -1, Action<R> lostRes = null, ChannelLostMsg<VoidType> lostMsg = null)
        {
            return csp_invoke(Channel, default(VoidType), invokeMs, lostRes, lostMsg);
        }

        static public DelayCsp<R> wrap_csp_invoke<R>(CspChannel<R, VoidType> Channel, int invokeMs = -1, Action<R> lostRes = null)
        {
            return wrap_csp_invoke(Channel, default(VoidType), invokeMs, lostRes);
        }

        private Action<CspWaitWrap<R, T>> async_csp_wait<R, T>(AsyncResultWrap<CspWaitWrap<R, T>> res)
        {
            _pullTask.NewTask();
            bool beginQuit = _beginQuit;
            return delegate (CspWaitWrap<R, T> cspRes)
            {
                if (Strand.RunningInThisThread() && !_mustTick)
                {
                    if (!_isStop && _beginQuit == beginQuit)
                    {
                        cspRes.result?.StartInvokeTimer(this);
                        res.value1 = cspRes;
                        NoCheckNext();
                    }
                    else cspRes.result?.Fail();
                }
                else
                {
                    Strand.Post(delegate ()
                    {
                        if (!_isStop && _beginQuit == beginQuit)
                        {
                            cspRes.result?.StartInvokeTimer(this);
                            res.value1 = cspRes;
                            NoCheckNext();
                        }
                        else cspRes.result?.Fail();
                    });
                }
            };
        }

        static public Task unsafe_csp_wait<R, T>(AsyncResultWrap<CspWaitWrap<R, T>> res, CspChannel<R, T> Channel)
        {
            Generator this_ = self;
            res.value1 = CspWaitWrap<R, T>.def;
            Channel.AsyncRecv(this_.async_csp_wait(res));
            return this_.async_wait();
        }

        private async Task<CspWaitWrap<R, T>> csp_wait_<R, T>(AsyncResultWrap<CspWaitWrap<R, T>> res, CspChannel<R, T> Channel, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            try
            {
                await push_task();
                return res.value1;
            }
            catch (StopException)
            {
                Channel.AsyncRemoveRecvNotify(unsafe_async_ignore<ChannelState>(), _ioSign);
                await async_wait();
                if (ChannelState.ok == res.value1.state)
                {
                    if (null != lostMsg)
                    {
                        lostMsg.set(res.value1);
                    }
                    else
                    {
                        res.value1.Fail();
                    }
                }
                res.value1 = CspWaitWrap<R, T>.def;
                throw;
            }
        }

        static public ValueTask<CspWaitWrap<R, T>> csp_wait<R, T>(CspChannel<R, T> Channel, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<CspWaitWrap<R, T>> result = new AsyncResultWrap<CspWaitWrap<R, T>> { value1 = CspWaitWrap<R, T>.def };
            Channel.AsyncRecv(this_.async_csp_wait(result), this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.csp_wait_(result, Channel, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public void csp_fail()
        {
            throw new CspFailException();
        }

        static public ValueTask<ChannelState> csp_wait<R, T>(CspChannel<R, T> Channel, Func<T, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return csp_wait_(Channel, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_wait<R, T>(CspChannel<R, T> Channel, Func<T, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return csp_wait_(Channel, null, handler, lostMsg);
        }

        static private async Task<ChannelState> csp_wait1_<R, T>(ValueTask<CspWaitWrap<R, T>> cspTask, Func<T, Task<R>> handler, Func<T, ValueTask<R>> gohandler)
        {
            CspWaitWrap<R, T> result = await cspTask;
            if (ChannelState.ok == result.state)
            {
                try
                {
                    result.Complete(null != handler ? await handler(result.msg) : await gohandler(result.msg));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return result.state;
        }

        static private async Task<ChannelState> csp_wait2_<R, T>(CspWaitWrap<R, T> result, ValueTask<R> callTask)
        {
            try
            {
                result.Complete(await callTask);
            }
            catch (CspFailException)
            {
                result.Fail();
            }
            catch (StopException)
            {
                result.Fail();
                throw;
            }
            catch (System.Exception)
            {
                result.Fail();
                throw;
            }
            return result.state;
        }

        static private ValueTask<ChannelState> csp_wait3_<R, T>(ValueTask<CspWaitWrap<R, T>> cspTask, Func<T, Task<R>> handler, Func<T, ValueTask<R>> gohandler)
        {
            if (!cspTask.IsCompleted)
            {
                return to_vtask(csp_wait1_(cspTask, handler, gohandler));
            }
            CspWaitWrap<R, T> result = cspTask.GetAwaiter().GetResult();
            if (ChannelState.ok == result.state)
            {
                try
                {
                    ValueTask<R> callTask = null != handler ? to_vtask(handler(result.msg)) : gohandler(result.msg);
                    if (!callTask.IsCompleted)
                    {
                        return to_vtask(csp_wait2_(result, callTask));
                    }
                    result.Complete(callTask.GetAwaiter().GetResult());
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return to_vtask(result.state);
        }

        static private ValueTask<ChannelState> csp_wait_<R, T>(CspChannel<R, T> Channel, Func<T, Task<R>> handler, Func<T, ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return csp_wait3_(csp_wait(Channel, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_wait<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
        {
            return csp_wait_(Channel, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_wait<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
        {
            return csp_wait_(Channel, null, handler, lostMsg);
        }

        static private async Task<ChannelState> csp_wait1_<R, T1, T2>(ValueTask<CspWaitWrap<R, TupleEx<T1, T2>>> cspTask, Func<T1, T2, Task<R>> handler, Func<T1, T2, ValueTask<R>> gohandler)
        {
            CspWaitWrap<R, TupleEx<T1, T2>> result = await cspTask;
            if (ChannelState.ok == result.state)
            {
                try
                {
                    result.Complete(null != handler ? await handler(result.msg.value1, result.msg.value2) : await gohandler(result.msg.value1, result.msg.value2));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return result.state;
        }

        static private ValueTask<ChannelState> csp_wait3_<R, T1, T2>(ValueTask<CspWaitWrap<R, TupleEx<T1, T2>>> cspTask, Func<T1, T2, Task<R>> handler, Func<T1, T2, ValueTask<R>> gohandler)
        {
            if (!cspTask.IsCompleted)
            {
                return to_vtask(csp_wait1_(cspTask, handler, gohandler));
            }
            CspWaitWrap<R, TupleEx<T1, T2>> result = cspTask.GetAwaiter().GetResult();
            if (ChannelState.ok == result.state)
            {
                try
                {
                    ValueTask<R> callTask = null != handler ? to_vtask(handler(result.msg.value1, result.msg.value2)) : gohandler(result.msg.value1, result.msg.value2);
                    if (!callTask.IsCompleted)
                    {
                        return to_vtask(csp_wait2_(result, callTask));
                    }
                    result.Complete(callTask.GetAwaiter().GetResult());
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return to_vtask(result.state);
        }

        static private ValueTask<ChannelState> csp_wait_<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, Task<R>> handler, Func<T1, T2, ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg)
        {
            return csp_wait3_(csp_wait(Channel, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_wait<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
        {
            return csp_wait_(Channel, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_wait<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
        {
            return csp_wait_(Channel, null, handler, lostMsg);
        }

        static private async Task<ChannelState> csp_wait1_<R, T1, T2, T3>(ValueTask<CspWaitWrap<R, TupleEx<T1, T2, T3>>> cspTask, Func<T1, T2, T3, Task<R>> handler, Func<T1, T2, T3, ValueTask<R>> gohandler)
        {
            CspWaitWrap<R, TupleEx<T1, T2, T3>> result = await cspTask;
            if (ChannelState.ok == result.state)
            {
                try
                {
                    result.Complete(null != handler ? await handler(result.msg.value1, result.msg.value2, result.msg.value3) : await gohandler(result.msg.value1, result.msg.value2, result.msg.value3));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return result.state;
        }

        static private ValueTask<ChannelState> csp_wait3_<R, T1, T2, T3>(ValueTask<CspWaitWrap<R, TupleEx<T1, T2, T3>>> cspTask, Func<T1, T2, T3, Task<R>> handler, Func<T1, T2, T3, ValueTask<R>> gohandler)
        {
            if (!cspTask.IsCompleted)
            {
                return to_vtask(csp_wait1_(cspTask, handler, gohandler));
            }
            CspWaitWrap<R, TupleEx<T1, T2, T3>> result = cspTask.GetAwaiter().GetResult();
            if (ChannelState.ok == result.state)
            {
                try
                {
                    ValueTask<R> callTask = null != handler ? to_vtask(handler(result.msg.value1, result.msg.value2, result.msg.value3)) : gohandler(result.msg.value1, result.msg.value2, result.msg.value3);
                    if (!callTask.IsCompleted)
                    {
                        return to_vtask(csp_wait2_(result, callTask));
                    }
                    result.Complete(callTask.GetAwaiter().GetResult());
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return to_vtask(result.state);
        }

        static private ValueTask<ChannelState> csp_wait_<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task<R>> handler, Func<T1, T2, T3, ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg)
        {
            return csp_wait3_(csp_wait(Channel, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_wait<R>(CspChannel<R, VoidType> Channel, Func<Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
        {
            return csp_wait_(Channel, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_wait<R>(CspChannel<R, VoidType> Channel, Func<ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
        {
            return csp_wait_(Channel, null, handler, lostMsg);
        }

        static private async Task<ChannelState> csp_wait1_<R>(ValueTask<CspWaitWrap<R, VoidType>> cspTask, Func<Task<R>> handler, Func<ValueTask<R>> gohandler)
        {
            CspWaitWrap<R, VoidType> result = await cspTask;
            if (ChannelState.ok == result.state)
            {
                try
                {
                    result.Complete(null != handler ? await handler() : await gohandler());
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return result.state;
        }

        static private ValueTask<ChannelState> csp_wait3_<R>(ValueTask<CspWaitWrap<R, VoidType>> cspTask, Func<Task<R>> handler, Func<ValueTask<R>> gohandler)
        {
            if (!cspTask.IsCompleted)
            {
                return to_vtask(csp_wait1_(cspTask, handler, gohandler));
            }
            CspWaitWrap<R, VoidType> result = cspTask.GetAwaiter().GetResult();
            if (ChannelState.ok == result.state)
            {
                try
                {
                    ValueTask<R> callTask = null != handler ? to_vtask(handler()) : gohandler();
                    if (!callTask.IsCompleted)
                    {
                        return to_vtask(csp_wait2_(result, callTask));
                    }
                    result.Complete(callTask.GetAwaiter().GetResult());
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return to_vtask(result.state);
        }

        static private ValueTask<ChannelState> csp_wait_<R>(CspChannel<R, VoidType> Channel, Func<Task<R>> handler, Func<ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg)
        {
            return csp_wait3_(csp_wait(Channel, lostMsg), handler, gohandler);
        }

        static private async Task<ChannelState> csp_wait1_<T>(ValueTask<CspWaitWrap<VoidType, T>> cspTask, Func<T, Task> handler)
        {
            CspWaitWrap<VoidType, T> result = await cspTask;
            if (ChannelState.ok == result.state)
            {
                try
                {
                    await handler(result.msg);
                    result.Complete(default(VoidType));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return result.state;
        }

        static private async Task<ChannelState> csp_wait2_<T>(CspWaitWrap<VoidType, T> result, Task callTask)
        {
            try
            {
                await callTask;
                result.Complete(default(VoidType));
            }
            catch (CspFailException)
            {
                result.Fail();
            }
            catch (StopException)
            {
                result.Fail();
                throw;
            }
            catch (System.Exception)
            {
                result.Fail();
                throw;
            }
            return result.state;
        }

        static private ValueTask<ChannelState> csp_wait3_<T>(ValueTask<CspWaitWrap<VoidType, T>> cspTask, Func<T, Task> handler)
        {
            if (!cspTask.IsCompleted)
            {
                return to_vtask(csp_wait1_(cspTask, handler));
            }
            CspWaitWrap<VoidType, T> result = cspTask.GetAwaiter().GetResult();
            if (ChannelState.ok == result.state)
            {
                try
                {
                    Task callTask = handler(result.msg);
                    if (!callTask.IsCompleted)
                    {
                        return to_vtask(csp_wait2_(result, callTask));
                    }
                    result.Complete(default(VoidType));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return to_vtask(result.state);
        }

        static public ValueTask<ChannelState> csp_wait<T>(CspChannel<VoidType, T> Channel, Func<T, Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, T>> lostMsg = null)
        {
            return csp_wait3_(csp_wait(Channel, lostMsg), handler);
        }

        static private async Task<ChannelState> csp_wait1_<T1, T2>(ValueTask<CspWaitWrap<VoidType, TupleEx<T1, T2>>> cspTask, Func<T1, T2, Task> handler)
        {
            CspWaitWrap<VoidType, TupleEx<T1, T2>> result = await cspTask;
            if (ChannelState.ok == result.state)
            {
                try
                {
                    await handler(result.msg.value1, result.msg.value2);
                    result.Complete(default(VoidType));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return result.state;
        }

        static private ValueTask<ChannelState> csp_wait3_<T1, T2>(ValueTask<CspWaitWrap<VoidType, TupleEx<T1, T2>>> cspTask, Func<T1, T2, Task> handler)
        {
            if (!cspTask.IsCompleted)
            {
                return to_vtask(csp_wait1_(cspTask, handler));
            }
            CspWaitWrap<VoidType, TupleEx<T1, T2>> result = cspTask.GetAwaiter().GetResult();
            if (ChannelState.ok == result.state)
            {
                try
                {
                    Task callTask = handler(result.msg.value1, result.msg.value2);
                    if (!callTask.IsCompleted)
                    {
                        return to_vtask(csp_wait2_(result, callTask));
                    }
                    result.Complete(default(VoidType));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return to_vtask(result.state);
        }

        static public ValueTask<ChannelState> csp_wait<T1, T2>(CspChannel<VoidType, TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2>>> lostMsg = null)
        {
            return csp_wait3_(csp_wait(Channel, lostMsg), handler);
        }

        static private async Task<ChannelState> csp_wait1_<T1, T2, T3>(ValueTask<CspWaitWrap<VoidType, TupleEx<T1, T2, T3>>> cspTask, Func<T1, T2, T3, Task> handler)
        {
            CspWaitWrap<VoidType, TupleEx<T1, T2, T3>> result = await cspTask;
            if (ChannelState.ok == result.state)
            {
                try
                {
                    await handler(result.msg.value1, result.msg.value2, result.msg.value3);
                    result.Complete(default(VoidType));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return result.state;
        }

        static private ValueTask<ChannelState> csp_wait3_<T1, T2, T3>(ValueTask<CspWaitWrap<VoidType, TupleEx<T1, T2, T3>>> cspTask, Func<T1, T2, T3, Task> handler)
        {
            if (!cspTask.IsCompleted)
            {
                return to_vtask(csp_wait1_(cspTask, handler));
            }
            CspWaitWrap<VoidType, TupleEx<T1, T2, T3>> result = cspTask.GetAwaiter().GetResult();
            if (ChannelState.ok == result.state)
            {
                try
                {
                    Task callTask = handler(result.msg.value1, result.msg.value2, result.msg.value3);
                    if (!callTask.IsCompleted)
                    {
                        return to_vtask(csp_wait2_(result, callTask));
                    }
                    result.Complete(default(VoidType));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return to_vtask(result.state);
        }

        static public ValueTask<ChannelState> csp_wait<T1, T2, T3>(CspChannel<VoidType, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2, T3>>> lostMsg = null)
        {
            return csp_wait3_(csp_wait(Channel, lostMsg), handler);
        }

        static private async Task<ChannelState> csp_wait1_(ValueTask<CspWaitWrap<VoidType, VoidType>> cspTask, Func<Task> handler)
        {
            CspWaitWrap<VoidType, VoidType> result = await cspTask;
            if (ChannelState.ok == result.state)
            {
                try
                {
                    await handler();
                    result.Complete(default(VoidType));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return result.state;
        }

        static private ValueTask<ChannelState> csp_wait3_(ValueTask<CspWaitWrap<VoidType, VoidType>> cspTask, Func<Task> handler)
        {
            if (!cspTask.IsCompleted)
            {
                return to_vtask(csp_wait1_(cspTask, handler));
            }
            CspWaitWrap<VoidType, VoidType> result = cspTask.GetAwaiter().GetResult();
            if (ChannelState.ok == result.state)
            {
                try
                {
                    Task callTask = handler();
                    if (!callTask.IsCompleted)
                    {
                        return to_vtask(csp_wait2_(result, callTask));
                    }
                    result.Complete(default(VoidType));
                }
                catch (CspFailException)
                {
                    result.Fail();
                }
                catch (StopException)
                {
                    result.Fail();
                    throw;
                }
                catch (System.Exception)
                {
                    result.Fail();
                    throw;
                }
            }
            return to_vtask(result.state);
        }

        static public ValueTask<ChannelState> csp_wait(CspChannel<VoidType, VoidType> Channel, Func<Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, VoidType>> lostMsg = null)
        {
            return csp_wait3_(csp_wait(Channel, lostMsg), handler);
        }

        static public Task unsafe_csp_try_invoke<R, T>(AsyncResultWrap<CspInvokeWrap<R>> res, CspChannel<R, T> Channel, T msg, int invokeMs = -1)
        {
            Generator this_ = self;
            res.value1 = CspInvokeWrap<R>.def;
            Channel.AsyncTrySend(invokeMs, this_.unsafe_async_result(res), msg);
            return this_.async_wait();
        }

        static public ValueTask<CspInvokeWrap<R>> csp_try_invoke<R, T>(CspChannel<R, T> Channel, T msg, int invokeMs = -1, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<CspInvokeWrap<R>> result = new AsyncResultWrap<CspInvokeWrap<R>> { value1 = CspInvokeWrap<R>.def };
            Channel.AsyncTrySend(invokeMs, null == lostRes ? this_.unsafe_async_result(result) : this_.async_csp_invoke(result, lostRes), msg, this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.csp_invoke_(result, Channel, msg, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public DelayCsp<R> wrap_csp_try_invoke<R, T>(CspChannel<R, T> Channel, T msg, int invokeMs = -1, Action<R> lostRes = null)
        {
            DelayCsp<R> result = new DelayCsp<R>(wrap_lost_handler(lostRes));
            Channel.AsyncTrySend(invokeMs, (CspInvokeWrap<R> res) => result.result = res, msg);
            return result;
        }

        static public Task unsafe_csp_try_invoke<R>(AsyncResultWrap<CspInvokeWrap<R>> res, CspChannel<R, VoidType> Channel, int invokeMs = -1)
        {
            return unsafe_csp_try_invoke(res, Channel, default(VoidType), invokeMs);
        }

        static public ValueTask<CspInvokeWrap<R>> csp_try_invoke<R>(CspChannel<R, VoidType> Channel, int invokeMs = -1, Action<R> lostRes = null, ChannelLostMsg<VoidType> lostMsg = null)
        {
            return csp_try_invoke(Channel, default(VoidType), invokeMs, lostRes, lostMsg);
        }

        static public DelayCsp<R> wrap_csp_try_invoke<R>(CspChannel<R, VoidType> Channel, int invokeMs = -1, Action<R> lostRes = null)
        {
            return wrap_csp_try_invoke(Channel, default(VoidType), invokeMs, lostRes);
        }

        static public Task unsafe_csp_try_wait<R, T>(AsyncResultWrap<CspWaitWrap<R, T>> res, CspChannel<R, T> Channel)
        {
            Generator this_ = self;
            res.value1 = CspWaitWrap<R, T>.def;
            Channel.AsyncTryRecv(this_.async_csp_wait(res));
            return this_.async_wait();
        }

        static public ValueTask<CspWaitWrap<R, T>> csp_try_wait<R, T>(CspChannel<R, T> Channel, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<CspWaitWrap<R, T>> result = new AsyncResultWrap<CspWaitWrap<R, T>> { value1 = CspWaitWrap<R, T>.def };
            Channel.AsyncTryRecv(this_.async_csp_wait(result), this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.csp_wait_(result, Channel, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public ValueTask<ChannelState> csp_try_wait<R, T>(CspChannel<R, T> Channel, Func<T, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return csp_try_wait_(Channel, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_try_wait<R, T>(CspChannel<R, T> Channel, Func<T, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return csp_try_wait_(Channel, null, handler, lostMsg);
        }

        static private ValueTask<ChannelState> csp_try_wait_<R, T>(CspChannel<R, T> Channel, Func<T, Task<R>> handler, Func<T, ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return csp_wait3_(csp_try_wait(Channel, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_try_wait<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
        {
            return csp_try_wait_(Channel, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_try_wait<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
        {
            return csp_try_wait_(Channel, null, handler, lostMsg);
        }

        static private ValueTask<ChannelState> csp_try_wait_<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, Task<R>> handler, Func<T1, T2, ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg)
        {
            return csp_wait3_(csp_try_wait(Channel, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_try_wait<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
        {
            return csp_try_wait_(Channel, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_try_wait<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
        {
            return csp_try_wait_(Channel, null, handler, lostMsg);
        }

        static private ValueTask<ChannelState> csp_try_wait_<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task<R>> handler, Func<T1, T2, T3, ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg)
        {
            return csp_wait3_(csp_try_wait(Channel, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_try_wait<R>(CspChannel<R, VoidType> Channel, Func<Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
        {
            return csp_try_wait_(Channel, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_try_wait<R>(CspChannel<R, VoidType> Channel, Func<ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
        {
            return csp_try_wait_(Channel, null, handler, lostMsg);
        }

        static private ValueTask<ChannelState> csp_try_wait_<R>(CspChannel<R, VoidType> Channel, Func<Task<R>> handler, Func<ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg)
        {
            return csp_wait3_(csp_try_wait(Channel, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_try_wait<T>(CspChannel<VoidType, T> Channel, Func<T, Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, T>> lostMsg = null)
        {
            return csp_wait3_(csp_try_wait(Channel, lostMsg), handler);
        }

        static public ValueTask<ChannelState> csp_try_wait<T1, T2>(CspChannel<VoidType, TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2>>> lostMsg = null)
        {
            return csp_wait3_(csp_try_wait(Channel, lostMsg), handler);
        }

        static public ValueTask<ChannelState> csp_try_wait<T1, T2, T3>(CspChannel<VoidType, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2, T3>>> lostMsg = null)
        {
            return csp_wait3_(csp_try_wait(Channel, lostMsg), handler);
        }

        static public ValueTask<ChannelState> csp_try_wait(CspChannel<VoidType, VoidType> Channel, Func<Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, VoidType>> lostMsg = null)
        {
            return csp_wait3_(csp_try_wait(Channel, lostMsg), handler);
        }

        static public Task unsafe_csp_timed_invoke<R, T>(AsyncResultWrap<CspInvokeWrap<R>> res, CspChannel<R, T> Channel, TupleEx<int, int> ms, T msg)
        {
            Generator this_ = self;
            res.value1 = CspInvokeWrap<R>.def;
            Channel.AsyncTimedSend(ms.value1, ms.value2, this_.unsafe_async_result(res), msg);
            return this_.async_wait();
        }

        static public ValueTask<CspInvokeWrap<R>> csp_timed_invoke<R, T>(CspChannel<R, T> Channel, TupleEx<int, int> ms, T msg, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<CspInvokeWrap<R>> result = new AsyncResultWrap<CspInvokeWrap<R>> { value1 = CspInvokeWrap<R>.def };
            Channel.AsyncTimedSend(ms.value1, ms.value2, null == lostRes ? this_.unsafe_async_result(result) : this_.async_csp_invoke(result, lostRes), msg, this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.csp_invoke_(result, Channel, msg, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public DelayCsp<R> wrap_csp_timed_invoke<R, T>(CspChannel<R, T> Channel, TupleEx<int, int> ms, T msg, Action<R> lostRes = null)
        {
            DelayCsp<R> result = new DelayCsp<R>(wrap_lost_handler(lostRes));
            Channel.AsyncTimedSend(ms.value1, ms.value2, (CspInvokeWrap<R> res) => result.result = res, msg);
            return result;
        }

        static public Task unsafe_csp_timed_invoke<R, T>(AsyncResultWrap<CspInvokeWrap<R>> res, CspChannel<R, T> Channel, int ms, T msg)
        {
            return unsafe_csp_timed_invoke(res, Channel, TupleEx.Make(ms, -1), msg);
        }

        static public ValueTask<CspInvokeWrap<R>> csp_timed_invoke<R, T>(CspChannel<R, T> Channel, int ms, T msg, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
        {
            return csp_timed_invoke(Channel, TupleEx.Make(ms, -1), msg, lostRes, lostMsg);
        }

        static public DelayCsp<R> wrap_csp_timed_invoke<R, T>(CspChannel<R, T> Channel, int ms, T msg, Action<R> lostRes = null)
        {
            return wrap_csp_timed_invoke(Channel, TupleEx.Make(ms, -1), msg, lostRes);
        }

        static public Task unsafe_csp_timed_invoke<R>(AsyncResultWrap<CspInvokeWrap<R>> res, CspChannel<R, VoidType> Channel, TupleEx<int, int> ms)
        {
            return unsafe_csp_timed_invoke(res, Channel, ms, default(VoidType));
        }

        static public ValueTask<CspInvokeWrap<R>> csp_timed_invoke<R>(CspChannel<R, VoidType> Channel, TupleEx<int, int> ms, Action<R> lostRes = null, ChannelLostMsg<VoidType> lostMsg = null)
        {
            return csp_timed_invoke(Channel, ms, default(VoidType), lostRes, lostMsg);
        }

        static public DelayCsp<R> wrap_csp_timed_invoke<R>(CspChannel<R, VoidType> Channel, TupleEx<int, int> ms, Action<R> lostRes = null)
        {
            return wrap_csp_timed_invoke(Channel, ms, default(VoidType), lostRes);
        }

        static public Task unsafe_csp_timed_invoke<R>(AsyncResultWrap<CspInvokeWrap<R>> res, CspChannel<R, VoidType> Channel, int ms)
        {
            return unsafe_csp_timed_invoke(res, Channel, ms, default(VoidType));
        }

        static public ValueTask<CspInvokeWrap<R>> csp_timed_invoke<R>(CspChannel<R, VoidType> Channel, int ms, Action<R> lostRes = null, ChannelLostMsg<VoidType> lostMsg = null)
        {
            return csp_timed_invoke(Channel, ms, default(VoidType), lostRes, lostMsg);
        }

        static public DelayCsp<R> wrap_csp_timed_invoke<R>(CspChannel<R, VoidType> Channel, int ms, Action<R> lostRes = null)
        {
            return wrap_csp_timed_invoke(Channel, ms, default(VoidType), lostRes);
        }

        static public Task unsafe_csp_timed_wait<R, T>(AsyncResultWrap<CspWaitWrap<R, T>> res, CspChannel<R, T> Channel, int ms)
        {
            Generator this_ = self;
            res.value1 = CspWaitWrap<R, T>.def;
            Channel.AsyncTimedRecv(ms, this_.async_csp_wait(res));
            return this_.async_wait();
        }

        static public ValueTask<CspWaitWrap<R, T>> csp_timed_wait<R, T>(CspChannel<R, T> Channel, int ms, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            Generator this_ = self;
            AsyncResultWrap<CspWaitWrap<R, T>> result = new AsyncResultWrap<CspWaitWrap<R, T>> { value1 = CspWaitWrap<R, T>.def };
            Channel.AsyncTimedRecv(ms, this_.async_csp_wait(result), this_._ioSign);
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.csp_wait_(result, Channel, lostMsg));
            }
            return to_vtask(result.value1);
        }

        static public ValueTask<ChannelState> csp_timed_wait<R, T>(CspChannel<R, T> Channel, int ms, Func<T, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return csp_timed_wait_(Channel, ms, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_timed_wait<R, T>(CspChannel<R, T> Channel, int ms, Func<T, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
        {
            return csp_timed_wait_(Channel, ms, null, handler, lostMsg);
        }

        static private ValueTask<ChannelState> csp_timed_wait_<R, T>(CspChannel<R, T> Channel, int ms, Func<T, Task<R>> handler, Func<T, ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
        {
            return csp_wait3_(csp_timed_wait(Channel, ms, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_timed_wait<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
        {
            return csp_timed_wait_(Channel, ms, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_timed_wait<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
        {
            return csp_timed_wait_(Channel, ms, null, handler, lostMsg);
        }

        static private ValueTask<ChannelState> csp_timed_wait_<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task<R>> handler, Func<T1, T2, ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg)
        {
            return csp_wait3_(csp_timed_wait(Channel, ms, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_timed_wait<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
        {
            return csp_timed_wait_(Channel, ms, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_timed_wait<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
        {
            return csp_timed_wait_(Channel, ms, null, handler, lostMsg);
        }

        static private ValueTask<ChannelState> csp_timed_wait_<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task<R>> handler, Func<T1, T2, T3, ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg)
        {
            return csp_wait3_(csp_timed_wait(Channel, ms, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_timed_wait<R>(CspChannel<R, VoidType> Channel, int ms, Func<Task<R>> handler, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
        {
            return csp_timed_wait_(Channel, ms, handler, null, lostMsg);
        }

        static public ValueTask<ChannelState> csp_timed_wait<R>(CspChannel<R, VoidType> Channel, int ms, Func<ValueTask<R>> handler, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
        {
            return csp_timed_wait_(Channel, ms, null, handler, lostMsg);
        }

        static private ValueTask<ChannelState> csp_timed_wait_<R>(CspChannel<R, VoidType> Channel, int ms, Func<Task<R>> handler, Func<ValueTask<R>> gohandler, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg)
        {
            return csp_wait3_(csp_timed_wait(Channel, ms, lostMsg), handler, gohandler);
        }

        static public ValueTask<ChannelState> csp_timed_wait<T>(CspChannel<VoidType, T> Channel, int ms, Func<T, Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, T>> lostMsg = null)
        {
            return csp_wait3_(csp_timed_wait(Channel, ms, lostMsg), handler);
        }

        static public ValueTask<ChannelState> csp_timed_wait<T1, T2>(CspChannel<VoidType, TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2>>> lostMsg = null)
        {
            return csp_wait3_(csp_timed_wait(Channel, ms, lostMsg), handler);
        }

        static public ValueTask<ChannelState> csp_timed_wait<T1, T2, T3>(CspChannel<VoidType, TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2, T3>>> lostMsg = null)
        {
            return csp_wait3_(csp_timed_wait(Channel, ms, lostMsg), handler);
        }

        static public ValueTask<ChannelState> csp_timed_wait(CspChannel<VoidType, VoidType> Channel, int ms, Func<Task> handler, ChannelLostMsg<CspWaitWrap<VoidType, VoidType>> lostMsg = null)
        {
            return csp_wait3_(csp_timed_wait(Channel, ms, lostMsg), handler);
        }

        static public async Task<int> chans_broadcast<T>(T msg, params Channel<T>[] chans)
        {
            Generator this_ = self;
            int Count = 0;
            WaitGroup wg = new WaitGroup(chans.Length);
            for (int i = 0; i < chans.Length; i++)
            {
                chans[i].AsyncSend(delegate (ChannelSendWrap sendRes)
                {
                    if (ChannelState.ok == sendRes.state)
                    {
                        Interlocked.Increment(ref Count);
                    }
                    wg.done();
                }, msg, null);
            }
            WaitGroup.cancel_token cancelToken = wg.async_wait(this_.unsafe_async_result());
            try
            {
                await this_.async_wait();
                return Count;
            }
            finally
            {
                wg.cancel_wait(cancelToken);
            }
        }

        static public async Task<int> chans_try_broadcast<T>(T msg, params Channel<T>[] chans)
        {
            Generator this_ = self;
            int Count = 0;
            WaitGroup wg = new WaitGroup(chans.Length);
            for (int i = 0; i < chans.Length; i++)
            {
                chans[i].AsyncTrySend(delegate (ChannelSendWrap sendRes)
                {
                    if (ChannelState.ok == sendRes.state)
                    {
                        Interlocked.Increment(ref Count);
                    }
                    wg.done();
                }, msg, null);
            }
            WaitGroup.cancel_token cancelToken = wg.async_wait(this_.unsafe_async_result());
            try
            {
                await this_.async_wait();
                return Count;
            }
            finally
            {
                wg.cancel_wait(cancelToken);
            }
        }

        static public async Task<int> chans_timed_broadcast<T>(int ms, T msg, params Channel<T>[] chans)
        {
            Generator this_ = self;
            int Count = 0;
            WaitGroup wg = new WaitGroup(chans.Length);
            for (int i = 0; i < chans.Length; i++)
            {
                chans[i].AsyncTimedSend(ms, delegate (ChannelSendWrap sendRes)
                {
                    if (ChannelState.ok == sendRes.state)
                    {
                        Interlocked.Increment(ref Count);
                    }
                    wg.done();
                }, msg, null);
            }
            WaitGroup.cancel_token cancelToken = wg.async_wait(this_.unsafe_async_result());
            try
            {
                await this_.async_wait();
                return Count;
            }
            finally
            {
                wg.cancel_wait(cancelToken);
            }
        }

        static public Task non_async()
        {
            return _nilTask;
        }

        static public ValueTask<R> non_async<R>(R value)
        {
            return to_vtask(value);
        }

        static public Task mutex_cancel(GoMutex mtx)
        {
            Generator this_ = self;
            mtx.AsyncCancel(this_._id, this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task mutex_lock(GoMutex mtx)
        {
            Generator this_ = self;
            //mtx.AsyncLock(this_._id, this_.unsafe_async_result());
            mtx.async_lock(this_._id, this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task mutex_try_lock(AsyncResultWrap<bool> res, GoMutex mtx)
        {
            Generator this_ = self;
            res.value1 = false;
            //mtx.AsyncTryLock(this_._id, this_.async_result(res));
            mtx.async_try_lock(this_._id, this_.async_result(res));
            return this_.async_wait();
        }

        static public ValueTask<bool> mutex_try_lock(GoMutex mtx)
        {
            Generator this_ = self;
            AsyncResultWrap<bool> res = new AsyncResultWrap<bool>();
            //mtx.AsyncTryLock(this_._id, this_.unsafe_async_result(res));
            mtx.async_try_lock(this_._id, this_.unsafe_async_result(res));
            return this_.async_wait(res);
        }

        static public Task mutex_timed_lock(AsyncResultWrap<bool> res, GoMutex mtx, int ms)
        {
            Generator this_ = self;
            res.value1 = false;
            mtx.async_timed_lock(this_._id, ms, this_.async_result(res));
            return this_.async_wait();
        }

        static public ValueTask<bool> mutex_timed_lock(GoMutex mtx, int ms)
        {
            Generator this_ = self;
            AsyncResultWrap<bool> res = new AsyncResultWrap<bool>();
            mtx.async_timed_lock(this_._id, ms, this_.unsafe_async_result(res));
            return this_.async_wait(res);
        }

        static public Task mutex_unlock(GoMutex mtx)
        {
            Generator this_ = self;
            mtx.async_unlock(this_._id, this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task mutex_lock_shared(GoSharedMutex mtx)
        {
            Generator this_ = self;
            mtx.async_lock_shared(this_._id, this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task mutex_lock_pess_shared(GoSharedMutex mtx)
        {
            Generator this_ = self;
            mtx.async_lock_pess_shared(this_._id, this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task mutex_lock_upgrade(GoSharedMutex mtx)
        {
            Generator this_ = self;
            mtx.async_lock_upgrade(this_._id, this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task mutex_try_lock_shared(AsyncResultWrap<bool> res, GoSharedMutex mtx)
        {
            Generator this_ = self;
            res.value1 = false;
            mtx.async_try_lock_shared(this_._id, this_.async_result(res));
            return this_.async_wait();
        }

        static public ValueTask<bool> mutex_try_lock_shared(GoSharedMutex mtx)
        {
            Generator this_ = self;
            AsyncResultWrap<bool> res = new AsyncResultWrap<bool>();
            mtx.async_try_lock_shared(this_._id, this_.unsafe_async_result(res));
            return this_.async_wait(res);
        }

        static public Task mutex_try_lock_upgrade(AsyncResultWrap<bool> res, GoSharedMutex mtx)
        {
            Generator this_ = self;
            res.value1 = false;
            mtx.async_try_lock_upgrade(this_._id, this_.async_result(res));
            return this_.async_wait();
        }

        static public ValueTask<bool> mutex_try_lock_upgrade(GoSharedMutex mtx)
        {
            Generator this_ = self;
            AsyncResultWrap<bool> res = new AsyncResultWrap<bool>();
            mtx.async_try_lock_upgrade(this_._id, this_.unsafe_async_result(res));
            return this_.async_wait(res);
        }

        static public Task mutex_timed_lock_shared(AsyncResultWrap<bool> res, GoSharedMutex mtx, int ms)
        {
            Generator this_ = self;
            res.value1 = false;
            mtx.async_timed_lock_shared(this_._id, ms, this_.async_result(res));
            return this_.async_wait();
        }

        static public ValueTask<bool> mutex_timed_lock_shared(GoSharedMutex mtx, int ms)
        {
            Generator this_ = self;
            AsyncResultWrap<bool> res = new AsyncResultWrap<bool>();
            mtx.async_timed_lock_shared(this_._id, ms, this_.unsafe_async_result(res));
            return this_.async_wait(res);
        }

        static public Task mutex_unlock_shared(GoSharedMutex mtx)
        {
            Generator this_ = self;
            mtx.async_unlock_shared(this_._id, this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task mutex_unlock_upgrade(GoSharedMutex mtx)
        {
            Generator this_ = self;
            mtx.async_unlock_upgrade(this_._id, this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task condition_wait(GoConditionVariable conVar, GoMutex mutex)
        {
            Generator this_ = self;
            conVar.async_wait(this_._id, mutex, this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task condition_timed_wait(AsyncResultWrap<bool> res, GoConditionVariable conVar, GoMutex mutex, int ms)
        {
            Generator this_ = self;
            res.value1 = false;
            conVar.async_timed_wait(this_._id, ms, mutex, this_.async_result(res));
            return this_.async_wait();
        }

        static public ValueTask<bool> condition_timed_wait(GoConditionVariable conVar, GoMutex mutex, int ms)
        {
            Generator this_ = self;
            AsyncResultWrap<bool> res = new AsyncResultWrap<bool>();
            conVar.async_timed_wait(this_._id, ms, mutex, this_.unsafe_async_result(res));
            return this_.async_wait(res);
        }

        static public Task condition_cancel(GoConditionVariable conVar)
        {
            Generator this_ = self;
            conVar.AsyncCancel(this_.id, this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task unsafe_send_strand(SharedStrand Strand, Action handler)
        {
            Generator this_ = self;
            if (this_.Strand == Strand)
            {
                handler();
                return non_async();
            }
            Strand.Post(this_.unsafe_async_callback(handler));
            return this_.async_wait();
        }

        static public Task unsafe_force_send_strand(SharedStrand Strand, Action handler)
        {
            Generator this_ = self;
            Strand.Post(this_.unsafe_async_callback(handler));
            return this_.async_wait();
        }

        private async Task send_strand_(SharedStrand Strand, Action handler)
        {
            System.Exception hasExcep = null;
            Strand.Post(unsafe_async_callback(delegate ()
            {
                try
                {
                    handler();
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
            }));
            await async_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
        }

        static public Task send_strand(SharedStrand Strand, Action handler)
        {
            Generator this_ = self;
            if (this_.Strand == Strand)
            {
                handler();
                return non_async();
            }
            return this_.send_strand_(Strand, handler);
        }

        static public Task force_send_strand(SharedStrand Strand, Action handler)
        {
            Generator this_ = self;
            return this_.send_strand_(Strand, handler);
        }

        static public Task unsafe_send_strand<R>(AsyncResultWrap<R> res, SharedStrand Strand, Func<R> handler)
        {
            Generator this_ = self;
            if (this_.Strand == Strand)
            {
                res.value1 = handler();
                return non_async();
            }
            res.Clear();
            Strand.Post(this_.unsafe_async_callback(delegate ()
            {
                res.value1 = handler();
            }));
            return this_.async_wait();
        }

        static public Task unsafe_force_send_strand<R>(AsyncResultWrap<R> res, SharedStrand Strand, Func<R> handler)
        {
            Generator this_ = self;
            res.Clear();
            Strand.Post(this_.unsafe_async_callback(delegate ()
            {
                res.value1 = handler();
            }));
            return this_.async_wait();
        }

        private async Task<R> send_strand_<R>(SharedStrand Strand, Func<R> handler)
        {
            R res = default(R);
            System.Exception hasExcep = null;
            Strand.Post(unsafe_async_callback(delegate ()
            {
                try
                {
                    res = handler();
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
            }));
            await async_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
            return res;
        }

        static public ValueTask<R> send_strand<R>(SharedStrand Strand, Func<R> handler)
        {
            Generator this_ = self;
            if (this_.Strand == Strand)
            {
                return to_vtask(handler());
            }
            return to_vtask(this_.send_strand_(Strand, handler));
        }

        static public Task<R> force_send_strand<R>(SharedStrand Strand, Func<R> handler)
        {
            Generator this_ = self;
            return this_.send_strand_(Strand, handler);
        }

        static public Func<Task> wrap_send_strand(SharedStrand Strand, Action handler)
        {
            return () => send_strand(Strand, handler);
        }

        static public Func<Task> wrap_force_send_strand(SharedStrand Strand, Action handler)
        {
            return () => force_send_strand(Strand, handler);
        }

        static public Func<T, Task> wrap_send_strand<T>(SharedStrand Strand, Action<T> handler)
        {
            return (T p) => send_strand(Strand, () => handler(p));
        }

        static public Func<T, Task> wrap_force_send_strand<T>(SharedStrand Strand, Action<T> handler)
        {
            return (T p) => force_send_strand(Strand, () => handler(p));
        }

        static public Func<ValueTask<R>> wrap_send_strand<R>(SharedStrand Strand, Func<R> handler)
        {
            return delegate ()
            {
                return send_strand(Strand, handler);
            };
        }

        static public Func<Task<R>> wrap_force_send_strand<R>(SharedStrand Strand, Func<R> handler)
        {
            return delegate ()
            {
                return force_send_strand(Strand, handler);
            };
        }

        static public Func<T, ValueTask<R>> wrap_send_strand<R, T>(SharedStrand Strand, Func<T, R> handler)
        {
            return delegate (T p)
            {
                return send_strand(Strand, () => handler(p));
            };
        }

        static public Func<T, Task<R>> wrap_force_send_strand<R, T>(SharedStrand Strand, Func<T, R> handler)
        {
            return delegate (T p)
            {
                return force_send_strand(Strand, () => handler(p));
            };
        }

        static public Task unsafe_send_service(WorkService Service, Action handler)
        {
            Generator this_ = self;
            Service.Post(this_.unsafe_async_callback(handler));
            return this_.async_wait();
        }

        private async Task send_service_(WorkService Service, Action handler)
        {
            System.Exception hasExcep = null;
            Service.Post(unsafe_async_callback(delegate ()
            {
                try
                {
                    handler();
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
            }));
            await async_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
        }

        static public Task send_service(WorkService Service, Action handler)
        {
            Generator this_ = self;
            return this_.send_service_(Service, handler);
        }

        static public Task unsafe_send_service<R>(AsyncResultWrap<R> res, WorkService Service, Func<R> handler)
        {
            Generator this_ = self;
            res.Clear();
            Service.Post(this_.unsafe_async_callback(delegate ()
            {
                res.value1 = handler();
            }));
            return this_.async_wait();
        }

        private async Task<R> send_service_<R>(WorkService Service, Func<R> handler)
        {
            R res = default(R);
            System.Exception hasExcep = null;
            Service.Post(unsafe_async_callback(delegate ()
            {
                try
                {
                    res = handler();
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
            }));
            await async_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
            return res;
        }

        static public Task<R> send_service<R>(WorkService Service, Func<R> handler)
        {
            Generator this_ = self;
            return this_.send_service_(Service, handler);
        }

        static public Func<Task> wrap_send_service(WorkService Service, Action handler)
        {
            return () => send_service(Service, handler);
        }

        static public Func<T, Task> wrap_send_service<T>(WorkService Service, Action<T> handler)
        {
            return (T p) => send_service(Service, () => handler(p));
        }

        static public Func<Task<R>> wrap_send_service<R>(WorkService Service, Func<R> handler)
        {
            return delegate ()
            {
                return send_service(Service, handler);
            };
        }

        static public Func<T, Task<R>> wrap_send_service<R, T>(WorkService Service, Func<T, R> handler)
        {
            return delegate (T p)
            {
                return send_service(Service, () => handler(p));
            };
        }
#if !NETCORE
        static public void post_control(System.Windows.Forms.Control ctrl, Action handler)
        {
            try
            {
                ctrl.BeginInvoke(handler);
            }
            catch (System.InvalidOperationException ec)
            {
                Trace.Fail(ec.Message, ec.StackTrace);
            }
        }

        static public Task unsafe_send_control(System.Windows.Forms.Control ctrl, Action handler)
        {
            Generator this_ = self;
            post_control(ctrl, this_.unsafe_async_callback(handler));
            return this_.async_wait();
        }

        static public async Task send_control(System.Windows.Forms.Control ctrl, Action handler)
        {
            Generator this_ = self;
            System.Exception hasExcep = null;
            post_control(ctrl, this_.unsafe_async_callback(delegate ()
            {
                try
                {
                    handler();
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
            }));
            await this_.async_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
        }

        static public Task unsafe_send_control<R>(AsyncResultWrap<R> res, System.Windows.Forms.Control ctrl, Func<R> handler)
        {
            res.Clear();
            Generator this_ = self;
            post_control(ctrl, this_.unsafe_async_callback(() => res.value1 = handler()));
            return this_.async_wait();
        }

        static public async Task<R> send_control<R>(System.Windows.Forms.Control ctrl, Func<R> handler)
        {
            Generator this_ = self;
            R res = default(R);
            System.Exception hasExcep = null;
            post_control(ctrl, this_.unsafe_async_callback(delegate ()
            {
                try
                {
                    res = handler();
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
            }));
            await this_.async_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
            return res;
        }

        static public Action wrap_post_control(System.Windows.Forms.Control ctrl, Action handler)
        {
            return () => post_control(ctrl, handler);
        }

        static public Func<Task> wrap_send_control(System.Windows.Forms.Control ctrl, Action handler)
        {
            return () => send_control(ctrl, handler);
        }

        static public Func<T, Task> wrap_send_control<T>(System.Windows.Forms.Control ctrl, Action<T> handler)
        {
            return (T p) => send_control(ctrl, () => handler(p));
        }

        static public Func<Task<R>> wrap_send_control<R>(System.Windows.Forms.Control ctrl, Func<R> handler)
        {
            return async delegate ()
            {
                R res = default(R);
                await send_control(ctrl, () => res = handler());
                return res;
            };
        }

        static public Func<T, Task<R>> wrap_send_control<R, T>(System.Windows.Forms.Control ctrl, Func<T, R> handler)
        {
            return async delegate (T p)
            {
                R res = default(R);
                await send_control(ctrl, () => res = handler(p));
                return res;
            };
        }
#endif
        static public Task unsafe_send_task(Action handler)
        {
            Generator this_ = self;
            this_._pullTask.NewTask();
            bool beginQuit = this_._beginQuit;
            Task _ = Task.Run(delegate ()
            {
                handler();
                this_.Strand.Post(beginQuit ? (Action)this_.QuitNext : this_.NoQuitNext);
            });
            return this_.async_wait();
        }

        static public async Task send_task(Action handler)
        {
            Generator this_ = self;
            System.Exception hasExcep = null;
            this_._pullTask.NewTask();
            bool beginQuit = this_._beginQuit;
            Task _ = Task.Run(delegate ()
            {
                try
                {
                    handler();
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
                this_.Strand.Post(beginQuit ? (Action)this_.QuitNext : this_.NoQuitNext);
            });
            await this_.async_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
        }

        static public Task unsafe_send_task<R>(AsyncResultWrap<R> res, Func<R> handler)
        {
            Generator this_ = self;
            this_._pullTask.NewTask();
            bool beginQuit = this_._beginQuit;
            Task _ = Task.Run(delegate ()
            {
                res.value1 = handler();
                this_.Strand.Post(beginQuit ? (Action)this_.QuitNext : this_.NoQuitNext);
            });
            return this_.async_wait();
        }

        static public async Task<R> send_task<R>(Func<R> handler)
        {
            Generator this_ = self;
            R res = default(R);
            System.Exception hasExcep = null;
            this_._pullTask.NewTask();
            bool beginQuit = this_._beginQuit;
            Task _ = Task.Run(delegate ()
            {
                try
                {
                    res = handler();
                }
                catch (System.Exception ec)
                {
                    ec.Source = string.Format("{0}\n{1}", ec.Source, ec.StackTrace);
                    hasExcep = ec;
                }
                this_.Strand.Post(beginQuit ? (Action)this_.QuitNext : this_.NoQuitNext);
            });
            await this_.async_wait();
            if (null != hasExcep)
            {
                throw hasExcep;
            }
            return res;
        }

        static public Func<Task> wrap_send_task(Action handler)
        {
            return () => send_task(handler);
        }

        static public Func<T, Task> wrap_send_task<T>(Action<T> handler)
        {
            return (T p) => send_task(() => handler(p));
        }

        static public Func<Task<R>> wrap_send_task<R>(Func<R> handler)
        {
            return async delegate ()
            {
                R res = default(R);
                await send_task(() => res = handler());
                return res;
            };
        }

        static public Func<T, Task<R>> wrap_send_task<R, T>(Func<T, R> handler)
        {
            return async delegate (T p)
            {
                R res = default(R);
                await send_task(() => res = handler(p));
                return res;
            };
        }

        static private VoidType check_task(Task task)
        {
            task.GetAwaiter().GetResult();
            return default(VoidType);
        }

        static private R check_task<R>(Task<R> task)
        {
            return task.GetAwaiter().GetResult();
        }

        static private CspInvokeWrap<R> check_csp<R>(DelayCsp<R> task)
        {
            return task.result;
        }

        static private void check_task(AsyncResultWrap<bool, Exception> res, Task task)
        {
            try
            {
                res.value1 = true;
                check_task(task);
            }
            catch (Exception innerEc)
            {
                res.value2 = innerEc;
            }
        }

        static private void check_task<R>(AsyncResultWrap<R, Exception> res, Task<R> task)
        {
            try
            {
                res.value1 = check_task(task);
            }
            catch (Exception innerEc)
            {
                res.value2 = innerEc;
            }
        }

        static private void check_task<R>(AsyncResultWrap<bool, R, Exception> res, Task<R> task)
        {
            try
            {
                res.value1 = true;
                res.value2 = check_task(task);
            }
            catch (Exception innerEc)
            {
                res.value3 = innerEc;
            }
        }

        private async Task<VoidType> wait_task_(Task task)
        {
            await async_wait();
            return check_task(task);
        }

        static public ValueTask<VoidType> wait_task(Task task)
        {
            if (!task.IsCompleted)
            {
                Generator this_ = self;
                task.GetAwaiter().UnsafeOnCompleted(this_.unsafe_async_result());
                return to_vtask(this_.wait_task_(task));
            }
            return to_vtask(check_task(task));
        }

        private async Task<R> wait_task_<R>(Task<R> task)
        {
            await async_wait();
            return check_task(task);
        }

        private async Task<CspInvokeWrap<R>> wait_csp_<R>(DelayCsp<R> task)
        {
            await async_wait();
            return check_csp(task);
        }

        static public ValueTask<R> wait_task<R>(Task<R> task)
        {
            if (!task.IsCompleted)
            {
                Generator this_ = self;
                task.GetAwaiter().UnsafeOnCompleted(this_.unsafe_async_result());
                return to_vtask(this_.wait_task_(task));
            }
            return to_vtask(check_task(task));
        }

        static public ValueTask<CspInvokeWrap<R>> wait_csp<R>(DelayCsp<R> task)
        {
            if (!task.is_completed)
            {
                Generator this_ = self;
                task.on_completed(this_.unsafe_async_result());
                return to_vtask(this_.wait_csp_(task));
            }
            return to_vtask(check_csp(task));
        }

        private async Task<bool> timed_wait_task_(Task task)
        {
            await async_wait();
            if (!_overtime)
            {
                check_task(task);
            }
            return !_overtime;
        }

        static public ValueTask<bool> timed_wait_task(int ms, Task task)
        {
            if (!task.IsCompleted)
            {
                Generator this_ = self;
                task.GetAwaiter().UnsafeOnCompleted(this_.timed_async_result(ms));
                return to_vtask(this_.timed_wait_task_(task));
            }
            check_task(task);
            return to_vtask(true);
        }

        private async Task<TupleEx<bool, R>> timed_wait_task_<R>(Task<R> task)
        {
            await async_wait();
            return TupleEx.Make(!_overtime, _overtime ? default(R) : check_task(task));
        }

        private async Task<TupleEx<bool, CspInvokeWrap<R>>> timed_wait_csp_<R>(DelayCsp<R> task)
        {
            await async_wait();
            return TupleEx.Make(!_overtime, _overtime ? default(CspInvokeWrap<R>) : check_csp(task));
        }

        static public ValueTask<TupleEx<bool, R>> timed_wait_task<R>(int ms, Task<R> task)
        {
            if (!task.IsCompleted)
            {
                Generator this_ = self;
                task.GetAwaiter().UnsafeOnCompleted(this_.timed_async_result(ms));
                return to_vtask(this_.timed_wait_task_(task));
            }
            return to_vtask(TupleEx.Make(true, check_task(task)));
        }

        static public ValueTask<TupleEx<bool, CspInvokeWrap<R>>> timed_wait_csp<R>(int ms, DelayCsp<R> task)
        {
            if (!task.is_completed)
            {
                Generator this_ = self;
                task.on_completed(this_.timed_async_result(ms));
                return to_vtask(this_.timed_wait_csp_(task));
            }
            return to_vtask(TupleEx.Make(true, check_csp(task)));
        }

        static public ValueTask<TupleEx<bool, CspInvokeWrap<R>>> try_wait_csp<R>(DelayCsp<R> task)
        {
            if (!task.is_completed)
            {
                return to_vtask(TupleEx.Make(false, default(CspInvokeWrap<R>)));
            }
            return to_vtask(TupleEx.Make(true, check_csp(task)));
        }

        static public Task unsafe_wait_task<R>(AsyncResultWrap<R, Exception> res, Task<R> task)
        {
            if (!task.IsCompleted)
            {
                Generator this_ = self;
                res.Clear();
                task.GetAwaiter().UnsafeOnCompleted(this_.async_callback(() => check_task(res, task)));
                return this_.async_wait();
            }
            check_task(res, task);
            return non_async();
        }

        static public Task unsafe_timed_wait_task(AsyncResultWrap<bool, Exception> res, int ms, Task task)
        {
            if (!task.IsCompleted)
            {
                Generator this_ = self;
                res.value1 = false;
                task.GetAwaiter().UnsafeOnCompleted(this_.timed_async_callback2(ms, () => check_task(res, task)));
                return this_.async_wait();
            }
            check_task(res, task);
            return non_async();
        }

        static public Task unsafe_timed_wait_task<R>(AsyncResultWrap<bool, R, Exception> res, int ms, Task<R> task)
        {
            if (!task.IsCompleted)
            {
                Generator this_ = self;
                res.value1 = false;
                task.GetAwaiter().UnsafeOnCompleted(this_.timed_async_callback2(ms, () => check_task(res, task)));
                return this_.async_wait();
            }
            check_task(res, task);
            return non_async();
        }

        static public Task stop_other(Generator otherGen)
        {
            Generator this_ = self;
            otherGen.Stop(this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public async Task stop_others(IEnumerable<Generator> otherGens)
        {
            Generator this_ = self;
            UnlimitChannel<VoidType> waitStop = new UnlimitChannel<VoidType>(this_.Strand);
            Action ntf = waitStop.WrapDefault();
            int Count = 0;
            foreach (Generator otherGen in otherGens)
            {
                if (!otherGen.is_completed())
                {
                    Count++;
                    otherGen.Stop(ntf);
                }
            }
            for (int i = 0; i < Count; i++)
            {
                await chan_receive(waitStop);
            }
        }

        static public Task stop_others(params Generator[] otherGens)
        {
            return stop_others((IEnumerable<Generator>)otherGens);
        }

        static public Task wait_other(Generator otherGen)
        {
            Generator this_ = self;
            otherGen.append_stop_callback(this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public async Task wait_others(IEnumerable<Generator> otherGens)
        {
            Generator this_ = self;
            UnlimitChannel<VoidType> waitStop = new UnlimitChannel<VoidType>(this_.Strand);
            Action ntf = waitStop.WrapDefault();
            int Count = 0;
            foreach (Generator otherGen in otherGens)
            {
                if (!otherGen.is_completed())
                {
                    Count++;
                    otherGen.append_stop_callback(ntf);
                }
            }
            for (int i = 0; i < Count; i++)
            {
                await chan_receive(waitStop);
            }
        }

        static public Task wait_others(params Generator[] otherGens)
        {
            return wait_others((IEnumerable<Generator>)otherGens);
        }

        private async Task<bool> timed_wait_other_(NilChannel<NotifyToken> waitRemove, Generator otherGen)
        {
            try
            {
                await async_wait();
            }
            finally
            {
                lock_suspend_and_stop_();
                if (_overtime)
                {
                    NotifyToken cancelToken = await chan_receive(waitRemove);
                    if (null != cancelToken.token)
                    {
                        otherGen.remove_stop_callback(cancelToken);
                    }
                }
                await unlock_suspend_and_stop_();
            }
            return !_overtime;
        }

        static public ValueTask<bool> timed_wait_other(int ms, Generator otherGen)
        {
            Generator this_ = self;
            NilChannel<NotifyToken> waitRemove = new NilChannel<NotifyToken>();
            otherGen.append_stop_callback(this_.timed_async_result(ms), waitRemove.Wrap());
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.timed_wait_other_(waitRemove, otherGen));
            }
            return to_vtask(!this_._overtime);
        }

        static public Task<Generator> wait_others_any(IEnumerable<Generator> otherGens)
        {
            return timed_wait_others_any(-1, otherGens);
        }

        static public Task<Generator> wait_others_any(params Generator[] otherGens)
        {
            return wait_others_any((IEnumerable<Generator>)otherGens);
        }

        static public async Task<Generator> timed_wait_others_any(int ms, IEnumerable<Generator> otherGens)
        {
            Generator this_ = self;
            UnlimitChannel<NotifyToken> waitRemove = new UnlimitChannel<NotifyToken>(this_.Strand);
            UnlimitChannel<Generator> waitStop = new UnlimitChannel<Generator>(this_.Strand);
            Action<Generator> stopHandler = (Generator host) => waitStop.Post(host);
            Action<NotifyToken> removeHandler = (NotifyToken cancelToken) => waitRemove.Post(cancelToken);
            int Count = 0;
            foreach (Generator ele in otherGens)
            {
                Count++;
                if (ele.is_completed())
                {
                    waitStop.Post(ele);
                    waitRemove.Post(new NotifyToken { host = ele });
                    break;
                }
                ele.append_stop_callback(stopHandler, removeHandler);
            }
            try
            {
                if (0 != Count)
                {
                    if (ms < 0)
                    {
                        return await chan_receive(waitStop);
                    }
                    else if (0 == ms)
                    {
                        ChannelReceiveWrap<Generator> gen = await chan_try_receive(waitStop);
                        if (ChannelState.ok == gen.state)
                        {
                            return gen.msg;
                        }
                    }
                    else
                    {
                        ChannelReceiveWrap<Generator> gen = await chan_timed_receive(waitStop, ms);
                        if (ChannelState.ok == gen.state)
                        {
                            return gen.msg;
                        }
                    }
                }
                return null;
            }
            finally
            {
                this_.lock_suspend_and_stop_();
                for (int i = 0; i < Count; i++)
                {
                    NotifyToken node = (await chan_receive(waitRemove)).msg;
                    if (null != node.token)
                    {
                        node.host.remove_stop_callback(node);
                    }
                }
                await this_.unlock_suspend_and_stop_();
            }
        }

        static public Task<Generator> timed_wait_others_any(int ms, params Generator[] otherGens)
        {
            return timed_wait_others_any(ms, (IEnumerable<Generator>)otherGens);
        }

        static public async Task<List<Generator>> timed_wait_others(int ms, IEnumerable<Generator> otherGens)
        {
            Generator this_ = self;
            long endTick = SystemTick.GetTickMs() + ms;
            UnlimitChannel<NotifyToken> waitRemove = new UnlimitChannel<NotifyToken>(this_.Strand);
            UnlimitChannel<Generator> waitStop = new UnlimitChannel<Generator>(this_.Strand);
            Action<Generator> stopHandler = (Generator host) => waitStop.Post(host);
            Action<NotifyToken> removeHandler = (NotifyToken cancelToken) => waitRemove.Post(cancelToken);
            int Count = 0;
            foreach (Generator ele in otherGens)
            {
                Count++;
                if (!ele.is_completed())
                {
                    ele.append_stop_callback(stopHandler, removeHandler);
                }
                else
                {
                    waitStop.Post(ele);
                    waitRemove.Post(new NotifyToken { host = ele });
                }
            }
            try
            {
                List<Generator> completedGens = new List<Generator>(Count);
                if (ms < 0)
                {
                    for (int i = 0; i < Count; i++)
                    {
                        completedGens.Add(await chan_receive(waitStop));
                    }
                }
                else if (0 == ms)
                {
                    for (int i = 0; i < Count; i++)
                    {
                        ChannelReceiveWrap<Generator> gen = await chan_try_receive(waitStop);
                        if (ChannelState.ok != gen.state)
                        {
                            break;
                        }
                        completedGens.Add(gen.msg);
                    }
                }
                else
                {
                    for (int i = 0; i < Count; i++)
                    {
                        long nowTick = SystemTick.GetTickMs();
                        if (nowTick >= endTick)
                        {
                            break;
                        }
                        ChannelReceiveWrap<Generator> gen = await chan_timed_receive(waitStop, (int)(endTick - nowTick));
                        if (ChannelState.ok != gen.state)
                        {
                            break;
                        }
                        completedGens.Add(gen.msg);
                    }
                }
                return completedGens;
            }
            finally
            {
                this_.lock_suspend_and_stop_();
                for (int i = 0; i < Count; i++)
                {
                    NotifyToken node = (await chan_receive(waitRemove)).msg;
                    if (null != node.token)
                    {
                        node.host.remove_stop_callback(node);
                    }
                }
                await this_.unlock_suspend_and_stop_();
            }
        }

        static public Task<List<Generator>> timed_wait_others(int ms, params Generator[] otherGens)
        {
            return timed_wait_others(ms, (IEnumerable<Generator>)otherGens);
        }

        static public Generator completed_others_any(IEnumerable<Generator> otherGens)
        {
            foreach (Generator ele in otherGens)
            {
                if (ele.is_completed())
                {
                    return ele;
                }
            }
            return null;
        }

        static public Generator completed_others_any(params Generator[] otherGens)
        {
            return completed_others_any((IEnumerable<Generator>)otherGens);
        }

        static public List<Generator> completed_others(IEnumerable<Generator> otherGens)
        {
            List<Generator> completedGens = new List<Generator>(32);
            foreach (Generator ele in otherGens)
            {
                if (ele.is_completed())
                {
                    completedGens.Add(ele);
                }
            }
            return completedGens;
        }

        static public List<Generator> completed_others(params Generator[] otherGens)
        {
            List<Generator> completedGens = new List<Generator>(otherGens.Length);
            foreach (Generator ele in otherGens)
            {
                if (ele.is_completed())
                {
                    completedGens.Add(ele);
                }
            }
            return completedGens;
        }

        private async Task wait_group_(WaitGroup wg, WaitGroup.cancel_token cancelToken)
        {
            try
            {
                await async_wait();
            }
            finally
            {
                wg.cancel_wait(cancelToken);
            }
        }

        static public Task WaitGroup(WaitGroup wg)
        {
            Generator this_ = self;
            WaitGroup.cancel_token cancelToken = wg.async_wait(this_.unsafe_async_result());
            if (!this_.new_task_completed())
            {
                return this_.wait_group_(wg, cancelToken);
            }
            return non_async();
        }

        static public Task unsafe_wait_group(WaitGroup wg)
        {
            Generator this_ = self;
            wg.async_wait(this_.unsafe_async_result());
            return this_.async_wait();
        }

        private async Task<bool> timed_wait_group_(WaitGroup wg, WaitGroup.cancel_token cancelToken)
        {
            try
            {
                await async_wait();
                return !_overtime;
            }
            finally
            {
                wg.cancel_wait(cancelToken);
            }
        }

        static public ValueTask<bool> timed_wait_group(int ms, WaitGroup wg)
        {
            Generator this_ = self;
            WaitGroup.cancel_token cancelToken = wg.async_wait(this_.timed_async_result(ms));
            if (!this_.new_task_completed())
            {
                return to_vtask(this_.timed_wait_group_(wg, cancelToken));
            }
            return to_vtask(!this_._overtime);
        }

        static public async Task enter_gate<R>(WaitGate<R> wg)
        {
            Generator this_ = self;
            WaitGate.cancel_token token = wg.async_enter(this_.unsafe_async_result());
            try
            {
                await this_.async_wait();
            }
            finally
            {
                if (wg.cancel_enter(token) && !wg.is_exit)
                {
                    this_.lock_suspend_and_stop_();
                    wg.safe_exit(this_.unsafe_async_result());
                    await this_.async_wait();
                    await this_.unlock_suspend_and_stop_();
                }
            }
        }

        static public async Task<bool> timed_enter_gate<R>(int ms, WaitGate<R> wg)
        {
            Generator this_ = self;
            WaitGate.cancel_token token = wg.async_enter(this_.timed_async_result(ms));
            try
            {
                await this_.async_wait();
                return !this_._overtime;
            }
            finally
            {
                if (wg.cancel_enter(token) && !wg.is_exit)
                {
                    this_.lock_suspend_and_stop_();
                    wg.safe_exit(this_.unsafe_async_result());
                    await this_.async_wait();
                    await this_.unlock_suspend_and_stop_();
                }
            }
        }

        static public Task unsafe_async_call(Action<Action> handler)
        {
            Generator this_ = self;
            handler(this_.unsafe_async_result());
            return this_.async_wait();
        }

        static public Task unsafe_async_call<R>(AsyncResultWrap<R> res, Action<Action<R>> handler)
        {
            Generator this_ = self;
            res.Clear();
            handler(this_.unsafe_async_result(res));
            return this_.async_wait();
        }

        static public Task unsafe_async_call<R1, R2>(AsyncResultWrap<R1, R2> res, Action<Action<R1, R2>> handler)
        {
            Generator this_ = self;
            res.Clear();
            handler(this_.unsafe_async_result(res));
            return this_.async_wait();
        }

        static public Task unsafe_async_call<R1, R2, R3>(AsyncResultWrap<R1, R2, R3> res, Action<Action<R1, R2, R3>> handler)
        {
            Generator this_ = self;
            res.Clear();
            handler(this_.unsafe_async_result(res));
            return this_.async_wait();
        }

        static public Task async_call(Action<Action> handler, Action lostHandler = null)
        {
            Generator this_ = self;
            handler(this_.async_result(lostHandler));
            return this_.async_wait();
        }

        static public ValueTask<R> async_call<R>(Action<Action<R>> handler, Action<R> lostHandler = null)
        {
            Generator this_ = self;
            AsyncResultWrap<R> res = new AsyncResultWrap<R>();
            handler(this_.async_result(res, lostHandler));
            return this_.async_wait(res);
        }

        static public ValueTask<TupleEx<R1, R2>> async_call<R1, R2>(Action<Action<R1, R2>> handler, Action<R1, R2> lostHandler = null)
        {
            Generator this_ = self;
            AsyncResultWrap<R1, R2> res = new AsyncResultWrap<R1, R2>();
            handler(this_.async_result(res, lostHandler));
            return this_.async_wait(res);
        }

        static public ValueTask<TupleEx<R1, R2, R3>> async_call<R1, R2, R3>(Action<Action<R1, R2, R3>> handler, Action<R1, R2, R3> lostHandler = null)
        {
            Generator this_ = self;
            AsyncResultWrap<R1, R2, R3> res = new AsyncResultWrap<R1, R2, R3>();
            handler(this_.async_result(res, lostHandler));
            return this_.async_wait(res);
        }

        static public Task unsafe_timed_async_call(AsyncResultWrap<bool> res, int ms, Action<Action> handler, Action timedHandler = null)
        {
            Generator this_ = self;
            res.value1 = false;
            handler(this_.timed_async_callback(ms, () => res.value1 = !this_._overtime, timedHandler));
            return this_.async_wait();
        }

        static public ValueTask<bool> timed_async_call(int ms, Action<Action> handler, Action timedHandler = null)
        {
            Generator this_ = self;
            handler(this_.timed_async_callback(ms, NilAction.action, timedHandler));
            return this_.timed_async_wait();
        }

        static public Task unsafe_timed_async_call<R>(AsyncResultWrap<bool, R> res, int ms, Action<Action<R>> handler, Action timedHandler = null)
        {
            Generator this_ = self;
            res.value1 = false;
            handler(this_.timed_async_callback(ms, delegate (R res1)
            {
                res.value1 = !this_._overtime;
                res.value2 = res1;
            }, timedHandler));
            return this_.async_wait();
        }

        static public ValueTask<TupleEx<bool, R>> timed_async_call<R>(int ms, Action<Action<R>> handler, Action timedHandler = null)
        {
            Generator this_ = self;
            AsyncResultWrap<R> res = new AsyncResultWrap<R>();
            handler(this_.timed_async_result(ms, res, timedHandler));
            return this_.timed_async_wait(res);
        }

        static public Task unsafe_timed_async_call<R1, R2>(AsyncResultWrap<bool, TupleEx<R1, R2>> res, int ms, Action<Action<R1, R2>> handler, Action timedHandler = null)
        {
            Generator this_ = self;
            res.value1 = false;
            handler(this_.timed_async_callback(ms, delegate (R1 res1, R2 res2)
            {
                res.value1 = !this_._overtime;
                res.value2 = TupleEx.Make(res1, res2);
            }, timedHandler));
            return this_.async_wait();
        }

        static public ValueTask<TupleEx<bool, TupleEx<R1, R2>>> timed_async_call<R1, R2>(int ms, Action<Action<R1, R2>> handler, Action timedHandler = null)
        {
            Generator this_ = self;
            AsyncResultWrap<R1, R2> res = new AsyncResultWrap<R1, R2>();
            handler(this_.timed_async_result(ms, res, timedHandler));
            return this_.timed_async_wait(res);
        }

        static public Task unsafe_timed_async_call<R1, R2, R3>(AsyncResultWrap<bool, TupleEx<R1, R2, R3>> res, int ms, Action<Action<R1, R2, R3>> handler, Action timedHandler = null)
        {
            Generator this_ = self;
            res.value1 = false;
            handler(this_.timed_async_callback(ms, delegate (R1 res1, R2 res2, R3 res3)
            {
                res.value1 = !this_._overtime;
                res.value2 = TupleEx.Make(res1, res2, res3);
            }, timedHandler));
            return this_.async_wait();
        }

        static public ValueTask<TupleEx<bool, TupleEx<R1, R2, R3>>> timed_async_call<R1, R2, R3>(int ms, Action<Action<R1, R2, R3>> handler, Action timedHandler = null)
        {
            Generator this_ = self;
            AsyncResultWrap<R1, R2, R3> res = new AsyncResultWrap<R1, R2, R3>();
            handler(this_.timed_async_result(ms, res, timedHandler));
            return this_.timed_async_wait(res);
        }
#if CHECK_STEP_TIMEOUT
        static public async Task call(action handler)
        {
            Generator this_ = self;
            UpStackFrame(this_._makeStack, 2);
            try
            {
                await handler();
            }
            catch (System.Exception)
            {
                this_._makeStack.RemoveFirst();
                throw;
            }
        }

        static public async Task<R> call<R>(Func<Task<R>> handler)
        {
            Generator this_ = self;
            UpStackFrame(this_._makeStack, 2);
            try
            {
                return await handler();
            }
            catch (System.Exception)
            {
                this_._makeStack.RemoveFirst();
                throw;
            }
        }

        static public async Task depth_call(SharedStrand Strand, action handler)
        {
            Generator this_ = self;
            UpStackFrame(this_._makeStack, 2);
            Generator depthGen = (new Generator()).Init(Strand, handler, this_.unsafe_async_result(), null, this_._makeStack);
            try
            {
                depthGen.Run();
                await this_.async_wait();
            }
            catch (StopException)
            {
                depthGen.Stop(this_.unsafe_async_result());
                await this_.async_wait();
                throw;
            }
            finally
            {
                this_._makeStack.RemoveFirst();
            }
        }

        static public async Task<R> depth_call<R>(SharedStrand Strand, Func<Task<R>> handler)
        {
            Generator this_ = self;
            R res = default(R);
            UpStackFrame(this_._makeStack, 2);
            Generator depthGen = (new Generator()).Init(Strand, async () => res = await handler(), this_.unsafe_async_result(), null, this_._makeStack);
            try
            {
                depthGen.Run();
                await this_.async_wait();
            }
            catch (StopException)
            {
                depthGen.Stop(this_.unsafe_async_result());
                await this_.async_wait();
                throw;
            }
            finally
            {
                this_._makeStack.RemoveFirst();
            }
            return res;
        }
#else
        static public Task call(action handler)
        {
            return handler();
        }

        static public Task<R> call<R>(Func<Task<R>> handler)
        {
            return handler();
        }

        static public async Task depth_call(SharedStrand Strand, action handler)
        {
            Generator this_ = self;
            Generator depthGen = Make(Strand, handler, this_.unsafe_async_result());
            try
            {
                depthGen.Run();
                await this_.async_wait();
            }
            catch (StopException)
            {
                depthGen.Stop(this_.unsafe_async_result());
                await this_.async_wait();
                throw;
            }
        }

        static public async Task<R> depth_call<R>(SharedStrand Strand, Func<Task<R>> handler)
        {
            Generator this_ = self;
            R res = default(R);
            Generator depthGen = Make(Strand, async () => res = await handler(), this_.unsafe_async_result());
            try
            {
                depthGen.Run();
                await this_.async_wait();
            }
            catch (StopException)
            {
                depthGen.Stop(this_.unsafe_async_result());
                await this_.async_wait();
                throw;
            }
            return res;
        }
#endif

#if CHECK_STEP_TIMEOUT
        static private LinkedList<CallStackInfo[]> debug_stack
        {
            get
            {
                return self._makeStack;
            }
        }
#endif

        static long calc_hash<T>(int id)
        {
            return (long)id << 32 | (uint)TypeHash<T>.code;
        }

        static public Channel<T> self_mailbox<T>(int id = 0)
        {
            Generator this_ = self;
            if (null == this_._mailboxMap)
            {
                this_._mailboxMap = new Dictionary<long, MailPick>();
            }
            MailPick mb = null;
            if (!this_._mailboxMap.TryGetValue(calc_hash<T>(id), out mb))
            {
                mb = new MailPick(new UnlimitChannel<T>(this_.Strand));
                this_._mailboxMap.Add(calc_hash<T>(id), mb);
            }
            return (Channel<T>)mb.mailbox;
        }

        public ValueTask<Channel<T>> get_mailbox<T>(int id = 0)
        {
            return send_strand(Strand, delegate ()
            {
                if (-1 == _lockSuspendCount)
                {
                    return null;
                }
                if (null == _mailboxMap)
                {
                    _mailboxMap = new Dictionary<long, MailPick>();
                }
                MailPick mb = null;
                if (!_mailboxMap.TryGetValue(calc_hash<T>(id), out mb))
                {
                    mb = new MailPick(new UnlimitChannel<T>(Strand));
                    _mailboxMap.Add(calc_hash<T>(id), mb);
                }
                return (Channel<T>)mb.mailbox;
            });
        }

        static public async Task<bool> agent_mail<T>(Generator agentGen, int id = 0)
        {
            Generator this_ = self;
            if (null == this_._agentMng)
            {
                this_._agentMng = new children();
            }
            if (null == this_._mailboxMap)
            {
                this_._mailboxMap = new Dictionary<long, MailPick>();
            }
            MailPick mb = null;
            if (!this_._mailboxMap.TryGetValue(calc_hash<T>(id), out mb))
            {
                mb = new MailPick(new UnlimitChannel<T>(this_.Strand));
                this_._mailboxMap.Add(calc_hash<T>(id), mb);
            }
            else if (null != mb.agentAction)
            {
                await this_._agentMng.Stop(mb.agentAction);
                mb.agentAction = null;
            }
            Channel<T> agentMb = await agentGen.get_mailbox<T>();
            if (null == agentMb)
            {
                return false;
            }
            mb.agentAction = this_._agentMng.Make(async delegate ()
            {
                Channel<T> selfMb = (Channel<T>)mb.mailbox;
                ChannelNotifySign ntfSign = new ChannelNotifySign();
                Generator self = Generator.self;
                try
                {
                    NilChannel<ChannelState> waitHasChan = new NilChannel<ChannelState>();
                    Action<ChannelState> waitHasNtf = waitHasChan.Wrap();
                    AsyncResultWrap<ChannelReceiveWrap<T>> recvRes = new AsyncResultWrap<ChannelReceiveWrap<T>>();
                    selfMb.AsyncAppendRecvNotify(waitHasNtf, ntfSign);
                    while (true)
                    {
                        await chan_receive(waitHasChan);
                        try
                        {
                            self.lock_suspend_and_stop_();
                            recvRes.value1 = ChannelReceiveWrap<T>.def;
                            selfMb.AsyncTryRecvAndAppendNotify(self.unsafe_async_result(recvRes), waitHasNtf, ntfSign);
                            await self.async_wait();
                            if (ChannelState.ok == recvRes.value1.state)
                            {
                                recvRes.value1 = new ChannelReceiveWrap<T> { state = await chan_send(agentMb, recvRes.value1.msg) };
                            }
                            if (ChannelState.closed == recvRes.value1.state)
                            {
                                break;
                            }
                        }
                        finally
                        {
                            await self.unlock_suspend_and_stop_();
                        }
                    }
                }
                finally
                {
                    self.lock_suspend_and_stop_();
                    selfMb.AsyncRemoveRecvNotify(self.unsafe_async_ignore<ChannelState>(), ntfSign);
                    await self.async_wait();
                    await self.unlock_suspend_and_stop_();
                }
            });
            mb.agentAction.Run();
            return true;
        }

        static public async Task<bool> cancel_agent<T>(int id = 0)
        {
            Generator this_ = self;
            MailPick mb = null;
            if (null != this_._agentMng && null != this_._mailboxMap &&
                this_._mailboxMap.TryGetValue(calc_hash<T>(id), out mb) && null != mb.agentAction)
            {
                await this_._agentMng.Stop(mb.agentAction);
                mb.agentAction = null;
                return true;
            }
            return false;
        }

        static public ValueTask<ChannelReceiveWrap<T>> recv_msg<T>(int id = 0, ChannelLostMsg<T> lostMsg = null)
        {
            return chan_receive(self_mailbox<T>(id), lostMsg);
        }

        static public ValueTask<ChannelReceiveWrap<T>> try_recv_msg<T>(int id = 0, ChannelLostMsg<T> lostMsg = null)
        {
            return chan_try_receive(self_mailbox<T>(id), lostMsg);
        }

        static public ValueTask<ChannelReceiveWrap<T>> timed_recv_msg<T>(int ms, int id = 0, ChannelLostMsg<T> lostMsg = null)
        {
            return chan_timed_receive(self_mailbox<T>(id), ms, lostMsg);
        }

        private async Task<ChannelSendWrap> send_msg_<T>(ValueTask<Channel<T>> mbTask, T msg, ChannelLostMsg<T> lostMsg)
        {
            Channel<T> mb = await mbTask;
            return null != mb ? await chan_send(mb, msg, lostMsg) : new ChannelSendWrap { state = ChannelState.Fail };
        }

        public ValueTask<ChannelSendWrap> send_msg<T>(int id, T msg, ChannelLostMsg<T> lostMsg = null)
        {
            ValueTask<Channel<T>> mbTask = get_mailbox<T>(id);
            if (!mbTask.IsCompleted)
            {
                return to_vtask(send_msg_(mbTask, msg, lostMsg));
            }
            Channel<T> mb = mbTask.GetAwaiter().GetResult();
            return null != mb ? chan_send(mb, msg, lostMsg) : to_vtask(new ChannelSendWrap { state = ChannelState.Fail });
        }

        public ValueTask<ChannelSendWrap> send_msg<T>(T msg, ChannelLostMsg<T> lostMsg = null)
        {
            return send_msg(0, msg, lostMsg);
        }

        public ValueTask<ChannelSendWrap> send_void_msg(int id, ChannelLostMsg<VoidType> lostMsg = null)
        {
            return send_msg(id, default(VoidType), lostMsg);
        }

        public ValueTask<ChannelSendWrap> send_void_msg(ChannelLostMsg<VoidType> lostMsg = null)
        {
            return send_msg(0, default(VoidType), lostMsg);
        }

        static private async Task<VoidType> wait_void_task(Task task)
        {
            await task;
            return default(VoidType);
        }

        static private ValueTask<VoidType> check_void_task(Task task)
        {
            if (!task.IsCompleted)
            {
                return to_vtask(wait_void_task(task));
            }
            return to_vtask(check_task(task));
        }

        public class receive_mail
        {
            bool _run = true;
            GoSharedMutex _mutex;
            children _children = new children();

            internal receive_mail(bool forceStopAll)
            {
                Generator self = Generator.self;
                if (null == self._mailboxMap)
                {
                    self._mailboxMap = new Dictionary<long, MailPick>();
                }
                if (null == self._genLocal)
                {
                    self._genLocal = new Dictionary<long, LocalWrap>();
                }
                _mutex = forceStopAll ? null : new GoSharedMutex(self.Strand);
            }

            public receive_mail case_of(Channel<VoidType> Channel, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return case_of(Channel, (VoidType _) => handler(), errHandler, lostMsg);
            }

            public receive_mail case_of<T>(Channel<T> Channel, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                _children.Go(async delegate ()
                {
                    Generator self = Generator.self;
                    if (null == _mutex)
                    {
                        try
                        {
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            while (_run)
                            {
                                ChannelReceiveWrap<T> recvRes = await chan_receive(Channel, lostMsg);
                                if (ChannelState.ok == recvRes.state)
                                {
                                    await handler(recvRes.msg);
                                }
                                else if (null != errHandler && await errHandler(recvRes.state))
                                {
                                    break;
                                }
                                else if (ChannelState.closed == recvRes.state)
                                {
                                    break;
                                }
                            }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self._mailboxMap = null;
                            self._genLocal = null;
                        }
                    }
                    else
                    {
                        ChannelNotifySign ntfSign = new ChannelNotifySign();
                        try
                        {
                            self.lock_suspend_();
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            NilChannel<ChannelState> waitHasChan = new NilChannel<ChannelState>();
                            Action<ChannelState> waitHasNtf = waitHasChan.Wrap();
                            AsyncResultWrap<ChannelReceiveWrap<T>> recvRes = new AsyncResultWrap<ChannelReceiveWrap<T>>();
                            Channel.AsyncAppendRecvNotify(waitHasNtf, ntfSign);
                            while (_run)
                            {
                                await chan_receive(waitHasChan);
                                await mutex_lock_shared(_mutex);
                                try
                                {
                                    try
                                    {
                                        recvRes.value1 = ChannelReceiveWrap<T>.def;
                                        Channel.AsyncTryRecvAndAppendNotify(self.unsafe_async_result(recvRes), waitHasNtf, ntfSign);
                                        await self.async_wait();
                                    }
                                    catch (StopException)
                                    {
                                        Channel.AsyncRemoveRecvNotify(self.unsafe_async_ignore<ChannelState>(), ntfSign);
                                        await self.async_wait();
                                        if (ChannelState.ok == recvRes.value1.state)
                                        {
                                            lostMsg?.set(recvRes.value1.msg);
                                        }
                                        throw;
                                    }
                                    try
                                    {
                                        await self.unlock_suspend_();
                                        if (ChannelState.ok == recvRes.value1.state)
                                        {
                                            await handler(recvRes.value1.msg);
                                        }
                                        else if (null != errHandler && await errHandler(recvRes.value1.state))
                                        {
                                            break;
                                        }
                                        else if (ChannelState.closed == recvRes.value1.state)
                                        {
                                            break;
                                        }
                                    }
                                    finally
                                    {
                                        self.lock_suspend_();
                                    }
                                }
                                finally
                                {
                                    await mutex_unlock_shared(_mutex);
                                }
                            }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self.lock_stop_();
                            self._mailboxMap = null;
                            self._genLocal = null;
                            Channel.AsyncRemoveRecvNotify(self.unsafe_async_ignore<ChannelState>(), ntfSign);
                            await self.async_wait();
                            await self.unlock_suspend_and_stop_();
                        }
                    }
                });
                return this;
            }

            public receive_mail case_of<T1, T2>(Channel<TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return case_of(Channel, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg);
            }

            public receive_mail case_of<T1, T2, T3>(Channel<TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return case_of(Channel, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg);
            }

            public receive_mail timed_case_of(Channel<VoidType> Channel, int ms, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (VoidType _) => handler(), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T>(Channel<T> Channel, int ms, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                _children.Go(async delegate ()
                {
                    Generator self = Generator.self;
                    if (null == _mutex)
                    {
                        ChannelReceiveWrap<T> recvRes = await chan_timed_receive(Channel, ms, lostMsg);
                        try
                        {
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            if (ChannelState.ok == recvRes.state)
                            {
                                await handler(recvRes.msg);
                            }
                            else if (null != errHandler && await errHandler(recvRes.state)) { }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self._mailboxMap = null;
                            self._genLocal = null;
                        }
                    }
                    else
                    {
                        ChannelNotifySign ntfSign = new ChannelNotifySign();
                        try
                        {
                            self.lock_suspend_();
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            long endTick = SystemTick.GetTickMs() + ms;
                            while (_run)
                            {
                                ChannelState result = ChannelState.undefined;
                                Channel.AsyncAppendRecvNotify(self.unsafe_async_callback((ChannelState state) => result = state), ntfSign, ms);
                                await self.async_wait();
                                await mutex_lock_shared(_mutex);
                                try
                                {
                                    if (ChannelState.overtime != result)
                                    {
                                        ChannelReceiveWrap<T> recvRes = await chan_try_receive(Channel, lostMsg);
                                        try
                                        {
                                            await self.unlock_suspend_();
                                            if (ChannelState.ok == recvRes.state)
                                            {
                                                await handler(recvRes.msg); break;
                                            }
                                            else if ((null != errHandler && await errHandler(recvRes.state)) || ChannelState.closed == recvRes.state) { break; }
                                            if (0 <= ms && 0 >= (ms = (int)(endTick - SystemTick.GetTickMs())))
                                            {
                                                ms = -1;
                                                if (null == errHandler || await errHandler(ChannelState.overtime))
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            self.lock_suspend_();
                                        }
                                    }
                                    else
                                    {
                                        ms = -1;
                                        if (null == errHandler || await errHandler(ChannelState.overtime))
                                        {
                                            break;
                                        }
                                    }
                                }
                                finally
                                {
                                    await mutex_unlock_shared(_mutex);
                                }
                            }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self.lock_stop_();
                            self._mailboxMap = null;
                            self._genLocal = null;
                            Channel.AsyncRemoveRecvNotify(self.unsafe_async_ignore<ChannelState>(), ntfSign);
                            await self.async_wait();
                            await self.unlock_suspend_and_stop_();
                        }
                    }
                });
                return this;
            }

            public receive_mail timed_case_of<T1, T2>(Channel<TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T1, T2, T3>(Channel<TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg);
            }

            public receive_mail try_case_of(Channel<VoidType> Channel, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return try_case_of(Channel, (VoidType _) => handler(), errHandler, lostMsg);
            }

            public receive_mail try_case_of<T>(Channel<T> Channel, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                _children.Go(async delegate ()
                {
                    Generator self = Generator.self;
                    if (null != _mutex)
                    {
                        await mutex_lock_shared(_mutex);
                    }
                    try
                    {
                        self._mailboxMap = _children.parent._mailboxMap;
                        self._genLocal = _children.parent._genLocal;
                        ChannelReceiveWrap<T> recvRes = await chan_try_receive(Channel, lostMsg);
                        if (ChannelState.ok == recvRes.state)
                        {
                            await handler(recvRes.msg);
                        }
                        else if (null != errHandler && await errHandler(recvRes.state)) { }
                    }
                    catch (MessageStopCurrentException) { }
                    catch (MessageStopAllException)
                    {
                        _run = false;
                    }
                    finally
                    {
                        self._mailboxMap = null;
                        self._genLocal = null;
                        if (null != _mutex)
                        {
                            await mutex_unlock_shared(_mutex);
                        }
                    }
                });
                return this;
            }

            public receive_mail try_case_of<T1, T2>(Channel<TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return try_case_of(Channel, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg);
            }

            public receive_mail try_case_of<T1, T2, T3>(Channel<TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return try_case_of(Channel, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg);
            }

            public receive_mail case_of(BroadcastChannel<VoidType> Channel, Func<Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return case_of(Channel, (VoidType _) => handler(), token, errHandler, lostMsg);
            }

            public receive_mail case_of<T1, T2>(BroadcastChannel<TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return case_of(Channel, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), token, errHandler, lostMsg);
            }

            public receive_mail case_of<T1, T2, T3>(BroadcastChannel<TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return case_of(Channel, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), token, errHandler, lostMsg);
            }

            public receive_mail case_of<T>(BroadcastChannel<T> Channel, Func<T, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                _children.Go(async delegate ()
                {
                    Generator self = Generator.self;
                    token = null != token ? token : new BroadcastToken();
                    if (null == _mutex)
                    {
                        try
                        {
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            while (_run)
                            {
                                ChannelReceiveWrap<T> recvRes = await chan_receive(Channel, token, lostMsg);
                                if (ChannelState.ok == recvRes.state)
                                {
                                    await handler(recvRes.msg);
                                }
                                else if (null != errHandler && await errHandler(recvRes.state))
                                {
                                    break;
                                }
                            }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self._mailboxMap = null;
                            self._genLocal = null;
                        }
                    }
                    else
                    {
                        ChannelNotifySign ntfSign = new ChannelNotifySign();
                        try
                        {
                            self.lock_suspend_();
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            NilChannel<ChannelState> waitHasChan = new NilChannel<ChannelState>();
                            Action<ChannelState> waitHasNtf = waitHasChan.Wrap();
                            AsyncResultWrap<ChannelReceiveWrap<T>> recvRes = new AsyncResultWrap<ChannelReceiveWrap<T>>();
                            Channel.AsyncAppendRecvNotify(waitHasNtf, ntfSign);
                            while (_run)
                            {
                                await chan_receive(waitHasChan);
                                await mutex_lock_shared(_mutex);
                                try
                                {
                                    try
                                    {
                                        recvRes.value1 = ChannelReceiveWrap<T>.def;
                                        Channel.AsyncTryRecvAndAppendNotify(self.unsafe_async_result(recvRes), waitHasNtf, ntfSign);
                                        await self.async_wait();
                                    }
                                    catch (StopException)
                                    {
                                        Channel.AsyncRemoveRecvNotify(self.unsafe_async_ignore<ChannelState>(), ntfSign);
                                        await self.async_wait();
                                        if (ChannelState.ok == recvRes.value1.state)
                                        {
                                            lostMsg?.set(recvRes.value1.msg);
                                        }
                                        throw;
                                    }
                                    try
                                    {
                                        await self.unlock_suspend_();
                                        if (ChannelState.ok == recvRes.value1.state)
                                        {
                                            await handler(recvRes.value1.msg);
                                        }
                                        else if (null != errHandler && await errHandler(recvRes.value1.state))
                                        {
                                            break;
                                        }
                                        else if (ChannelState.closed == recvRes.value1.state)
                                        {
                                            break;
                                        }
                                    }
                                    finally
                                    {
                                        self.lock_suspend_();
                                    }
                                }
                                finally
                                {
                                    await mutex_unlock_shared(_mutex);
                                }
                            }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self.lock_stop_();
                            self._mailboxMap = null;
                            self._genLocal = null;
                            Channel.AsyncRemoveRecvNotify(self.unsafe_async_ignore<ChannelState>(), ntfSign);
                            await self.async_wait();
                            await self.unlock_suspend_and_stop_();
                        }
                    }
                });
                return this;
            }

            public receive_mail timed_case_of(BroadcastChannel<VoidType> Channel, int ms, Func<Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (VoidType _) => handler(), token, errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T1, T2>(BroadcastChannel<TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), token, errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T1, T2, T3>(BroadcastChannel<TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), token, errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T>(BroadcastChannel<T> Channel, int ms, Func<T, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                _children.Go(async delegate ()
                {
                    Generator self = Generator.self;
                    token = null != token ? token : new BroadcastToken();
                    if (null == _mutex)
                    {
                        ChannelReceiveWrap<T> recvRes = await chan_timed_receive(Channel, ms, token, lostMsg);
                        try
                        {
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            if (ChannelState.ok == recvRes.state)
                            {
                                await handler(recvRes.msg);
                            }
                            else if (null != errHandler && await errHandler(recvRes.state)) { }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self._mailboxMap = null;
                            self._genLocal = null;
                        }
                    }
                    else
                    {
                        ChannelNotifySign ntfSign = new ChannelNotifySign();
                        try
                        {
                            self.lock_suspend_();
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            long endTick = SystemTick.GetTickMs() + ms;
                            while (_run)
                            {
                                ChannelState result = ChannelState.undefined;
                                Channel.AsyncAppendRecvNotify(self.unsafe_async_callback((ChannelState state) => result = state), ntfSign, ms);
                                await self.async_wait();
                                await mutex_lock_shared(_mutex);
                                try
                                {
                                    if (ChannelState.overtime != result)
                                    {
                                        ChannelReceiveWrap<T> recvRes = await chan_try_receive(Channel, lostMsg);
                                        try
                                        {
                                            await self.unlock_suspend_();
                                            if (ChannelState.ok == recvRes.state)
                                            {
                                                await handler(recvRes.msg); break;
                                            }
                                            else if ((null != errHandler && await errHandler(recvRes.state)) || ChannelState.closed == recvRes.state) { break; }
                                            if (0 <= ms && 0 >= (ms = (int)(endTick - SystemTick.GetTickMs())))
                                            {
                                                ms = -1;
                                                if (null == errHandler || await errHandler(ChannelState.overtime))
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            self.lock_suspend_();
                                        }
                                    }
                                    else
                                    {
                                        ms = -1;
                                        if (null == errHandler || await errHandler(ChannelState.overtime))
                                        {
                                            break;
                                        }
                                    }
                                }
                                finally
                                {
                                    await mutex_unlock_shared(_mutex);
                                }
                            }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self.lock_stop_();
                            self._mailboxMap = null;
                            self._genLocal = null;
                            Channel.AsyncRemoveRecvNotify(self.unsafe_async_ignore<ChannelState>(), ntfSign);
                            await self.async_wait();
                            await self.unlock_suspend_and_stop_();
                        }
                    }
                });
                return this;
            }

            public receive_mail try_case_of(BroadcastChannel<VoidType> Channel, Func<Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return try_case_of(Channel, (VoidType _) => handler(), token, errHandler, lostMsg);
            }

            public receive_mail try_case_of<T1, T2>(BroadcastChannel<TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return try_case_of(Channel, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), token, errHandler, lostMsg);
            }

            public receive_mail try_case_of<T1, T2, T3>(BroadcastChannel<TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return try_case_of(Channel, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), token, errHandler, lostMsg);
            }

            public receive_mail try_case_of<T>(BroadcastChannel<T> Channel, Func<T, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                _children.Go(async delegate ()
                {
                    Generator self = Generator.self;
                    if (null != _mutex)
                    {
                        await mutex_lock_shared(_mutex);
                    }
                    try
                    {
                        self._mailboxMap = _children.parent._mailboxMap;
                        self._genLocal = _children.parent._genLocal;
                        ChannelReceiveWrap<T> recvRes = await chan_try_receive(Channel, null != token ? token : new BroadcastToken(), lostMsg);
                        if (ChannelState.ok == recvRes.state)
                        {
                            await handler(recvRes.msg);
                        }
                        else if (null != errHandler && await errHandler(recvRes.state)) { }
                    }
                    catch (MessageStopCurrentException) { }
                    catch (MessageStopAllException)
                    {
                        _run = false;
                    }
                    finally
                    {
                        self._mailboxMap = null;
                        self._genLocal = null;
                        if (null != _mutex)
                        {
                            await mutex_unlock_shared(_mutex);
                        }
                    }
                });
                return this;
            }

            public receive_mail case_of<T>(CspChannel<VoidType, T> Channel, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, T>> lostMsg = null)
            {
                return case_of(Channel, (T msg) => check_void_task(handler(msg)), errHandler, lostMsg);
            }

            public receive_mail case_of<T1, T2>(CspChannel<VoidType, TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2>>> lostMsg = null)
            {
                return case_of(Channel, (TupleEx<T1, T2> msg) => check_void_task(handler(msg.value1, msg.value2)), errHandler, lostMsg);
            }

            public receive_mail case_of<T1, T2, T3>(CspChannel<VoidType, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                return case_of(Channel, (TupleEx<T1, T2, T3> msg) => check_void_task(handler(msg.value1, msg.value2, msg.value3)), errHandler, lostMsg);
            }

            public receive_mail case_of(CspChannel<VoidType, VoidType> Channel, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, VoidType>> lostMsg = null)
            {
                return case_of(Channel, (VoidType _) => check_void_task(handler()), errHandler, lostMsg);
            }

            public receive_mail case_of<R>(CspChannel<R, VoidType> Channel, Func<Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
            {
                return case_of(Channel, (VoidType _) => handler(), errHandler, lostMsg);
            }

            public receive_mail case_of<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
            {
                return case_of(Channel, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg);
            }

            public receive_mail case_of<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                return case_of(Channel, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg);
            }

            public receive_mail case_of<R, T>(CspChannel<R, T> Channel, Func<T, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                return case_of(Channel, handler, null, errHandler, lostMsg);
            }

            public receive_mail case_of<R>(CspChannel<R, VoidType> Channel, Func<ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
            {
                return case_of(Channel, (VoidType _) => handler(), errHandler, lostMsg);
            }

            public receive_mail case_of<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
            {
                return case_of(Channel, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg);
            }

            public receive_mail case_of<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                return case_of(Channel, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg);
            }

            public receive_mail case_of<R, T>(CspChannel<R, T> Channel, Func<T, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                return case_of(Channel, null, handler, errHandler, lostMsg);
            }

            private receive_mail case_of<R, T>(CspChannel<R, T> Channel, Func<T, Task<R>> handler, Func<T, ValueTask<R>> gohandler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
            {
                _children.Go(async delegate ()
                {
                    Generator self = Generator.self;
                    if (null == _mutex)
                    {
                        try
                        {
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            while (_run)
                            {
                                CspWaitWrap<R, T> recvRes = await csp_wait(Channel, lostMsg);
                                if (ChannelState.ok == recvRes.state)
                                {
                                    try
                                    {
                                        recvRes.Complete(null != handler ? await handler(recvRes.msg) : await gohandler(recvRes.msg));
                                    }
                                    catch (CspFailException)
                                    {
                                        recvRes.Fail();
                                    }
                                    catch (StopException)
                                    {
                                        recvRes.Fail();
                                        throw;
                                    }
                                    catch (System.Exception)
                                    {
                                        recvRes.Fail();
                                        throw;
                                    }
                                }
                                else if (null != errHandler && await errHandler(recvRes.state))
                                {
                                    break;
                                }
                                else if (ChannelState.closed == recvRes.state)
                                {
                                    break;
                                }
                            }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self._mailboxMap = null;
                            self._genLocal = null;
                        }
                    }
                    else
                    {
                        ChannelNotifySign ntfSign = new ChannelNotifySign();
                        try
                        {
                            self.lock_suspend_();
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            NilChannel<ChannelState> waitHasChan = new NilChannel<ChannelState>();
                            Action<ChannelState> waitHasNtf = waitHasChan.Wrap();
                            AsyncResultWrap<CspWaitWrap<R, T>> recvRes = new AsyncResultWrap<CspWaitWrap<R, T>>();
                            Channel.AsyncAppendRecvNotify(waitHasNtf, ntfSign);
                            while (_run)
                            {
                                await chan_receive(waitHasChan);
                                await mutex_lock_shared(_mutex);
                                try
                                {
                                    recvRes.value1 = CspWaitWrap<R, T>.def;
                                    try
                                    {
                                        Channel.AsyncTryRecvAndAppendNotify(self.unsafe_async_result(recvRes), waitHasNtf, ntfSign);
                                        await self.async_wait();
                                    }
                                    catch (StopException)
                                    {
                                        Channel.AsyncRemoveRecvNotify(self.unsafe_async_ignore<ChannelState>(), ntfSign);
                                        await self.async_wait();
                                        if (ChannelState.ok == recvRes.value1.state)
                                        {
                                            if (null != lostMsg)
                                            {
                                                lostMsg.set(recvRes.value1);
                                            }
                                            else
                                            {
                                                recvRes.value1.Fail();
                                            }
                                        }
                                        throw;
                                    }
                                    try
                                    {
                                        recvRes.value1.result?.StartInvokeTimer(self);
                                        await self.unlock_suspend_();
                                        if (ChannelState.ok == recvRes.value1.state)
                                        {
                                            try
                                            {
                                                recvRes.value1.Complete(null != handler ? await handler(recvRes.value1.msg) : await gohandler(recvRes.value1.msg));
                                            }
                                            catch (CspFailException)
                                            {
                                                recvRes.value1.Fail();
                                            }
                                            catch (StopException)
                                            {
                                                recvRes.value1.Fail();
                                                throw;
                                            }
                                            catch (System.Exception)
                                            {
                                                recvRes.value1.Fail();
                                                throw;
                                            }
                                        }
                                        else if (null != errHandler && await errHandler(recvRes.value1.state))
                                        {
                                            break;
                                        }
                                        else if (ChannelState.closed == recvRes.value1.state)
                                        {
                                            break;
                                        }
                                    }
                                    finally
                                    {
                                        self.lock_suspend_();
                                    }
                                }
                                finally
                                {
                                    await mutex_unlock_shared(_mutex);
                                }
                            }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self.lock_stop_();
                            self._mailboxMap = null;
                            self._genLocal = null;
                            Channel.AsyncRemoveRecvNotify(self.unsafe_async_ignore<ChannelState>(), ntfSign);
                            await self.async_wait();
                            await self.unlock_suspend_and_stop_();
                        }
                    }
                });
                return this;
            }

            public receive_mail timed_case_of<T>(CspChannel<VoidType, T> Channel, int ms, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, T>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (T msg) => check_void_task(handler(msg)), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T1, T2>(CspChannel<VoidType, TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2>>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (TupleEx<T1, T2> msg) => check_void_task(handler(msg.value1, msg.value2)), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T1, T2, T3>(CspChannel<VoidType, TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (TupleEx<T1, T2, T3> msg) => check_void_task(handler(msg.value1, msg.value2, msg.value3)), errHandler, lostMsg);
            }

            public receive_mail timed_case_of(CspChannel<VoidType, VoidType> Channel, int ms, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, VoidType>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (VoidType _) => check_void_task(handler()), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<R>(CspChannel<R, VoidType> Channel, int ms, Func<Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (VoidType _) => handler(), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<R, T>(CspChannel<R, T> Channel, int ms, Func<T, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, handler, null, errHandler, lostMsg);
            }

            public receive_mail timed_case_of<R>(CspChannel<R, VoidType> Channel, int ms, Func<ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (VoidType _) => handler(), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg);
            }

            public receive_mail timed_case_of<R, T>(CspChannel<R, T> Channel, int ms, Func<T, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                return timed_case_of(Channel, ms, null, handler, errHandler, lostMsg);
            }

            private receive_mail timed_case_of<R, T>(CspChannel<R, T> Channel, int ms, Func<T, Task<R>> handler, Func<T, ValueTask<R>> gohandler, Func<ChannelState, Task<bool>> errHandler, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg)
            {
                _children.Go(async delegate ()
                {
                    Generator self = Generator.self;
                    if (null == _mutex)
                    {
                        CspWaitWrap<R, T> recvRes = await csp_timed_wait(Channel, ms, lostMsg);
                        try
                        {
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            if (ChannelState.ok == recvRes.state)
                            {
                                try
                                {
                                    recvRes.Complete(null != handler ? await handler(recvRes.msg) : await gohandler(recvRes.msg));
                                }
                                catch (CspFailException)
                                {
                                    recvRes.Fail();
                                }
                                catch (StopException)
                                {
                                    recvRes.Fail();
                                    throw;
                                }
                                catch (System.Exception)
                                {
                                    recvRes.Fail();
                                    throw;
                                }
                            }
                            else if (null != errHandler && await errHandler(recvRes.state)) { }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self._mailboxMap = null;
                            self._genLocal = null;
                        }
                    }
                    else
                    {
                        ChannelNotifySign ntfSign = new ChannelNotifySign();
                        try
                        {
                            self.lock_suspend_();
                            self._mailboxMap = _children.parent._mailboxMap;
                            self._genLocal = _children.parent._genLocal;
                            long endTick = SystemTick.GetTickMs() + ms;
                            while (_run)
                            {
                                ChannelState result = ChannelState.undefined;
                                Channel.AsyncAppendRecvNotify(self.unsafe_async_callback((ChannelState state) => result = state), ntfSign, ms);
                                await self.async_wait();
                                await mutex_lock_shared(_mutex);
                                try
                                {
                                    if (ChannelState.overtime != result)
                                    {
                                        CspWaitWrap<R, T> recvRes = await csp_try_wait(Channel, lostMsg);
                                        try
                                        {
                                            await self.unlock_suspend_();
                                            if (ChannelState.ok == recvRes.state)
                                            {
                                                try
                                                {
                                                    recvRes.Complete(null != handler ? await handler(recvRes.msg) : await gohandler(recvRes.msg)); break;
                                                }
                                                catch (CspFailException)
                                                {
                                                    recvRes.Fail();
                                                }
                                                catch (StopException)
                                                {
                                                    recvRes.Fail();
                                                    throw;
                                                }
                                                catch (System.Exception)
                                                {
                                                    recvRes.Fail();
                                                    throw;
                                                }
                                            }
                                            else if ((null != errHandler && await errHandler(recvRes.state)) || ChannelState.closed == recvRes.state) { break; }
                                            if (0 <= ms && 0 >= (ms = (int)(endTick - SystemTick.GetTickMs())))
                                            {
                                                ms = -1;
                                                if (null == errHandler || await errHandler(ChannelState.overtime))
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            self.lock_suspend_();
                                        }
                                    }
                                    else
                                    {
                                        ms = -1;
                                        if (null == errHandler || await errHandler(ChannelState.overtime))
                                        {
                                            break;
                                        }
                                    }
                                }
                                finally
                                {
                                    await mutex_unlock_shared(_mutex);
                                }
                            }
                        }
                        catch (MessageStopCurrentException) { }
                        catch (MessageStopAllException)
                        {
                            _run = false;
                        }
                        finally
                        {
                            self.lock_stop_();
                            self._mailboxMap = null;
                            self._genLocal = null;
                            Channel.AsyncRemoveRecvNotify(self.unsafe_async_ignore<ChannelState>(), ntfSign);
                            await self.async_wait();
                            await self.unlock_suspend_and_stop_();
                        }
                    }
                });
                return this;
            }

            public receive_mail try_case_of<T>(CspChannel<VoidType, T> Channel, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, T>> lostMsg = null)
            {
                return try_case_of(Channel, (T msg) => check_void_task(handler(msg)), errHandler, lostMsg);
            }

            public receive_mail try_case_of<T1, T2>(CspChannel<VoidType, TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2>>> lostMsg = null)
            {
                return try_case_of(Channel, (TupleEx<T1, T2> msg) => check_void_task(handler(msg.value1, msg.value2)), errHandler, lostMsg);
            }

            public receive_mail try_case_of<T1, T2, T3>(CspChannel<VoidType, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                return try_case_of(Channel, (TupleEx<T1, T2, T3> msg) => check_void_task(handler(msg.value1, msg.value2, msg.value3)), errHandler, lostMsg);
            }

            public receive_mail try_case_of(CspChannel<VoidType, VoidType> Channel, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, VoidType>> lostMsg = null)
            {
                return try_case_of(Channel, (VoidType _) => check_void_task(handler()), errHandler, lostMsg);
            }

            public receive_mail try_case_of<R>(CspChannel<R, VoidType> Channel, Func<Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
            {
                return try_case_of(Channel, (VoidType _) => handler(), errHandler, lostMsg);
            }

            public receive_mail try_case_of<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
            {
                return try_case_of(Channel, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg);
            }

            public receive_mail try_case_of<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                return try_case_of(Channel, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg);
            }

            public receive_mail try_case_of<R, T>(CspChannel<R, T> Channel, Func<T, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                return try_case_of(Channel, handler, null, errHandler, lostMsg);
            }

            public receive_mail try_case_of<R>(CspChannel<R, VoidType> Channel, Func<ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
            {
                return try_case_of(Channel, (VoidType _) => handler(), errHandler, lostMsg);
            }

            public receive_mail try_case_of<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
            {
                return try_case_of(Channel, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg);
            }

            public receive_mail try_case_of<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                return try_case_of(Channel, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg);
            }

            public receive_mail try_case_of<R, T>(CspChannel<R, T> Channel, Func<T, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                return try_case_of(Channel, null, handler, errHandler, lostMsg);
            }

            private receive_mail try_case_of<R, T>(CspChannel<R, T> Channel, Func<T, Task<R>> handler, Func<T, ValueTask<R>> gohandler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                _children.Go(async delegate ()
                {
                    Generator self = Generator.self;
                    if (null != _mutex)
                    {
                        await mutex_lock_shared(_mutex);
                    }
                    try
                    {
                        self._mailboxMap = _children.parent._mailboxMap;
                        self._genLocal = _children.parent._genLocal;
                        CspWaitWrap<R, T> recvRes = await csp_try_wait(Channel, lostMsg);
                        if (ChannelState.ok == recvRes.state)
                        {
                            try
                            {
                                recvRes.Complete(null != handler ? await handler(recvRes.msg) : await gohandler(recvRes.msg));
                            }
                            catch (CspFailException)
                            {
                                recvRes.Fail();
                            }
                            catch (StopException)
                            {
                                recvRes.Fail();
                                throw;
                            }
                            catch (System.Exception)
                            {
                                recvRes.Fail();
                                throw;
                            }
                        }
                        else if (null != errHandler && await errHandler(recvRes.state)) { }
                    }
                    catch (MessageStopCurrentException) { }
                    catch (MessageStopAllException)
                    {
                        _run = false;
                    }
                    finally
                    {
                        self._mailboxMap = null;
                        self._genLocal = null;
                        if (null != _mutex)
                        {
                            await mutex_unlock_shared(_mutex);
                        }
                    }
                });
                return this;
            }

            public receive_mail case_of<T>(int id, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                return case_of(self_mailbox<T>(id), handler, errHandler, lostMsg);
            }

            public receive_mail case_of<T1, T2>(int id, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return case_of(self_mailbox<TupleEx<T1, T2>>(id), handler, errHandler, lostMsg);
            }

            public receive_mail case_of<T1, T2, T3>(int id, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return case_of(self_mailbox<TupleEx<T1, T2, T3>>(id), handler, errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T>(int id, int ms, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                return timed_case_of(self_mailbox<T>(id), ms, handler, errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T1, T2>(int id, int ms, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return timed_case_of(self_mailbox<TupleEx<T1, T2>>(id), ms, handler, errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T1, T2, T3>(int id, int ms, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return timed_case_of(self_mailbox<TupleEx<T1, T2, T3>>(id), ms, handler, errHandler, lostMsg);
            }

            public receive_mail try_case_of<T>(int id, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                return try_case_of(self_mailbox<T>(id), handler, errHandler, lostMsg);
            }

            public receive_mail try_case_of<T1, T2>(int id, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return try_case_of(self_mailbox<TupleEx<T1, T2>>(id), handler, errHandler, lostMsg);
            }

            public receive_mail try_case_of<T1, T2, T3>(int id, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return try_case_of(self_mailbox<TupleEx<T1, T2, T3>>(id), handler, errHandler, lostMsg);
            }

            public receive_mail case_of<T>(Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                return case_of(0, handler, errHandler, lostMsg);
            }

            public receive_mail case_of<T1, T2>(Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return case_of(0, handler, errHandler, lostMsg);
            }

            public receive_mail case_of<T1, T2, T3>(Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return case_of(0, handler, errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T>(int ms, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                return timed_case_of(0, ms, handler, errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T1, T2>(int ms, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return timed_case_of(0, ms, handler, errHandler, lostMsg);
            }

            public receive_mail timed_case_of<T1, T2, T3>(int ms, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return timed_case_of(0, ms, handler, errHandler, lostMsg);
            }

            public receive_mail try_case_of<T>(Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                return try_case_of(0, handler, errHandler, lostMsg);
            }

            public receive_mail try_case_of<T1, T2>(Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return try_case_of(0, handler, errHandler, lostMsg);
            }

            public receive_mail try_case_of<T1, T2, T3>(Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return try_case_of(0, handler, errHandler, lostMsg);
            }

            public receive_mail case_of(int id, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return case_of(self_mailbox<VoidType>(id), handler, errHandler, lostMsg);
            }

            public receive_mail timed_case_of(int id, int ms, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return timed_case_of(self_mailbox<VoidType>(id), ms, handler, errHandler, lostMsg);
            }

            public receive_mail try_case_of(int id, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return try_case_of(self_mailbox<VoidType>(id), handler, errHandler, lostMsg);
            }

            public receive_mail case_of(Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return case_of(0, handler, errHandler, lostMsg);
            }

            public receive_mail timed_case_of(int ms, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return timed_case_of(0, ms, handler, errHandler, lostMsg);
            }

            public receive_mail try_case_of(Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                return try_case_of(0, handler, errHandler, lostMsg);
            }

            public async Task End()
            {
                while (0 != _children.Count)
                {
                    await _children.wait_any();
                    if (!_run)
                    {
                        if (null != _mutex)
                        {
                            await mutex_lock(_mutex);
                            await _children.Stop();
                            await mutex_unlock(_mutex);
                        }
                        else
                        {
                            await _children.Stop();
                        }
                    }
                }
            }

            public Func<Task> Wrap()
            {
                return End;
            }

            static public void stop_current()
            {
                Debug.Assert(null != self && null != self.parent && self.parent._mailboxMap == self._mailboxMap, "不正确的 stop_current 调用!");
                throw new MessageStopCurrentException();
            }

            static public void stop_all()
            {
                Debug.Assert(null != self && null != self.parent && self.parent._mailboxMap == self._mailboxMap, "不正确的 stop_all 调用!");
                throw new MessageStopAllException();
            }
        }

        static public receive_mail Receive(bool forceStopAll = true)
        {
            return new receive_mail(forceStopAll);
        }

        public struct select_chans
        {
            internal bool _random;
            internal bool _whenEnable;
            internal LinkedList<SelectChannelBase> _chans;
            internal LinkedListNode<SelectChannelBase> _lastChansNode;
            internal UnlimitChannel<TupleEx<ChannelState, SelectChannelBase>> _selectChans;

            public select_chans case_recv_mail<T>(Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                return case_receive(self_mailbox<T>(), handler, errHandler, lostMsg);
            }

            public select_chans case_recv_mail<T1, T2>(Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return case_receive(self_mailbox<TupleEx<T1, T2>>(), handler, errHandler, lostMsg);
            }

            public select_chans case_recv_mail<T1, T2, T3>(Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return case_receive(self_mailbox<TupleEx<T1, T2, T3>>(), handler, errHandler, lostMsg);
            }

            public select_chans case_recv_mail<T>(int id, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                return case_receive(self_mailbox<T>(id), handler, errHandler, lostMsg);
            }

            public select_chans case_recv_mail<T1, T2>(int id, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return case_receive(self_mailbox<TupleEx<T1, T2>>(id), handler, errHandler, lostMsg);
            }

            public select_chans case_recv_mail<T1, T2, T3>(int id, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return case_receive(self_mailbox<TupleEx<T1, T2, T3>>(id), handler, errHandler, lostMsg);
            }

            public select_chans case_receive<T>(Channel<T> Channel, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<T1, T2>(Channel<TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), null, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<T1, T2, T3>(Channel<TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), null, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive(Channel<VoidType> Channel, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((VoidType _) => handler(), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_send<T>(Channel<T> Channel, AsyncResultWrap<T> msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(msg, handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_send<T>(Channel<T> Channel, T msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(msg, handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_send(Channel<VoidType> Channel, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(default(VoidType), handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<T1, T2>(BroadcastChannel<TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), token, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<T1, T2, T3>(BroadcastChannel<TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), token, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<T>(BroadcastChannel<T> Channel, Func<T, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(handler, token, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive(BroadcastChannel<VoidType> Channel, Func<Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((VoidType _) => handler(), token, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<R, T>(CspChannel<R, T> Channel, Func<T, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<R>(CspChannel<R, VoidType> Channel, Func<Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((VoidType _) => handler(), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<R, T>(CspChannel<R, T> Channel, Func<T, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, Func<T1, T2, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<R>(CspChannel<R, VoidType> Channel, Func<ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((VoidType _) => handler(), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<T>(CspChannel<VoidType, T> Channel, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, T>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((T msg) => check_void_task(handler(msg)), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<T1, T2>(CspChannel<VoidType, TupleEx<T1, T2>> Channel, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((TupleEx<T1, T2> msg) => check_void_task(handler(msg.value1, msg.value2)), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive<T1, T2, T3>(CspChannel<VoidType, TupleEx<T1, T2, T3>> Channel, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((TupleEx<T1, T2, T3> msg) => check_void_task(handler(msg.value1, msg.value2, msg.value3)), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_receive(CspChannel<VoidType, VoidType> Channel, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, VoidType>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader((VoidType _) => check_void_task(handler()), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_send<R, T>(CspChannel<R, T> Channel, AsyncResultWrap<T> msg, Func<R, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(msg, handler, errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_send<R, T>(CspChannel<R, T> Channel, T msg, Func<R, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(msg, handler, errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_send<R>(CspChannel<R, VoidType> Channel, Func<R, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<R> lostRes = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(default(VoidType), handler, errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_send<T>(CspChannel<VoidType, T> Channel, AsyncResultWrap<T> msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<VoidType> lostRes = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(msg, (VoidType _) => handler(), errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_send<T>(CspChannel<VoidType, T> Channel, T msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<VoidType> lostRes = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(msg, (VoidType _) => handler(), errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_send(CspChannel<VoidType, VoidType> Channel, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<VoidType> lostRes = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(default(VoidType), (VoidType _) => handler(), errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_recv_mail<T>(int ms, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                return case_timed_receive(self_mailbox<T>(), ms, handler, errHandler, lostMsg);
            }

            public select_chans case_timed_recv_mail<T1, T2>(int ms, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return case_timed_receive(self_mailbox<TupleEx<T1, T2>>(), ms, handler, errHandler, lostMsg);
            }

            public select_chans case_timed_recv_mail<T1, T2, T3>(int ms, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return case_timed_receive(self_mailbox<TupleEx<T1, T2, T3>>(), ms, handler, errHandler, lostMsg);
            }

            public select_chans case_timed_recv_mail<T>(int id, int ms, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                return case_timed_receive(self_mailbox<T>(id), ms, handler, errHandler, lostMsg);
            }

            public select_chans case_timed_recv_mail<T1, T2>(int id, int ms, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                return case_timed_receive(self_mailbox<TupleEx<T1, T2>>(id), ms, handler, errHandler, lostMsg);
            }

            public select_chans case_timed_recv_mail<T1, T2, T3>(int id, int ms, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                return case_timed_receive(self_mailbox<TupleEx<T1, T2, T3>>(id), ms, handler, errHandler, lostMsg);
            }

            public select_chans case_timed_receive<T>(Channel<T> Channel, int ms, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<T1, T2>(Channel<TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), null, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<T1, T2, T3>(Channel<TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), null, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive(Channel<VoidType> Channel, int ms, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (VoidType _) => handler(), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_send<T>(Channel<T> Channel, int ms, AsyncResultWrap<T> msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(ms, msg, handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_send<T>(Channel<T> Channel, int ms, T msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(ms, msg, handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_send(Channel<VoidType> Channel, int ms, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(ms, default(VoidType), handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<T1, T2>(BroadcastChannel<TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), token, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<T1, T2, T3>(BroadcastChannel<TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<TupleEx<T1, T2, T3>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), token, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<T>(BroadcastChannel<T> Channel, int ms, Func<T, Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, handler, token, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive(BroadcastChannel<VoidType> Channel, int ms, Func<Task> handler, BroadcastToken token = null, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (VoidType _) => handler(), token, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<R, T>(CspChannel<R, T> Channel, int ms, Func<T, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<R>(CspChannel<R, VoidType> Channel, int ms, Func<Task<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (VoidType _) => handler(), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<R, T>(CspChannel<R, T> Channel, int ms, Func<T, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, T>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, handler, errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<R, T1, T2>(CspChannel<R, TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (TupleEx<T1, T2> msg) => handler(msg.value1, msg.value2), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<R, T1, T2, T3>(CspChannel<R, TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (TupleEx<T1, T2, T3> msg) => handler(msg.value1, msg.value2, msg.value3), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<R>(CspChannel<R, VoidType> Channel, int ms, Func<ValueTask<R>> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<R, VoidType>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (VoidType _) => handler(), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<T>(CspChannel<VoidType, T> Channel, int ms, Func<T, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, T>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (T msg) => check_void_task(handler(msg)), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<T1, T2>(CspChannel<VoidType, TupleEx<T1, T2>> Channel, int ms, Func<T1, T2, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (TupleEx<T1, T2> msg) => check_void_task(handler(msg.value1, msg.value2)), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive<T1, T2, T3>(CspChannel<VoidType, TupleEx<T1, T2, T3>> Channel, int ms, Func<T1, T2, T3, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, TupleEx<T1, T2, T3>>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (TupleEx<T1, T2, T3> msg) => check_void_task(handler(msg.value1, msg.value2, msg.value3)), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_receive(CspChannel<VoidType, VoidType> Channel, int ms, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, ChannelLostMsg<CspWaitWrap<VoidType, VoidType>> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectReader(ms, (VoidType _) => check_void_task(handler()), errHandler, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_send<R, T>(CspChannel<R, T> Channel, int ms, AsyncResultWrap<T> msg, Func<R, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(ms, msg, handler, errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_send<R, T>(CspChannel<R, T> Channel, int ms, T msg, Func<R, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<R> lostRes = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(ms, msg, handler, errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_send<R>(CspChannel<R, VoidType> Channel, int ms, Func<R, Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<R> lostRes = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(ms, default(VoidType), handler, errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_send<T>(CspChannel<VoidType, T> Channel, int ms, AsyncResultWrap<T> msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<VoidType> lostRes = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(ms, msg, (VoidType _) => handler(), errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_send<T>(CspChannel<VoidType, T> Channel, int ms, T msg, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<VoidType> lostRes = null, ChannelLostMsg<T> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(ms, msg, (VoidType _) => handler(), errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public select_chans case_timed_send(CspChannel<VoidType, VoidType> Channel, int ms, Func<Task> handler, Func<ChannelState, Task<bool>> errHandler = null, Action<VoidType> lostRes = null, ChannelLostMsg<VoidType> lostMsg = null)
            {
                SelectChannelBase selectChan = Channel.MakeSelectWriter(ms, default(VoidType), (VoidType _) => handler(), errHandler, lostRes, lostMsg); selectChan.enable = _whenEnable;
                return new select_chans { _random = _random, _whenEnable = true, _chans = _chans, _lastChansNode = _chans.AddLast(selectChan), _selectChans = _selectChans };
            }

            public bool effective()
            {
                return null != _lastChansNode;
            }

            public void enable(bool enable)
            {
                _lastChansNode.Value.enable = enable;
            }

            public void remove()
            {
                _chans.Remove(_lastChansNode);
            }

            public void restore()
            {
                _chans.AddLast(_lastChansNode);
            }

            public select_chans when(bool when)
            {
                return new select_chans { _random = _random, _whenEnable = when, _chans = _chans, _selectChans = _selectChans };
            }

            static private SelectChannelBase[] shuffle(LinkedList<SelectChannelBase> chans)
            {
                int Count = chans.Count;
                SelectChannelBase[] shuffChans = new SelectChannelBase[Count];
                chans.CopyTo(shuffChans, 0);
                RndEx randGen = RndEx.Global;
                for (int i = 0; i < Count; i++)
                {
                    int rand = randGen.Next(i, Count);
                    SelectChannelBase t = shuffChans[rand];
                    shuffChans[rand] = shuffChans[i];
                    shuffChans[i] = t;
                }
                return shuffChans;
            }

            public async Task<bool> loop(Func<Task> eachAfterDo = null)
            {
                Generator this_ = self;
                LinkedList<SelectChannelBase> chans = _chans;
                UnlimitChannel<TupleEx<ChannelState, SelectChannelBase>> selectChans = _selectChans;
                try
                {
                    this_.lock_suspend_and_stop_();
                    if (null == this_._topSelectChans)
                    {
                        this_._topSelectChans = new LinkedList<LinkedList<SelectChannelBase>>();
                    }
                    this_._topSelectChans.AddFirst(chans);
                    if (_random)
                    {
                        SelectChannelBase[] shuffChans = await send_task(() => shuffle(chans));
                        int len = shuffChans.Length;
                        for (int i = 0; i < len; i++)
                        {
                            SelectChannelBase Channel = shuffChans[i];
                            Channel.ntfSign._selectOnce = false;
                            Channel.NextSelect = (ChannelState state) => selectChans.Post(TupleEx.Make(state, Channel));
                            Channel.Begin(this_);
                        }
                    }
                    else
                    {
                        for (LinkedListNode<SelectChannelBase> it = chans.First; null != it; it = it.Next)
                        {
                            SelectChannelBase Channel = it.Value;
                            Channel.ntfSign._selectOnce = false;
                            Channel.NextSelect = (ChannelState state) => selectChans.Post(TupleEx.Make(state, Channel));
                            Channel.Begin(this_);
                        }
                    }
                    this_.unlock_stop_();
                    int Count = chans.Count;
                    bool selected = false;
                    Func<Task> stepOne = null == eachAfterDo ? (Func<Task>)null : delegate ()
                    {
                        selected = true;
                        return non_async();
                    };
                    while (0 != Count)
                    {
                        TupleEx<ChannelState, SelectChannelBase> selectedChan = (await chan_receive(selectChans)).msg;
                        if (ChannelState.ok != selectedChan.value1)
                        {
                            if (await selectedChan.value2.ErrInvoke(selectedChan.value1))
                            {
                                Count--;
                            }
                            continue;
                        }
                        else if (selectedChan.value2.disabled())
                        {
                            continue;
                        }
                        try
                        {
                            SelectChannelState selState = await selectedChan.value2.Invoke(stepOne);
                            if (!selState.NextRound)
                            {
                                Count--;
                            }
                        }
                        catch (SelectStopCurrentException)
                        {
                            Count--;
                            await selectedChan.value2.End();
                        }
                        if (selected)
                        {
                            try
                            {
                                selected = false;
                                await this_.unlock_suspend_();
                                await eachAfterDo();
                            }
                            finally
                            {
                                this_.lock_suspend_();
                            }
                        }
                    }
                    return true;
                }
                catch (SelectStopAllException)
                {
                    return false;
                }
                finally
                {
                    this_.lock_stop_();
                    this_._topSelectChans.RemoveFirst();
                    for (LinkedListNode<SelectChannelBase> it = chans.First; null != it; it = it.Next)
                    {
                        await it.Value.End();
                    }
                    selectChans.Clear();
                    await this_.unlock_suspend_and_stop_();
                }
            }

            public async Task<bool> timed_loop(int ms, Func<bool, Task> eachAfterDo = null)
            {
                Generator this_ = self;
                LinkedList<SelectChannelBase> chans = _chans;
                UnlimitChannel<TupleEx<ChannelState, SelectChannelBase>> selectChans = _selectChans;
                AsyncTimer timer = ms >= 0 ? new AsyncTimer(this_.Strand) : null;
                try
                {
                    this_.lock_suspend_and_stop_();
                    if (null == this_._topSelectChans)
                    {
                        this_._topSelectChans = new LinkedList<LinkedList<SelectChannelBase>>();
                    }
                    this_._topSelectChans.AddFirst(chans);
                    if (_random)
                    {
                        SelectChannelBase[] shuffChans = await send_task(() => shuffle(chans));
                        int len = shuffChans.Length;
                        for (int i = 0; i < len; i++)
                        {
                            SelectChannelBase Channel = shuffChans[i];
                            Channel.ntfSign._selectOnce = false;
                            Channel.NextSelect = (ChannelState state) => selectChans.Post(TupleEx.Make(state, Channel));
                            Channel.Begin(this_);
                            if (0 == ms)
                            {
                                SharedStrand chanStrand = Channel.Channel().SelfStrand();
                                if (chanStrand != this_.Strand)
                                {
                                    chanStrand.Post(this_.unsafe_async_result());
                                    await this_.async_wait();
                                }
                            }
                        }
                    }
                    else
                    {
                        for (LinkedListNode<SelectChannelBase> it = chans.First; null != it; it = it.Next)
                        {
                            SelectChannelBase Channel = it.Value;
                            Channel.ntfSign._selectOnce = false;
                            Channel.NextSelect = (ChannelState state) => selectChans.Post(TupleEx.Make(state, Channel));
                            Channel.Begin(this_);
                            if (0 == ms)
                            {
                                SharedStrand chanStrand = Channel.Channel().SelfStrand();
                                if (chanStrand != this_.Strand)
                                {
                                    chanStrand.Post(this_.unsafe_async_result());
                                    await this_.async_wait();
                                }
                            }
                        }
                    }
                    this_.unlock_stop_();
                    int timerCount = 0;
                    int Count = chans.Count;
                    bool selected = false;
                    Func<Task> stepOne = delegate ()
                    {
                        timer?.Cancel();
                        selected = true;
                        return non_async();
                    };
                    if (null != timer)
                    {
                        int timerId = ++timerCount;
                        timer.Timeout(ms, () => selectChans.Post(TupleEx.Make((ChannelState)timerId, default(SelectChannelBase))));
                    }
                    while (0 != Count)
                    {
                        TupleEx<ChannelState, SelectChannelBase> selectedChan = (await chan_receive(selectChans)).msg;
                        if (null != selectedChan.value2)
                        {
                            if (ChannelState.ok != selectedChan.value1)
                            {
                                if (await selectedChan.value2.ErrInvoke(selectedChan.value1))
                                {
                                    Count--;
                                }
                                continue;
                            }
                            else if (selectedChan.value2.disabled())
                            {
                                continue;
                            }
                            try
                            {
                                SelectChannelState selState = await selectedChan.value2.Invoke(stepOne);
                                if (!selState.NextRound)
                                {
                                    Count--;
                                }
                                else if (0 == ms)
                                {
                                    SharedStrand chanStrand = selectedChan.value2.Channel().SelfStrand();
                                    if (chanStrand != this_.Strand)
                                    {
                                        chanStrand.Post(this_.unsafe_async_result());
                                        await this_.async_wait();
                                    }
                                }
                            }
                            catch (SelectStopCurrentException)
                            {
                                Count--;
                                await selectedChan.value2.End();
                            }
                        }
                        else if (timerCount == (int)selectedChan.value1)
                        {
                            selected = true;
                        }
                        else
                        {
                            continue;
                        }
                        if (selected)
                        {
                            try
                            {
                                selected = false;
                                await this_.unlock_suspend_();
                                if (null != eachAfterDo)
                                {
                                    await eachAfterDo(null != selectedChan.value2);
                                }
                                if (null != timer)
                                {
                                    int timerId = ++timerCount;
                                    timer.Timeout(ms, () => selectChans.Post(TupleEx.Make((ChannelState)timerId, default(SelectChannelBase))));
                                }
                            }
                            finally
                            {
                                this_.lock_suspend_();
                            }
                        }
                    }
                    return true;
                }
                catch (SelectStopAllException)
                {
                    return false;
                }
                finally
                {
                    this_.lock_stop_();
                    timer?.Cancel();
                    this_._topSelectChans.RemoveFirst();
                    for (LinkedListNode<SelectChannelBase> it = chans.First; null != it; it = it.Next)
                    {
                        await it.Value.End();
                    }
                    selectChans.Clear();
                    await this_.unlock_suspend_and_stop_();
                }
            }

            public async Task<bool> End()
            {
                Generator this_ = self;
                LinkedList<SelectChannelBase> chans = _chans;
                UnlimitChannel<TupleEx<ChannelState, SelectChannelBase>> selectChans = _selectChans;
                bool selected = false;
                try
                {
                    this_.lock_suspend_and_stop_();
                    if (null == this_._topSelectChans)
                    {
                        this_._topSelectChans = new LinkedList<LinkedList<SelectChannelBase>>();
                    }
                    this_._topSelectChans.AddFirst(chans);
                    if (_random)
                    {
                        SelectChannelBase[] shuffChans = await send_task(() => shuffle(chans));
                        int len = shuffChans.Length;
                        for (int i = 0; i < len; i++)
                        {
                            SelectChannelBase Channel = shuffChans[i];
                            Channel.ntfSign._selectOnce = false;
                            Channel.NextSelect = (ChannelState state) => selectChans.Post(TupleEx.Make(state, Channel));
                            Channel.Begin(this_);
                        }
                    }
                    else
                    {
                        for (LinkedListNode<SelectChannelBase> it = chans.First; null != it; it = it.Next)
                        {
                            SelectChannelBase Channel = it.Value;
                            Channel.ntfSign._selectOnce = true;
                            Channel.NextSelect = (ChannelState state) => selectChans.Post(TupleEx.Make(state, Channel));
                            Channel.Begin(this_);
                        }
                    }
                    this_.unlock_stop_();
                    int Count = chans.Count;
                    while (0 != Count)
                    {
                        TupleEx<ChannelState, SelectChannelBase> selectedChan = (await chan_receive(selectChans)).msg;
                        if (ChannelState.ok != selectedChan.value1)
                        {
                            if (await selectedChan.value2.ErrInvoke(selectedChan.value1))
                            {
                                Count--;
                            }
                            continue;
                        }
                        else if (selectedChan.value2.disabled())
                        {
                            continue;
                        }
                        try
                        {
                            SelectChannelState selState = await selectedChan.value2.Invoke(async delegate ()
                            {
                                for (LinkedListNode<SelectChannelBase> it = chans.First; null != it; it = it.Next)
                                {
                                    if (selectedChan.value2 != it.Value)
                                    {
                                        await it.Value.End();
                                    }
                                }
                                selected = true;
                            });
                            if (!selState.Failed)
                            {
                                break;
                            }
                            else if (!selState.NextRound)
                            {
                                Count--;
                            }
                        }
                        catch (SelectStopCurrentException)
                        {
                            if (selected)
                            {
                                break;
                            }
                            else
                            {
                                Count--;
                                await selectedChan.value2.End();
                            }
                        }
                    }
                }
                catch (SelectStopAllException) { }
                finally
                {
                    this_.lock_stop_();
                    this_._topSelectChans.RemoveFirst();
                    if (!selected)
                    {
                        for (LinkedListNode<SelectChannelBase> it = chans.First; null != it; it = it.Next)
                        {
                            await it.Value.End();
                        }
                    }
                    selectChans.Clear();
                    await this_.unlock_suspend_and_stop_();
                }
                return selected;
            }

            public async Task<bool> timed(int ms)
            {
                Generator this_ = self;
                LinkedList<SelectChannelBase> chans = _chans;
                UnlimitChannel<TupleEx<ChannelState, SelectChannelBase>> selectChans = _selectChans;
                AsyncTimer timer = ms >= 0 ? new AsyncTimer(this_.Strand) : null;
                bool selected = false;
                try
                {
                    this_.lock_suspend_and_stop_();
                    if (null == this_._topSelectChans)
                    {
                        this_._topSelectChans = new LinkedList<LinkedList<SelectChannelBase>>();
                    }
                    this_._topSelectChans.AddFirst(chans);
                    if (_random)
                    {
                        SelectChannelBase[] shuffChans = await send_task(() => shuffle(chans));
                        int len = shuffChans.Length;
                        for (int i = 0; i < len; i++)
                        {
                            SelectChannelBase Channel = shuffChans[i];
                            Channel.ntfSign._selectOnce = false;
                            Channel.NextSelect = (ChannelState state) => selectChans.Post(TupleEx.Make(state, Channel));
                            Channel.Begin(this_);
                            if (0 == ms)
                            {
                                SharedStrand chanStrand = Channel.Channel().SelfStrand();
                                if (chanStrand != this_.Strand)
                                {
                                    chanStrand.Post(this_.unsafe_async_result());
                                    await this_.async_wait();
                                }
                            }
                        }
                    }
                    else
                    {
                        for (LinkedListNode<SelectChannelBase> it = chans.First; null != it; it = it.Next)
                        {
                            SelectChannelBase Channel = it.Value;
                            Channel.ntfSign._selectOnce = true;
                            Channel.NextSelect = (ChannelState state) => selectChans.Post(TupleEx.Make(state, Channel));
                            Channel.Begin(this_);
                            if (0 == ms)
                            {
                                SharedStrand chanStrand = Channel.Channel().SelfStrand();
                                if (chanStrand != this_.Strand)
                                {
                                    chanStrand.Post(this_.unsafe_async_result());
                                    await this_.async_wait();
                                }
                            }
                        }
                    }
                    this_.unlock_stop_();
                    int Count = chans.Count;
                    timer?.Timeout(ms, selectChans.WrapDefault());
                    while (0 != Count)
                    {
                        TupleEx<ChannelState, SelectChannelBase> selectedChan = (await chan_receive(selectChans)).msg;
                        if (null != selectedChan.value2)
                        {
                            if (ChannelState.ok != selectedChan.value1)
                            {
                                if (await selectedChan.value2.ErrInvoke(selectedChan.value1))
                                {
                                    Count--;
                                }
                                continue;
                            }
                            else if (selectedChan.value2.disabled())
                            {
                                continue;
                            }
                            try
                            {
                                SelectChannelState selState = await selectedChan.value2.Invoke(async delegate ()
                                {
                                    timer?.Cancel();
                                    for (LinkedListNode<SelectChannelBase> it = chans.First; null != it; it = it.Next)
                                    {
                                        if (selectedChan.value2 != it.Value)
                                        {
                                            await it.Value.End();
                                        }
                                    }
                                    selected = true;
                                });
                                if (!selState.Failed)
                                {
                                    break;
                                }
                                else if (!selState.NextRound)
                                {
                                    Count--;
                                }
                            }
                            catch (SelectStopCurrentException)
                            {
                                if (selected)
                                {
                                    break;
                                }
                                else
                                {
                                    Count--;
                                    await selectedChan.value2.End();
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (SelectStopAllException) { }
                finally
                {
                    this_.lock_stop_();
                    timer?.Cancel();
                    this_._topSelectChans.RemoveFirst();
                    if (!selected)
                    {
                        for (LinkedListNode<SelectChannelBase> it = chans.First; null != it; it = it.Next)
                        {
                            await it.Value.End();
                        }
                    }
                    selectChans.Clear();
                    await this_.unlock_suspend_and_stop_();
                }
                return true;
            }

            public Task<bool> again()
            {
                return End();
            }

            public Task<bool> timed_again(int ms)
            {
                return timed(ms);
            }

            public Func<Func<Task>, Task<bool>> wrap_loop()
            {
                return loop;
            }

            public Func<Task<bool>> Wrap()
            {
                return End;
            }

            public Func<int, Task<bool>> WrapTimed()
            {
                return timed;
            }

            public Func<Task<bool>> wrap_loop(Func<Task> eachAferDo)
            {
                return Functional.Bind(loop, eachAferDo);
            }

            public Func<Task<bool>> WrapTimed(int ms)
            {
                return Functional.Bind(timed, ms);
            }

            static public void stop_all()
            {
                Debug.Assert(null != self && null != self._topSelectChans && 0 != self._topSelectChans.Count, "不正确的 stop_all 调用!");
                throw new SelectStopAllException();
            }

            static public void stop_current()
            {
                Debug.Assert(null != self && null != self._topSelectChans && 0 != self._topSelectChans.Count, "不正确的 stop_current 调用!");
                throw new SelectStopCurrentException();
            }

            static public Task disable_other(ChannelBase otherChan, bool disable = true)
            {
                Generator this_ = self;
                if (null != this_._topSelectChans && 0 != this_._topSelectChans.Count)
                {
                    LinkedList<SelectChannelBase> currSelect = this_._topSelectChans.First.Value;
                    for (LinkedListNode<SelectChannelBase> it = currSelect.First; null != it; it = it.Next)
                    {
                        SelectChannelBase Channel = it.Value;
                        if (Channel.Channel() == otherChan && Channel.disabled() != disable)
                        {
                            if (disable)
                            {
                                return Channel.End();
                            }
                            else
                            {
                                Channel.Begin(this_);
                            }
                        }
                    }
                }
                return non_async();
            }

            static public Task disable_other_receive(ChannelBase otherChan, bool disable = true)
            {
                Generator this_ = self;
                if (null != this_._topSelectChans && 0 != this_._topSelectChans.Count)
                {
                    LinkedList<SelectChannelBase> currSelect = this_._topSelectChans.First.Value;
                    for (LinkedListNode<SelectChannelBase> it = currSelect.First; null != it; it = it.Next)
                    {
                        SelectChannelBase Channel = it.Value;
                        if (Channel.Channel() == otherChan && Channel.IsRead() && Channel.disabled() != disable)
                        {
                            if (disable)
                            {
                                return Channel.End();
                            }
                            else
                            {
                                Channel.Begin(this_);
                            }
                        }
                    }
                }
                return non_async();
            }

            static public Task disable_other_send(ChannelBase otherChan, bool disable = true)
            {
                Generator this_ = self;
                if (null != this_._topSelectChans && 0 != this_._topSelectChans.Count)
                {
                    LinkedList<SelectChannelBase> currSelect = this_._topSelectChans.First.Value;
                    for (LinkedListNode<SelectChannelBase> it = currSelect.First; null != it; it = it.Next)
                    {
                        SelectChannelBase Channel = it.Value;
                        if (Channel.Channel() == otherChan && !Channel.IsRead() && Channel.disabled() != disable)
                        {
                            if (disable)
                            {
                                return Channel.End();
                            }
                            else
                            {
                                Channel.Begin(this_);
                            }
                        }
                    }
                }
                return non_async();
            }

            static public Task enable_other(ChannelBase otherChan)
            {
                return disable_other(otherChan, false);
            }

            static public Task enable_other_receive(ChannelBase otherChan)
            {
                return disable_other_receive(otherChan, false);
            }

            static public Task enable_other_send(ChannelBase otherChan)
            {
                return disable_other_send(otherChan, false);
            }
        }

        static public select_chans select(bool random = false)
        {
            return new select_chans { _random = random, _whenEnable = true, _chans = new LinkedList<SelectChannelBase>(), _selectChans = new UnlimitChannel<TupleEx<ChannelState, SelectChannelBase>>(SelfStrand()) };
        }

        static public System.Version version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        static public Action<System.Exception> hook_exception
        {
            set
            {
                _hookException = value;
            }
        }

        public class child : Generator
        {
            bool _isFree;
            internal children _childrenMgr;
            internal LinkedListNode<child> _childNode;

            child(children childrenMgr, bool isFree = false) : base()
            {
                _isFree = isFree;
                _childrenMgr = childrenMgr;
            }

            static internal child Make(children childrenMgr, SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                return (child)(new child(childrenMgr)).Init(Strand, generatorAction, completedHandler, suspendHandler);
            }

            static internal child free_make(children childrenMgr, SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                return (child)(new child(childrenMgr, true)).Init(Strand, generatorAction, completedHandler, suspendHandler);
            }

            public override Generator parent
            {
                get
                {
                    return null != _childrenMgr ? _childrenMgr.parent : null;
                }
            }

            public override children brothers
            {
                get
                {
                    return _childrenMgr;
                }
            }

            public bool is_free()
            {
                return _isFree;
            }
        }

        public class children
        {
            bool _ignoreSuspend;
            Generator _parent;
            LinkedList<child> _children;
            LinkedListNode<children> _node;

            public children()
            {
                Debug.Assert(null != self, "children 必须在 Generator 内部创建!");
                _parent = self;
                _ignoreSuspend = false;
                _children = new LinkedList<child>();
                if (null == _parent._children)
                {
                    _parent._children = new LinkedList<children>();
                }
            }

            void check_append_node()
            {
                if (0 == _children.Count)
                {
                    _node = _parent._children.AddLast(this);
                }
            }

            void check_remove_node()
            {
                if (0 == _children.Count && null != _node)
                {
                    _parent._children.Remove(_node);
                    _node = null;
                }
            }

            public child Make(SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                check_append_node();
                child newGen = child.Make(this, Strand, generatorAction, completedHandler, suspendHandler);
                newGen._childNode = _children.AddLast(newGen);
                return newGen;
            }

            public child free_make(SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                check_append_node();
                child newGen = null;
                newGen = child.free_make(this, Strand, generatorAction, delegate ()
                {
                    _parent.Strand.Post(delegate ()
                    {
                        if (null != newGen._childNode)
                        {
                            _children.Remove(newGen._childNode);
                            newGen._childNode = null;
                            newGen._childrenMgr = null;
                            check_remove_node();
                        }
                    });
                    completedHandler?.Invoke();
                }, suspendHandler);
                newGen._childNode = _children.AddLast(newGen);
                return newGen;
            }

            public void Go(SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                Make(Strand, generatorAction, completedHandler, suspendHandler).Run();
            }

            public void Go(out child newChild, SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                newChild = Make(Strand, generatorAction, completedHandler, suspendHandler);
                newChild.Run();
            }

            public void free_go(SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                free_make(Strand, generatorAction, completedHandler, suspendHandler).Run();
            }

            public void free_go(out child newChild, SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                newChild = free_make(Strand, generatorAction, completedHandler, suspendHandler);
                newChild.Run();
            }

            public child Tgo(SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                child newGen = Make(Strand, generatorAction, completedHandler, suspendHandler);
                newGen.Trun();
                return newGen;
            }

            public child free_tgo(SharedStrand Strand, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                child newGen = free_make(Strand, generatorAction, completedHandler, suspendHandler);
                newGen.Trun();
                return newGen;
            }

            public child Make(action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                return Make(_parent.Strand, generatorAction, completedHandler, suspendHandler);
            }

            public child free_make(action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                return free_make(_parent.Strand, generatorAction, completedHandler, suspendHandler);
            }

            public void Go(action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                Go(_parent.Strand, generatorAction, completedHandler, suspendHandler);
            }

            public void Go(out child newChild, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                Go(out newChild, _parent.Strand, generatorAction, completedHandler, suspendHandler);
            }

            public void free_go(action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                free_go(_parent.Strand, generatorAction, completedHandler, suspendHandler);
            }

            public void free_go(out child newChild, action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                free_go(out newChild, _parent.Strand, generatorAction, completedHandler, suspendHandler);
            }

            public child Tgo(action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                return Tgo(_parent.Strand, generatorAction, completedHandler, suspendHandler);
            }

            public child free_tgo(action generatorAction, Action completedHandler = null, Action<bool> suspendHandler = null)
            {
                return free_tgo(_parent.Strand, generatorAction, completedHandler, suspendHandler);
            }

            public void ignore_suspend(bool igonre = true)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                _ignoreSuspend = igonre;
            }

            internal void Suspend(bool isSuspend, Action cb)
            {
                if (!_ignoreSuspend && 0 != _children.Count)
                {
                    int Count = _children.Count;
                    Action handler = _parent.Strand.Wrap(delegate ()
                    {
                        if (0 == --Count)
                        {
                            cb();
                        }
                    });
                    if (isSuspend)
                    {
                        for (LinkedListNode<child> it = _children.First; null != it; it = it.Next)
                        {
                            it.Value.Suspend(handler);
                        }
                    }
                    else
                    {
                        for (LinkedListNode<child> it = _children.First; null != it; it = it.Next)
                        {
                            it.Value.Resume(handler);
                        }
                    }
                }
                else
                {
                    cb();
                }
            }

            public int Count
            {
                get
                {
                    Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                    return _children.Count;
                }
            }

            public Generator parent
            {
                get
                {
                    return _parent;
                }
            }

            public bool ignored_suspend
            {
                get
                {
                    return _ignoreSuspend;
                }
            }

            public int Discard(IEnumerable<child> gens)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                int Count = 0;
                foreach (child ele in gens)
                {
                    if (null != ele._childNode)
                    {
                        Debug.Assert(ele._childNode.List == _children, "此 child 不属于当前 children!");
                        Count++;
                        _children.Remove(ele._childNode);
                        ele._childNode = null;
                        ele._childrenMgr = null;
                    }
                }
                check_remove_node();
                return Count;
            }

            public int Discard(params child[] gens)
            {
                return Discard((IEnumerable<child>)gens);
            }

            static public int Discard(IEnumerable<children> childrens)
            {
                int Count = 0;
                foreach (children childs in childrens)
                {
                    Debug.Assert(self == childs._parent, "此 children 不属于当前 Generator!");
                    for (LinkedListNode<child> it = childs._children.First; null != it; it = it.Next)
                    {
                        Count++;
                        childs._children.Remove(it.Value._childNode);
                        it.Value._childNode = null;
                        it.Value._childrenMgr = null;
                        childs.check_remove_node();
                    }
                }
                return Count;
            }

            static public int Discard(params children[] childrens)
            {
                return Discard((IEnumerable<children>)childrens);
            }

            private async Task stop_(child gen)
            {
                await _parent.async_wait();
                if (null != gen._childNode)
                {
                    _children.Remove(gen._childNode);
                    gen._childNode = null;
                    gen._childrenMgr = null;
                    check_remove_node();
                }
            }

            public Task Stop(child gen)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                Debug.Assert(null == gen._childNode || gen._childNode.List == _children, "此 child 不属于当前 children!");
                if (null != gen._childNode)
                {
                    gen.Stop(_parent.unsafe_async_result());
                    if (!_parent.new_task_completed())
                    {
                        return stop_(gen);
                    }
                    if (null != gen._childNode)
                    {
                        _children.Remove(gen._childNode);
                        gen._childNode = null;
                        gen._childrenMgr = null;
                        check_remove_node();
                    }
                }
                return non_async();
            }

            public async Task Stop(IEnumerable<child> gens)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                int Count = 0;
                UnlimitChannel<child> waitStop = new UnlimitChannel<child>(_parent.Strand);
                Action<Generator> stopHandler = (Generator host) => waitStop.Post((child)host);
                foreach (child ele in gens)
                {
                    Count++;
                    Debug.Assert(null == ele._childNode || ele._childNode.List == _children, "此 child 不属于当前 children!");
                    if (null != ele._childNode && !ele.is_completed())
                    {
                        ele.Stop(stopHandler);
                    }
                    else
                    {
                        waitStop.Post(ele);
                    }
                }
                for (int i = 0; i < Count; i++)
                {
                    child gen = (await chan_receive(waitStop)).msg;
                    if (null != gen._childNode)
                    {
                        _children.Remove(gen._childNode);
                        gen._childNode = null;
                        gen._childrenMgr = null;
                    }
                }
                check_remove_node();
            }

            public Task Stop(params child[] gens)
            {
                return Stop((IEnumerable<child>)gens);
            }

            private async Task wait_(child gen)
            {
                await _parent.async_wait();
                if (null != gen._childNode)
                {
                    _children.Remove(gen._childNode);
                    gen._childNode = null;
                    gen._childrenMgr = null;
                    check_remove_node();
                }
            }

            public Task Wait(child gen)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                Debug.Assert(null == gen._childNode || gen._childNode.List == _children, "此 child 不属于当前 children!");
                if (null != gen._childNode)
                {
                    gen.append_stop_callback(_parent.unsafe_async_result());
                    if (!_parent.new_task_completed())
                    {
                        return wait_(gen);
                    }
                    if (null != gen._childNode)
                    {
                        _children.Remove(gen._childNode);
                        gen._childNode = null;
                        gen._childrenMgr = null;
                        check_remove_node();
                    }
                }
                return non_async();
            }

            public async Task Wait(IEnumerable<child> gens)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                int Count = 0;
                UnlimitChannel<child> waitStop = new UnlimitChannel<child>(_parent.Strand);
                Action<Generator> stopHandler = (Generator host) => waitStop.Post((child)host);
                foreach (child ele in gens)
                {
                    Count++;
                    Debug.Assert(null == ele._childNode || ele._childNode.List == _children, "此 child 不属于当前 children!");
                    if (null != ele._childNode && !ele.is_completed())
                    {
                        ele.append_stop_callback(stopHandler);
                    }
                    else
                    {
                        waitStop.Post(ele);
                    }
                }
                for (int i = 0; i < Count; i++)
                {
                    child gen = (await chan_receive(waitStop)).msg;
                    if (null != gen._childNode)
                    {
                        _children.Remove(gen._childNode);
                        gen._childNode = null;
                        gen._childrenMgr = null;
                    }
                }
                check_remove_node();
            }

            public Task Wait(params child[] gens)
            {
                return Wait((IEnumerable<child>)gens);
            }

            private async Task<bool> timed_wait_(ValueTask<bool> task, child gen)
            {
                bool overtime = !await task;
                if (!overtime && null != gen._childNode)
                {
                    _children.Remove(gen._childNode);
                    gen._childNode = null;
                    gen._childrenMgr = null;
                    check_remove_node();
                }
                return !overtime;
            }

            public ValueTask<bool> TimedWait(int ms, child gen)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                Debug.Assert(null == gen._childNode || gen._childNode.List == _children, "此 child 不属于当前 children!");
                bool overtime = false;
                if (null != gen._childNode)
                {
                    ValueTask<bool> task = timed_wait_other(ms, gen);
                    if (!task.IsCompleted)
                    {
                        return to_vtask(timed_wait_(task, gen));
                    }
                    overtime = !task.GetAwaiter().GetResult();
                    if (!overtime && null != gen._childNode)
                    {
                        _children.Remove(gen._childNode);
                        gen._childNode = null;
                        gen._childrenMgr = null;
                        check_remove_node();
                    }
                }
                return to_vtask(!overtime);
            }

            public Task<child> wait_any(IEnumerable<child> gens)
            {
                return timed_wait_any(-1, gens);
            }

            public Task<child> wait_any(params child[] gens)
            {
                return wait_any((IEnumerable<child>)gens);
            }

            public async Task<child> timed_wait_any(int ms, IEnumerable<child> gens)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                UnlimitChannel<NotifyToken> waitRemove = new UnlimitChannel<NotifyToken>(_parent.Strand);
                UnlimitChannel<child> waitStop = new UnlimitChannel<child>(_parent.Strand);
                Action<Generator> stopHandler = (Generator host) => waitStop.Post((child)host);
                Action<NotifyToken> removeHandler = (NotifyToken cancelToken) => waitRemove.Post(cancelToken);
                int Count = 0;
                foreach (child ele in gens)
                {
                    Count++;
                    Debug.Assert(null == ele._childNode || ele._childNode.List == _children, "此 child 不属于当前 children!");
                    if (null != ele._childNode && !ele.is_completed())
                    {
                        ele.append_stop_callback(stopHandler, removeHandler);
                    }
                    else
                    {
                        waitStop.Post(ele);
                        waitRemove.Post(new NotifyToken { host = ele });
                        break;
                    }
                }
                try
                {
                    if (0 != Count)
                    {
                        if (ms < 0)
                        {
                            ChannelReceiveWrap<child> gen = await chan_receive(waitStop);
                            if (null != gen.msg._childNode)
                            {
                                _children.Remove(gen.msg._childNode);
                                gen.msg._childNode = null;
                                gen.msg._childrenMgr = null;
                                check_remove_node();
                            }
                            return gen.msg;
                        }
                        else if (0 == ms)
                        {
                            ChannelReceiveWrap<child> gen = await chan_try_receive(waitStop);
                            if (ChannelState.ok == gen.state)
                            {
                                if (null != gen.msg._childNode)
                                {
                                    _children.Remove(gen.msg._childNode);
                                    gen.msg._childNode = null;
                                    gen.msg._childrenMgr = null;
                                    check_remove_node();
                                }
                                return gen.msg;
                            }
                        }
                        else
                        {
                            ChannelReceiveWrap<child> gen = await chan_timed_receive(waitStop, ms);
                            if (ChannelState.ok == gen.state)
                            {
                                if (null != gen.msg._childNode)
                                {
                                    _children.Remove(gen.msg._childNode);
                                    gen.msg._childNode = null;
                                    gen.msg._childrenMgr = null;
                                    check_remove_node();
                                }
                                return gen.msg;
                            }
                        }
                    }
                    return null;
                }
                finally
                {
                    lock_suspend_and_stop();
                    for (int i = 0; i < Count; i++)
                    {
                        NotifyToken node = (await chan_receive(waitRemove)).msg;
                        if (null != node.token)
                        {
                            node.host.remove_stop_callback(node);
                        }
                    }
                    await unlock_suspend_and_stop();
                }
            }

            public Task<child> timed_wait_any(int ms, params child[] gens)
            {
                return timed_wait_any(ms, (IEnumerable<child>)gens);
            }

            public async Task<List<child>> TimedWait(int ms, IEnumerable<child> gens)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                int Count = 0;
                long endTick = SystemTick.GetTickMs() + ms;
                UnlimitChannel<NotifyToken> waitRemove = new UnlimitChannel<NotifyToken>(_parent.Strand);
                UnlimitChannel<child> waitStop = new UnlimitChannel<child>(_parent.Strand);
                Action<Generator> stopHandler = (Generator host) => waitStop.Post((child)host);
                Action<NotifyToken> removeHandler = (NotifyToken cancelToken) => waitRemove.Post(cancelToken);
                foreach (child ele in gens)
                {
                    Count++;
                    Debug.Assert(null == ele._childNode || ele._childNode.List == _children, "此 child 不属于当前 children!");
                    if (null != ele._childNode && !ele.is_completed())
                    {
                        ele.append_stop_callback(stopHandler, removeHandler);
                    }
                    else
                    {
                        waitStop.Post(ele);
                        waitRemove.Post(new NotifyToken { host = ele });
                    }
                }
                try
                {
                    List<child> completedGens = new List<child>(Count);
                    if (ms < 0)
                    {
                        for (int i = 0; i < Count; i++)
                        {
                            ChannelReceiveWrap<child> gen = await chan_receive(waitStop);
                            if (null != gen.msg._childNode)
                            {
                                _children.Remove(gen.msg._childNode);
                                gen.msg._childNode = null;
                                gen.msg._childrenMgr = null;
                            }
                            completedGens.Add(gen.msg);
                        }
                    }
                    else if (0 == ms)
                    {
                        for (int i = 0; i < Count; i++)
                        {
                            ChannelReceiveWrap<child> gen = await chan_try_receive(waitStop);
                            if (ChannelState.ok != gen.state)
                            {
                                break;
                            }
                            if (null != gen.msg._childNode)
                            {
                                _children.Remove(gen.msg._childNode);
                                gen.msg._childNode = null;
                                gen.msg._childrenMgr = null;
                            }
                            completedGens.Add(gen.msg);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Count; i++)
                        {
                            long nowTick = SystemTick.GetTickMs();
                            if (nowTick >= endTick)
                            {
                                break;
                            }
                            ChannelReceiveWrap<child> gen = await chan_timed_receive(waitStop, (int)(endTick - nowTick));
                            if (ChannelState.ok != gen.state)
                            {
                                break;
                            }
                            if (null != gen.msg._childNode)
                            {
                                _children.Remove(gen.msg._childNode);
                                gen.msg._childNode = null;
                                gen.msg._childrenMgr = null;
                            }
                            completedGens.Add(gen.msg);
                        }
                    }
                    check_remove_node();
                    return completedGens;
                }
                finally
                {
                    lock_suspend_and_stop();
                    for (int i = 0; i < Count; i++)
                    {
                        NotifyToken node = (await chan_receive(waitRemove)).msg;
                        if (null != node.token)
                        {
                            node.host.remove_stop_callback(node);
                        }
                    }
                    await unlock_suspend_and_stop();
                }
            }

            public Task<List<child>> TimedWait(int ms, params child[] gens)
            {
                return TimedWait(ms, (IEnumerable<child>)gens);
            }

            public async Task Stop(bool containFree = true)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                if (0 != _children.Count)
                {
                    UnlimitChannel<child> waitStop = new UnlimitChannel<child>(_parent.Strand);
                    Action<Generator> stopHandler = (Generator host) => waitStop.Post((child)host);
                    int Count = 0;
                    for (LinkedListNode<child> it = _children.First; null != it; it = it.Next)
                    {
                        child ele = it.Value;
                        if (!containFree && ele.is_free())
                        {
                            continue;
                        }
                        Count++;
                        if (!ele.is_completed())
                        {
                            ele.Stop(stopHandler);
                        }
                        else
                        {
                            waitStop.Post(ele);
                        }
                    }
                    for (int i = 0; i < Count; i++)
                    {
                        child gen = (await chan_receive(waitStop)).msg;
                        if (null != gen._childNode)
                        {
                            _children.Remove(gen._childNode);
                            gen._childNode = null;
                            gen._childrenMgr = null;
                        }
                    }
                    check_remove_node();
                }
            }

            static public async Task Stop(IEnumerable<children> childrens, bool containFree = true)
            {
                Generator self = Generator.self;
                UnlimitChannel<TupleEx<children, child>> waitStop = new UnlimitChannel<TupleEx<children, child>>(self.Strand);
                int Count = 0;
                foreach (children childs in childrens)
                {
                    Debug.Assert(self == childs._parent, "此 children 不属于当前 Generator!");
                    if (0 != childs._children.Count)
                    {
                        Action<Generator> stopHandler = (Generator host) => waitStop.Post(TupleEx.Make(childs, (child)host));
                        for (LinkedListNode<child> it = childs._children.First; null != it; it = it.Next)
                        {
                            child ele = it.Value;
                            if (!containFree && ele.is_free())
                            {
                                continue;
                            }
                            Count++;
                            if (!ele.is_completed())
                            {
                                ele.Stop(stopHandler);
                            }
                            else
                            {
                                waitStop.Post(TupleEx.Make(childs, ele));
                            }
                        }
                    }
                }
                for (int i = 0; i < Count; i++)
                {
                    TupleEx<children, child> oneRes = (await chan_receive(waitStop)).msg;
                    if (null != oneRes.value2._childNode)
                    {
                        oneRes.value1._children.Remove(oneRes.value2._childNode);
                        oneRes.value2._childNode = null;
                        oneRes.value2._childrenMgr = null;
                        oneRes.value1.check_remove_node();
                    }
                }
            }

            static public Task Stop(params children[] childrens)
            {
                return Stop((IEnumerable<children>)childrens);
            }

            public Task<child> wait_any(bool containFree = false)
            {
                return timed_wait_any(-1, containFree);
            }

            public async Task<child> timed_wait_any(int ms, bool containFree = false)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                UnlimitChannel<NotifyToken> waitRemove = new UnlimitChannel<NotifyToken>(_parent.Strand);
                UnlimitChannel<child> waitStop = new UnlimitChannel<child>(_parent.Strand);
                Action<Generator> stopHandler = (Generator host) => waitStop.Post((child)host);
                Action<NotifyToken> removeHandler = (NotifyToken cancelToken) => waitRemove.Post(cancelToken);
                int Count = 0;
                for (LinkedListNode<child> it = _children.First; null != it; it = it.Next)
                {
                    child ele = it.Value;
                    if (!containFree && ele.is_free())
                    {
                        continue;
                    }
                    Count++;
                    if (ele.is_completed())
                    {
                        waitStop.Post(ele);
                        waitRemove.Post(new NotifyToken { host = ele });
                        break;
                    }
                    ele.append_stop_callback(stopHandler, removeHandler);
                }
                try
                {
                    if (0 != Count)
                    {
                        if (ms < 0)
                        {
                            ChannelReceiveWrap<child> gen = await chan_receive(waitStop);
                            if (null != gen.msg._childNode)
                            {
                                _children.Remove(gen.msg._childNode);
                                gen.msg._childNode = null;
                                gen.msg._childrenMgr = null;
                                check_remove_node();
                            }
                            return gen.msg;
                        }
                        else if (0 == ms)
                        {
                            ChannelReceiveWrap<child> gen = await chan_try_receive(waitStop);
                            if (ChannelState.ok == gen.state)
                            {
                                if (null != gen.msg._childNode)
                                {
                                    _children.Remove(gen.msg._childNode);
                                    gen.msg._childNode = null;
                                    gen.msg._childrenMgr = null;
                                    check_remove_node();
                                }
                                return gen.msg;
                            }
                        }
                        else
                        {
                            ChannelReceiveWrap<child> gen = await chan_timed_receive(waitStop, ms);
                            if (ChannelState.ok == gen.state)
                            {
                                if (null != gen.msg._childNode)
                                {
                                    _children.Remove(gen.msg._childNode);
                                    gen.msg._childNode = null;
                                    gen.msg._childrenMgr = null;
                                    check_remove_node();
                                }
                                return gen.msg;
                            }
                        }
                    }
                    return null;
                }
                finally
                {
                    lock_suspend_and_stop();
                    for (int i = 0; i < Count; i++)
                    {
                        NotifyToken node = (await chan_receive(waitRemove)).msg;
                        if (null != node.token)
                        {
                            node.host.remove_stop_callback(node);
                        }
                    }
                    await unlock_suspend_and_stop();
                }
            }

            public async Task wait_all(bool containFree = true)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                if (0 != _children.Count)
                {
                    UnlimitChannel<child> waitStop = new UnlimitChannel<child>(_parent.Strand);
                    Action<Generator> stopHandler = (Generator host) => waitStop.Post((child)host);
                    int Count = 0;
                    for (LinkedListNode<child> it = _children.First; null != it; it = it.Next)
                    {
                        child ele = it.Value;
                        if (!containFree && ele.is_free())
                        {
                            continue;
                        }
                        Count++;
                        if (!ele.is_completed())
                        {
                            ele.append_stop_callback(stopHandler);
                        }
                        else
                        {
                            waitStop.Post(ele);
                        }
                    }
                    for (int i = 0; i < Count; i++)
                    {
                        child gen = (await chan_receive(waitStop)).msg;
                        if (null != gen._childNode)
                        {
                            _children.Remove(gen._childNode);
                            gen._childNode = null;
                            gen._childrenMgr = null;
                        }
                    }
                    check_remove_node();
                }
            }

            public async Task<List<child>> timed_wait_all(int ms, bool containFree = true)
            {
                Debug.Assert(self == _parent, "此 children 不属于当前 Generator!");
                int Count = 0;
                long endTick = SystemTick.GetTickMs() + ms;
                UnlimitChannel<NotifyToken> waitRemove = new UnlimitChannel<NotifyToken>(_parent.Strand);
                UnlimitChannel<child> waitStop = new UnlimitChannel<child>(_parent.Strand);
                Action<Generator> stopHandler = (Generator host) => waitStop.Post((child)host);
                Action<NotifyToken> removeHandler = (NotifyToken cancelToken) => waitRemove.Post(cancelToken);
                for (LinkedListNode<child> it = _children.First; null != it; it = it.Next)
                {
                    child ele = it.Value;
                    if (!containFree && ele.is_free())
                    {
                        continue;
                    }
                    Count++;
                    if (!ele.is_completed())
                    {
                        ele.append_stop_callback(stopHandler, removeHandler);
                    }
                    else
                    {
                        waitStop.Post(ele);
                        waitRemove.Post(new NotifyToken { host = ele });
                    }
                }
                try
                {
                    List<child> completedGens = new List<child>(Count);
                    if (ms < 0)
                    {
                        for (int i = 0; i < Count; i++)
                        {
                            child gen = (await chan_receive(waitStop)).msg;
                            if (null != gen._childNode)
                            {
                                _children.Remove(gen._childNode);
                                gen._childNode = null;
                                gen._childrenMgr = null;
                            }
                            completedGens.Add(gen);
                        }
                    }
                    else if (0 == ms)
                    {
                        for (int i = 0; i < Count; i++)
                        {
                            ChannelReceiveWrap<child> gen = await chan_try_receive(waitStop);
                            if (ChannelState.ok != gen.state)
                            {
                                break;
                            }
                            if (null != gen.msg._childNode)
                            {
                                _children.Remove(gen.msg._childNode);
                                gen.msg._childNode = null;
                                gen.msg._childrenMgr = null;
                            }
                            completedGens.Add(gen.msg);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Count; i++)
                        {
                            long nowTick = SystemTick.GetTickMs();
                            if (nowTick >= endTick)
                            {
                                break;
                            }
                            ChannelReceiveWrap<child> gen = await chan_timed_receive(waitStop, (int)(endTick - nowTick));
                            if (ChannelState.ok != gen.state)
                            {
                                break;
                            }
                            if (null != gen.msg._childNode)
                            {
                                _children.Remove(gen.msg._childNode);
                                gen.msg._childNode = null;
                                gen.msg._childrenMgr = null;
                            }
                            completedGens.Add(gen.msg);
                        }
                    }
                    check_remove_node();
                    return completedGens;
                }
                finally
                {
                    lock_suspend_and_stop();
                    for (int i = 0; i < Count; i++)
                    {
                        NotifyToken node = (await chan_receive(waitRemove)).msg;
                        if (null != node.token)
                        {
                            node.host.remove_stop_callback(node);
                        }
                    }
                    await unlock_suspend_and_stop();
                }
            }

            public child completed_any(bool containFree = false)
            {
                for (LinkedListNode<child> it = _children.First; null != it; it = it.Next)
                {
                    child ele = it.Value;
                    if (!containFree && ele.is_free())
                    {
                        continue;
                    }
                    if (ele.is_completed())
                    {
                        return ele;
                    }
                }
                return null;
            }

            public List<child> completed_all(bool containFree = true)
            {
                List<child> completedGens = new List<child>(_children.Count);
                for (LinkedListNode<child> it = _children.First; null != it; it = it.Next)
                {
                    child ele = it.Value;
                    if (!containFree && ele.is_free())
                    {
                        continue;
                    }
                    if (ele.is_completed())
                    {
                        completedGens.Add(ele);
                    }
                }
                return completedGens;
            }
        }
    }

    public class WaitGroup
    {
        public struct cancel_token
        {
            internal LinkedListNode<Action> token;
        }

        bool _hasWait;
        int _tasks;
        LinkedList<Action> _waitList;

        public WaitGroup(int initTasks = 0)
        {
            _hasWait = false;
            _tasks = initTasks;
            _waitList = new LinkedList<Action>();
        }

        public void add(int delta = 1)
        {
            if (0 == Interlocked.Add(ref _tasks, delta))
            {
                notify();
            }
        }

        public void notify()
        {
            LinkedList<Action> newList = new LinkedList<Action>();
            Monitor.Enter(this);
            LinkedList<Action> snapList = _waitList;
            _waitList = newList;
            if (_hasWait)
            {
                _hasWait = false;
                Monitor.PulseAll(this);
            }
            Monitor.Exit(this);
            for (LinkedListNode<Action> it = snapList.First; null != it; it = it.Next)
            {
                Functional.CatchInvoke(it.Value);
            }
        }

        public void done()
        {
            add(-1);
        }

        public bool is_done
        {
            get
            {
                return 0 == _tasks;
            }
        }

        public cancel_token async_wait(Action continuation)
        {
            if (is_done)
            {
                Functional.CatchInvoke(continuation);
                return new cancel_token { token = null };
            }
            LinkedListNode<Action> newNode = new LinkedListNode<Action>(continuation);
            Monitor.Enter(this);
            if (is_done)
            {
                Monitor.Exit(this);
                Functional.CatchInvoke(continuation);
                return new cancel_token { token = null };
            }
            else
            {
                _waitList.AddLast(newNode);
                Monitor.Exit(this);
                return new cancel_token { token = newNode };
            }
        }

        public bool cancel_wait(cancel_token cancelToken)
        {
            bool success = false;
            if (null != cancelToken.token && null != cancelToken.token.List)
            {
                Monitor.Enter(this);
                if (cancelToken.token.List == _waitList)
                {
                    _waitList.Remove(cancelToken.token);
                    success = true;
                }
                Monitor.Exit(this);
            }
            return success;
        }

        public Task Wait()
        {
            return Generator.WaitGroup(this);
        }

        public ValueTask<bool> TimedWait(int ms)
        {
            return Generator.timed_wait_group(ms, this);
        }

        public void sync_wait()
        {
            if (is_done)
            {
                return;
            }
            Monitor.Enter(this);
            if (!is_done)
            {
                _hasWait = true;
                Monitor.Wait(this);
            }
            Monitor.Exit(this);
        }

        public bool sync_timed_wait(int ms)
        {
            if (is_done)
            {
                return true;
            }
            bool success = true;
            Monitor.Enter(this);
            if (!is_done)
            {
                _hasWait = true;
                success = Monitor.Wait(this, ms);
            }
            Monitor.Exit(this);
            return success;
        }
    }

    public class WaitGate<R>
    {
        int _tasks;
        int _enterCnt;
        int _cancelCnt;
        ChannelNotifySign _cspSign;
        CspInvokeWrap<R> _result;
        LinkedList<Action> _waitList;
        CspChannel<R, VoidType> _action;

        public WaitGate(CspChannel<R, VoidType> action, int initTasks = 0)
        {
            _enterCnt = 0;
            _cancelCnt = 0;
            _action = action;
            _cspSign = new ChannelNotifySign();
            _tasks = initTasks > 0 ? initTasks : 0;
            _waitList = _tasks > 0 ? new LinkedList<Action>() : null;
        }

        public void Reset(int tasks = -1)
        {
            Debug.Assert(is_exit, "不正确的 Reset 调用!");
            _enterCnt = 0;
            _cancelCnt = 0;
            _tasks = tasks > 0 ? tasks : _tasks;
            _waitList = _tasks > 0 ? new LinkedList<Action>() : null;
        }

        public CspInvokeWrap<R> result
        {
            get
            {
                return _result;
            }
        }

        public bool is_exit
        {
            get
            {
                return null == _waitList;
            }
        }

        private WaitGate.cancel_token Wait(Action continuation)
        {
            if (null == _waitList)
            {
                Functional.CatchInvoke(continuation);
                return new WaitGate.cancel_token { token = null };
            }
            LinkedListNode<Action> newNode = new LinkedListNode<Action>(continuation);
            Monitor.Enter(this);
            if (null != _waitList)
            {
                _waitList.AddLast(newNode);
                Monitor.Exit(this);
                return new WaitGate.cancel_token { token = newNode };
            }
            else
            {
                Monitor.Exit(this);
                Functional.CatchInvoke(continuation);
                return new WaitGate.cancel_token { token = null };
            }
        }

        private void notify()
        {
            Monitor.Enter(this);
            LinkedList<Action> snapList = _waitList;
            _waitList = null;
            Monitor.Exit(this);
            for (LinkedListNode<Action> it = snapList.First; null != it; it = it.Next)
            {
                Functional.CatchInvoke(it.Value);
            }
        }

        public void async_enter()
        {
            async_enter(null);
        }

        public WaitGate.cancel_token async_enter(Action continuation)
        {
            if (_tasks == Interlocked.Increment(ref _enterCnt))
            {
                _action.AsyncSend(delegate (CspInvokeWrap<R> res)
                {
                    _result = res;
                    notify();
                }, default(VoidType), _cspSign);
            }
            else
            {
                Debug.Assert(_enterCnt < _tasks, "异常的 async_enter 调用!");
            }
            if (null == continuation)
            {
                if (_tasks == Interlocked.Increment(ref _cancelCnt) && null != _waitList)
                {
                    _action.AsyncRemoveSendNotify(delegate (ChannelState state)
                    {
                        if (ChannelState.ok == state)
                        {
                            notify();
                        }
                    }, _cspSign);
                }
                return new WaitGate.cancel_token { token = null };
            }
            return Wait(continuation);
        }

        public bool cancel_enter(WaitGate.cancel_token cancelToken)
        {
            if (null != cancelToken.token && null != cancelToken.token.List)
            {
                bool success = false;
                Monitor.Enter(this);
                if (null != _waitList && cancelToken.token.List == _waitList)
                {
                    _waitList.Remove(cancelToken.token);
                    success = true;
                }
                Monitor.Exit(this);
                if (success && _tasks == Interlocked.Increment(ref _cancelCnt) && null != _waitList)
                {
                    _action.AsyncRemoveSendNotify(delegate (ChannelState state)
                    {
                        if (ChannelState.ok == state)
                        {
                            notify();
                        }
                    }, _cspSign);
                    return true;
                }
            }
            return false;
        }

        public void safe_exit(Action continuation)
        {
            Wait(continuation);
        }

        public Task enter()
        {
            return Generator.enter_gate(this);
        }

        public Task<bool> timed_enter(int ms)
        {
            return Generator.timed_enter_gate(ms, this);
        }
    }

    public class WaitGate : WaitGate<VoidType>
    {
        public struct cancel_token
        {
            internal LinkedListNode<Action> token;
        }

        public WaitGate(CspChannel<VoidType, VoidType> action, int initTasks = 0) : base(action, initTasks)
        {
        }
    }
}
