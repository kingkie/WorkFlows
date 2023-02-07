using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Yu3zx.GoCsp
{
    /// <summary>
    /// 二级调度，使用post将任务提交给一个双缓冲事件队列;
    /// 内部将事件一个接一个的按序抛给Task执行，只有上一个任务完成;
    /// 才开始下一个任务，这样尽管任务可能被分派到不同的线程中;
    /// 但所执行的任务彼此也将是线程安全的。
    /// </summary>
    public class SharedStrand
    {
        protected class CurrentStrand
        {
            public readonly bool work_back_thread;
            public readonly WorkService WorkService;
            public SharedStrand Strand;

            public CurrentStrand(bool workBackThread = false, WorkService workService = null)
            {
                work_back_thread = workBackThread;
                WorkService = workService;
            }
        }
        protected static readonly ThreadLocal<CurrentStrand> _currStrand = new ThreadLocal<CurrentStrand>();
        private static readonly SharedStrand[] _defaultStrand = Functional.Init(delegate ()
        {
            SharedStrand[] strands = new SharedStrand[Environment.ProcessorCount];
            for (int i = 0; i < strands.Length; i++)
            {
                strands[i] = new SharedStrand();
            }
            return strands;
        });

        internal readonly AsyncTimer.SteadyTimer _sysTimer;
        internal readonly AsyncTimer.SteadyTimer _utcTimer;
        internal Generator currSelf = null;
        protected volatile bool _locked;
        protected volatile int _pauseState;
        protected MsgQueue<Action> _readyQueue;
        protected MsgQueue<Action> _waitQueue;
        protected Action _runTask;
        /// <summary>
        /// 
        /// </summary>
        public SharedStrand()
        {
            _locked = false;
            _pauseState = 0;
            _sysTimer = new AsyncTimer.SteadyTimer(this, false);
            _utcTimer = new AsyncTimer.SteadyTimer(this, true);
            _readyQueue = new MsgQueue<Action>();
            _waitQueue = new MsgQueue<Action>();
            _runTask = () => RunTask();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currStrand"></param>
        /// <returns></returns>
        protected bool RunningOneRound(CurrentStrand currStrand)
        {
            currStrand.Strand = this;
            while (0 != _readyQueue.Count)
            {
                if (0 != _pauseState && 0 != Interlocked.CompareExchange(ref _pauseState, 2, 1))
                {
                    currStrand.Strand = null;
                    return false;
                }
                Action stepHandler = _readyQueue.First.Value;
                _readyQueue.RemoveFirst();
                Functional.CatchInvoke(stepHandler);
            }
            MsgQueue<Action> waitQueue = _waitQueue;
            Monitor.Enter(this);
            if (0 != _waitQueue.Count)
            {
                _waitQueue = _readyQueue;
                Monitor.Exit(this);
                _readyQueue = waitQueue;
                currStrand.Strand = null;
                NextOneRound();
            }
            else
            {
                _locked = false;
                Monitor.Exit(this);
                currStrand.Strand = null;
            }
            return true;
        }

        protected virtual void RunTask()
        {
            CurrentStrand currStrand = _currStrand.Value;
            if (null == currStrand)
            {
                currStrand = new CurrentStrand(true);
                _currStrand.Value = currStrand;
            }
            RunningOneRound(currStrand);
        }

        protected virtual void NextOneRound()
        {
            Task.Run(_runTask);
        }

        public int Count
        {
            get
            {
                while (true)
                {
                    MsgQueue<Action> readyQueue = _readyQueue;
                    MsgQueue<Action> waitQueue = _waitQueue;
                    if (readyQueue != waitQueue)
                    {
                        return readyQueue.Count + waitQueue.Count;
                    }
                    Thread.Yield();
                }
            }
        }
        /// <summary>
        /// 增加一个事件
        /// </summary>
        /// <param name="action"></param>
        public void Post(Action action)
        {
            MsgQueueNode<Action> newNode = new MsgQueueNode<Action>(action);
            Monitor.Enter(this);
            if (_locked)
            {
                _waitQueue.AddLast(newNode);
                Monitor.Exit(this);
            }
            else
            {
                _locked = true;
                _readyQueue.AddLast(newNode);
                Monitor.Exit(this);
                NextOneRound();
            }
        }
        /// <summary>
        /// 执行下一个事件
        /// </summary>
        /// <param name="action"></param>
        public void NextDispatch(Action action)
        {
            if (RunningInThisThread())
            {
                _readyQueue.AddFirst(action);
            }
            else
            {
                MsgQueueNode<Action> newNode = new MsgQueueNode<Action>(action);
                Monitor.Enter(this);
                if (_locked) //锁住时添加到等待队列，否则添加到就绪队列
                {
                    _waitQueue.AddFirst(newNode);
                    Monitor.Exit(this);
                }
                else
                {
                    _locked = true;
                    _readyQueue.AddFirst(newNode);
                    Monitor.Exit(this);
                    NextOneRound();
                }
            }
        }

        public void LastDispatch(Action action)
        {
            if (RunningInThisThread())
            {
                _readyQueue.AddLast(action);
            }
            else
            {
                MsgQueueNode<Action> newNode = new MsgQueueNode<Action>(action);
                Monitor.Enter(this);
                if (_locked)
                {
                    _waitQueue.AddLast(newNode);
                    Monitor.Exit(this);
                }
                else
                {
                    _locked = true;
                    _readyQueue.AddLast(newNode);
                    Monitor.Exit(this);
                    NextOneRound();
                }
            }
        }

        public virtual bool Dispatched(Action action)
        {
            CurrentStrand currStrand = _currStrand.Value;
            if (null != currStrand && this == currStrand.Strand)
            {
                Functional.CatchInvoke(action);
                return true;
            }
            else
            {
                MsgQueueNode<Action> newNode = new MsgQueueNode<Action>(action);
                Monitor.Enter(this);
                if (_locked)
                {
                    _waitQueue.AddLast(newNode);
                    Monitor.Exit(this);
                }
                else
                {
                    _locked = true;
                    _readyQueue.AddLast(newNode);
                    Monitor.Exit(this);
                    if (null != currStrand && currStrand.work_back_thread && null == currStrand.Strand)
                    {
                        return RunningOneRound(currStrand);
                    }
                    NextOneRound();
                }
            }
            return false;
        }
        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            Interlocked.CompareExchange(ref _pauseState, 1, 0);
        }
        /// <summary>
        /// 继续
        /// </summary>
        public void Resume()
        {
            if (2 == Interlocked.Exchange(ref _pauseState, 0))
            {
                NextOneRound();
            }
        }

        public bool RunningInThisThread()
        {
            return this == RunningStrand();
        }

        static public SharedStrand RunningStrand()
        {
            CurrentStrand currStrand = _currStrand.Value;
            return null != currStrand ? currStrand.Strand : null;
        }

        static public SharedStrand DefaultStrand()
        {
            CurrentStrand currStrand = _currStrand.Value;
            if (null != currStrand && null != currStrand.Strand)
            {
                return currStrand.Strand;
            }
            return _defaultStrand[RndEx.Global.Next(0, _defaultStrand.Length)];
        }

        static public SharedStrand GlobalStrand()
        {
            return _defaultStrand[RndEx.Global.Next(0, _defaultStrand.Length)];
        }

        static public void NextTick(Action action)
        {
            SharedStrand currStrand = RunningStrand();
            Debug.Assert(null != currStrand, "不正确的 NextTick 调用!");
            currStrand._readyQueue.AddFirst(action);
        }

        static public void LastTick(Action action)
        {
            SharedStrand currStrand = RunningStrand();
            Debug.Assert(null != currStrand, "不正确的 LastTick 调用!");
            currStrand._readyQueue.AddLast(action);
        }

        public void AddNext(Action action)
        {
            Debug.Assert(RunningInThisThread(), "不正确的 AddNext 调用!");
            _readyQueue.AddFirst(action);
        }

        public void AddLast(Action action)
        {
            Debug.Assert(RunningInThisThread(), "不正确的 AddLast 调用!");
            _readyQueue.AddLast(action);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool WaitSafe()
        {
            return !RunningInThisThread();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool ThreadSafe()
        {
            return RunningInThisThread();
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void HoldWork()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void ReleaseWork()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public Action Wrap(Action handler)
        {
            return () => Dispatched(handler);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        public Action<T1> Wrap<T1>(Action<T1> handler)
        {
            return (T1 p1) => Dispatched(() => handler(p1));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        public Action<T1, T2> Wrap<T1, T2>(Action<T1, T2> handler)
        {
            return (T1 p1, T2 p2) => Dispatched(() => handler(p1, p2));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        public Action<T1, T2, T3> Wrap<T1, T2, T3>(Action<T1, T2, T3> handler)
        {
            return (T1 p1, T2 p2, T3 p3) => Dispatched(() => handler(p1, p2, p3));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public Action WrapPost(Action handler)
        {
            return () => Post(handler);
        }

        public Action<T1> WrapPost<T1>(Action<T1> handler)
        {
            return (T1 p1) => Post(() => handler(p1));
        }

        public Action<T1, T2> WrapPost<T1, T2>(Action<T1, T2> handler)
        {
            return (T1 p1, T2 p2) => Post(() => handler(p1, p2));
        }

        public Action<T1, T2, T3> WrapPost<T1, T2, T3>(Action<T1, T2, T3> handler)
        {
            return (T1 p1, T2 p2, T3 p3) => Post(() => handler(p1, p2, p3));
        }
    }
    /// <summary>
    /// 继承自shared_strand，只不过将任务抛给work_service执行。
    /// </summary>
    public class WorkStrand : SharedStrand
    {
        WorkService _service;

        public WorkStrand(WorkService Service) : base()
        {
            _service = Service;
        }

        public WorkStrand(WorkEngine eng) : base()
        {
            _service = eng.Service;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool Dispatched(Action action)
        {
            CurrentStrand currStrand = _currStrand.Value;
            if (null != currStrand && this == currStrand.Strand)
            {
                Functional.CatchInvoke(action);
                return true;
            }
            else
            {
                MsgQueueNode<Action> newNode = new MsgQueueNode<Action>(action);
                Monitor.Enter(this);
                if (_locked)
                {
                    _waitQueue.AddLast(newNode);
                    Monitor.Exit(this);
                }
                else
                {
                    _locked = true;
                    _readyQueue.AddLast(newNode);
                    Monitor.Exit(this);
                    if (null != currStrand && _service == currStrand.WorkService && null == currStrand.Strand)
                    {
                        return RunningOneRound(currStrand);
                    }
                    NextOneRound();
                }
            }
            return false;
        }

        protected override void RunTask()
        {
            CurrentStrand currStrand = _currStrand.Value;
            if (null == currStrand)
            {
                currStrand = new CurrentStrand(false, _service);
                _currStrand.Value = currStrand;
            }
            RunningOneRound(currStrand);
            _service.ReleaseWork();
        }

        protected override void NextOneRound()
        {
            _service.HoldWork();
            _service.Post(_runTask);
        }

        public override void HoldWork()
        {
            _service.HoldWork();
        }

        public override void ReleaseWork()
        {
            _service.ReleaseWork();
        }
    }
    /// <summary>
    /// 继承自shared_strand，只不过将任务抛给Control控件;
    /// 任务将在UI线程中执行。这个对象一个进程只能创建一个.
    /// </summary>
#if !NETCORE
    public class ControlStrand : SharedStrand
    {
        public class RepeatException : System.Exception
        {
            private ControlStrand _strand;

            internal RepeatException(ControlStrand Strand)
            {
                _strand = Strand;
            }

            public ControlStrand Strand
            {
                get
                {
                    return _strand;
                }
            }
        }

        static readonly ThreadLocal<ControlStrand> _UIThreadOnlyOneStrand = new ThreadLocal<ControlStrand>();
        System.Windows.Forms.Control _ctrl;
        bool _checkRequired;

        public ControlStrand(System.Windows.Forms.Control ctrl, bool checkRequired = true) : base()
        {
            ControlStrand checkUIStrand = _UIThreadOnlyOneStrand.Value;
            if (null != checkUIStrand)
            {
                throw new RepeatException(checkUIStrand);
            }
            _UIThreadOnlyOneStrand.Value = this;
            _checkRequired = checkRequired;
            _ctrl = ctrl;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool Dispatched(Action action)
        {
            CurrentStrand currStrand = _currStrand.Value;
            if (null != currStrand && this == currStrand.Strand)
            {
                Functional.CatchInvoke(action);
                return true;
            }
            else
            {
                MsgQueueNode<Action> newNode = new MsgQueueNode<Action>(action);
                Monitor.Enter(this);
                if (_locked)
                {
                    _waitQueue.AddLast(newNode);
                    Monitor.Exit(this);
                }
                else
                {
                    _locked = true;
                    _readyQueue.AddLast(newNode);
                    Monitor.Exit(this);
                    if (_checkRequired && null != currStrand && null == currStrand.Strand && !_ctrl.InvokeRequired)
                    {
                        return RunningOneRound(currStrand);
                    }
                    NextOneRound();
                }
            }
            return false;
        }

        protected override void RunTask()
        {
            CurrentStrand currStrand = _currStrand.Value;
            if (null == currStrand)
            {
                currStrand = new CurrentStrand();
                _currStrand.Value = currStrand;
            }
            RunningOneRound(currStrand);
        }

        protected override void NextOneRound()
        {
            try
            {
                _ctrl.BeginInvoke(_runTask);
            }
            catch (System.InvalidOperationException ec)
            {
                Trace.Fail(ec.Message, ec.StackTrace);
            }
        }

        public override bool WaitSafe()
        {
            return _ctrl.InvokeRequired;
        }

        public override bool ThreadSafe()
        {
            return !_ctrl.InvokeRequired;
        }
    }
#endif
}
