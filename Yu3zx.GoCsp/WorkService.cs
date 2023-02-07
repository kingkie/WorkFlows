using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yu3zx.GoCsp
{
    /// <summary>
    /// 一级调度，使用post将任务提交给一个单事件队列；
    /// 启用一个或多个线程执行run函数，事件队列中的任务将被并行执行；
    /// 这个类可用来执行一些无序任务，类似于线程池。
    /// </summary>
    public class WorkService
    {
        public class WorkGuard : IDisposable
        {
            WorkService _service;

            internal WorkGuard(WorkService Service)
            {
                _service = Service;
                _service.HoldWork();
            }

            public void Dispose()
            {
                _service.ReleaseWork();
            }
        }

        int _work;
        int _waiting;
        volatile bool _runSign;
        /// <summary>
        /// 事件队列
        /// </summary>
        MsgQueue<Action> _opQueue;

        public WorkService()
        {
            _work = 0;
            _waiting = 0;
            _runSign = true;
            _opQueue = new MsgQueue<Action>();
        }
        /// <summary>
        /// 事件队列增加事件
        /// </summary>
        /// <param name="handler"></param>
        public void Post(Action handler)
        {
            MsgQueueNode<Action> newNode = new MsgQueueNode<Action>(handler);
            Monitor.Enter(_opQueue);//线程同步
            _opQueue.AddLast(newNode);
            if (0 != _waiting)
            {
                _waiting--;
                Monitor.Pulse(_opQueue);
            }
            Monitor.Exit(_opQueue);
        }
        /// <summary>
        /// 从队列中取出一个运行
        /// </summary>
        /// <returns></returns>
        public bool RunOne()
        {
            Monitor.Enter(_opQueue);
            if (_runSign && 0 != _opQueue.Count)
            {
                MsgQueueNode<Action> firstNode = _opQueue.First;
                _opQueue.RemoveFirst();
                Monitor.Exit(_opQueue);
                Functional.CatchInvoke(firstNode.Value);
                return true;
            }
            Monitor.Exit(_opQueue);
            return false;
        }
        /// <summary>
        /// 取出队列中的事件运行
        /// </summary>
        /// <returns></returns>
        public long Run()
        {
            long Count = 0;
            while (_runSign)
            {
                Monitor.Enter(_opQueue);
                if (0 != _opQueue.Count)
                {
                    MsgQueueNode<Action> firstNode = _opQueue.First;
                    _opQueue.RemoveFirst();
                    Monitor.Exit(_opQueue);
                    Count++;
                    Functional.CatchInvoke(firstNode.Value);
                }
                else if (0 != _work)
                {
                    _waiting++;
                    Monitor.Wait(_opQueue);
                    Monitor.Exit(_opQueue);
                }
                else
                {
                    Monitor.Exit(_opQueue);
                    break;
                }
            }
            return Count;
        }
        /// <summary>
        /// 停止事件队列运行
        /// 当前运行的事件不会马上停止
        /// </summary>
        public void Stop()
        {
            _runSign = false;
        }
        /// <summary>
        /// 重新开始执行
        /// </summary>
        public void Reset()
        {
            _runSign = true;
        }

        public void HoldWork()
        {
            Interlocked.Increment(ref _work);
        }

        public void ReleaseWork()
        {
            if (0 == Interlocked.Decrement(ref _work))
            {
                Monitor.Enter(_opQueue);
                if (0 != _waiting)
                {
                    _waiting = 0;
                    Monitor.PulseAll(_opQueue);
                }
                Monitor.Exit(_opQueue);
            }
        }

        public int Count
        {
            get
            {
                return _opQueue.Count;
            }
        }

        public WorkGuard Work()
        {
            return new WorkGuard(this);
        }
    }
    /// <summary>
    /// 对work_service的多线程封装，设置好线程数和优先级；
    /// 直接开启run执行即可。不用另外开启线程执行work_service.run()。
    /// </summary>
    public class WorkEngine
    {
        WorkService _service;
        Thread[] _runThreads;

        public WorkEngine()
        {
            _service = new WorkService();
        }
        /// <summary>
        /// 启动线程运行
        /// </summary>
        /// <param name="Threads"></param>
        /// <param name="priority"></param>
        /// <param name="background"></param>
        /// <param name="name"></param>
        public void Run(int Threads = 1, ThreadPriority priority = ThreadPriority.Normal, bool background = false, string name = null)
        {
            lock (this)
            {
                Debug.Assert(null == _runThreads, "WorkEngine 已经运行!");
                _service.Reset();
                _service.HoldWork();
                _runThreads = new Thread[Threads];
                for (int i = 0; i < Threads; ++i)
                {
                    _runThreads[i] = new Thread(() => _service.Run());
                    _runThreads[i].Priority = priority;
                    _runThreads[i].IsBackground = background;
                    _runThreads[i].Name = null == name ? "任务调度" : string.Format("{0}<{1}>", name, i);
                    _runThreads[i].Start();
                }
            }
        }
        /// <summary>
        /// 停止运行
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (null != _runThreads)
                {
                    _service.ReleaseWork();
                    for (int i = 0; i < _runThreads.Length; i++)
                    {
                        _runThreads[i].Join();
                    }
                    _runThreads = null;
                }
            }
        }
        /// <summary>
        /// 强制停止
        /// </summary>
        public void ForceStop()
        {
            _service.Stop();
            Stop();
        }
        /// <summary>
        /// 获取线程总数
        /// </summary>
        public int Threads
        {
            get
            {
                return _runThreads.Length;
            }
        }
        /// <summary>
        /// 服务
        /// </summary>
        public WorkService Service
        {
            get
            {
                return _service;
            }
        }
    }
}
