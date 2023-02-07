using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Yu3zx.GoCsp
{
    public class SystemTick
    {
        [DllImport("NtDll.dll")]
        private static extern int NtQueryTimerResolution(out uint MaximumTime, out uint MinimumTime, out uint CurrentTime);
        [DllImport("NtDll.dll")]
        private static extern int NtSetTimerResolution(uint DesiredTime, uint SetResolution, out uint ActualTime);
        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
        [DllImport("kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long frequency);

        private static SystemTick _pcCycle = new SystemTick();
#if CHECK_STEP_TIMEOUT
        private static volatile bool _checkStepDebugSign = false;
#endif

        private double _sCycle;
        private double _msCycle;
        private double _usCycle;

        private SystemTick()
        {
            long freq;
            if (!QueryPerformanceFrequency(out freq))
            {
                throw new Exception("QueryPerformanceFrequency 初始化失败!");
            }
            _sCycle = 1.0 / (double)freq;
            _msCycle = 1000.0 / (double)freq;
            _usCycle = 1000000.0 / (double)freq;
#if CHECK_STEP_TIMEOUT
            Thread checkStepDebug = new Thread(delegate ()
            {
                long checkTick = GetTickMs();
                while (true)
                {
                    Thread.Sleep(80);
                    long oldTick = checkTick;
                    checkTick = GetTickMs();
                    _checkStepDebugSign = (checkTick - oldTick) > 100;
                }
            });
            checkStepDebug.Priority = ThreadPriority.Highest;
            checkStepDebug.IsBackground = true;
            checkStepDebug.Name = "单步调试检测";
            checkStepDebug.Start();
#endif
        }

        public static void HighResolution()
        {
            uint MaximumTime = 0, MinimumTime = 0, CurrentTime = 0, ActualTime = 0;
            if (0 == NtQueryTimerResolution(out MaximumTime, out MinimumTime, out CurrentTime))
            {
                NtSetTimerResolution(MinimumTime, 1, out ActualTime);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static long GetTick()
        {
            long quadPart;
            QueryPerformanceCounter(out quadPart);
            return quadPart;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static long GetTickUs()
        {
            long quadPart;
            QueryPerformanceCounter(out quadPart);
            return (long)((double)quadPart * _pcCycle._usCycle);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static long GetTickMs()
        {
            long quadPart;
            QueryPerformanceCounter(out quadPart);
            return (long)((double)quadPart * _pcCycle._msCycle);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static long GetTickSec()
        {
            long quadPart;
            QueryPerformanceCounter(out quadPart);
            return (long)((double)quadPart * _pcCycle._sCycle);
        }
        /// <summary>
        /// 微秒
        /// </summary>
        public static long Us
        {
            get
            {
                return GetTickUs();
            }
        }
        /// <summary>
        /// 毫秒
        /// </summary>
        public static long Ms
        {
            get
            {
                return GetTickMs();
            }
        }
        /// <summary>
        /// 秒
        /// </summary>
        public static long Sec
        {
            get
            {
                return GetTickSec();
            }
        }
#if CHECK_STEP_TIMEOUT
        public static bool CheckStepDebugging()
        {
            return _checkStepDebugSign;
        }
#endif
    }

    public static class UtcTick
    {
        [DllImport("kernel32.dll")]
        private static extern void GetSystemTimeAsFileTime(out long time);

        public const long fileTimeOffset = 504911232000000000L;

        public static long GetTick()
        {
            long tm;
            GetSystemTimeAsFileTime(out tm);
            return tm;
        }

        public static long GetTickUs()
        {
            long tm;
            GetSystemTimeAsFileTime(out tm);
            return tm / 10;
        }

        public static long GetTickMs()
        {
            long tm;
            GetSystemTimeAsFileTime(out tm);
            return tm / 10000;
        }

        public static long GetTickSec()
        {
            long tm;
            GetSystemTimeAsFileTime(out tm);
            return tm / 10000000;
        }

        public static long Us
        {
            get
            {
                return GetTickUs();
            }
        }

        public static long Ms
        {
            get
            {
                return GetTickMs();
            }
        }

        public static long Sec
        {
            get
            {
                return GetTickSec();
            }
        }
    }

    public class AsyncTimer
    {
        struct SteadyTimerHandle
        {
            public long absus;
            public long period;
            public MapNode<long, AsyncTimer> node;
        }

        internal class SteadyTimer
        {
            struct waitable_event_handle
            {
                public int id;
                public SteadyTimer steadyTimer;
            }

            class WaitableTimer
            {
                [DllImport("kernel32.dll")]
                private static extern int CreateWaitableTimer(int lpTimerAttributes, int bManualReset, int lpTimerName);
                [DllImport("kernel32.dll")]
                private static extern int SetWaitableTimer(int hTimer, ref long pDueTime, int lPeriod, int pfnCompletionRoutine, int lpArgToCompletionRoutine, int fResume);
                [DllImport("kernel32.dll")]
                private static extern int CancelWaitableTimer(int hTimer);
                [DllImport("kernel32.dll")]
                private static extern int CloseHandle(int hObject);
                [DllImport("kernel32.dll")]
                private static extern int WaitForSingleObject(int hHandle, int dwMilliseconds);

                static public readonly WaitableTimer sysTimer = new WaitableTimer(false);
                static public readonly WaitableTimer utcTimer = new WaitableTimer(true);

                bool _utcMode;
                bool _exited;
                int _timerHandle;
                long _expireTime;
                Thread _timerThread;
                WorkEngine _workEngine;
                Map<long, waitable_event_handle> _eventsQueue;

                WaitableTimer(bool utcMode)
                {
                    _utcMode = utcMode;
                    _exited = false;
                    _expireTime = long.MaxValue;
                    _eventsQueue = new Map<long, waitable_event_handle>(true);
                    _timerHandle = CreateWaitableTimer(0, 0, 0);
                    _workEngine = new WorkEngine();
                    _timerThread = new Thread(TimerThread);
                    _timerThread.Priority = ThreadPriority.Highest;
                    _timerThread.IsBackground = true;
                    _timerThread.Name = _utcMode ? "UTC定时器" : "系统定时器";
                    _workEngine.Run(1, ThreadPriority.Highest, true, _utcMode ? "UTC定时器调度" : "系统定时器调度");
                    _timerThread.Start();
                }

                private void SetTimer()
                {
                    if (_utcMode)
                    {
                        long sleepTime = _expireTime * 10;
                        sleepTime = sleepTime > 0 ? sleepTime : 0;
                        SetWaitableTimer(_timerHandle, ref sleepTime, 0, 0, 0, 0);
                    }
                    else
                    {
                        long sleepTime = -(_expireTime - SystemTick.GetTickUs()) * 10;
                        sleepTime = sleepTime < 0 ? sleepTime : 0;
                        SetWaitableTimer(_timerHandle, ref sleepTime, 0, 0, 0, 0);
                    }
                }

                public void Close()
                {
                    _workEngine.Service.Post(delegate ()
                    {
                        _exited = true;
                        long sleepTime = 0;
                        SetWaitableTimer(_timerHandle, ref sleepTime, 0, 0, 0, 0);
                    });
                    _timerThread.Join();
                    _workEngine.Stop();
                    CloseHandle(_timerHandle);
                }

                public void AppendEvent(long absus, waitable_event_handle eventHandle)
                {
                    _workEngine.Service.Post(delegate ()
                    {
                        if (_exited) return;
                        eventHandle.steadyTimer._waitableNode = _eventsQueue.Insert(absus, eventHandle);
                        if (absus < _expireTime)
                        {
                            _expireTime = absus;
                            SetTimer();
                        }
                    });
                }

                public void RemoveEvent(SteadyTimer steadyTime)
                {
                    _workEngine.Service.Post(delegate ()
                    {
                        if (_exited) return;
                        if (null != steadyTime._waitableNode)
                        {
                            long lastAbsus = steadyTime._waitableNode.Key;
                            _eventsQueue.Remove(steadyTime._waitableNode);
                            steadyTime._waitableNode = null;
                            if (0 == _eventsQueue.Count)
                            {
                                _expireTime = long.MaxValue;
                                CancelWaitableTimer(_timerHandle);
                            }
                            else if (lastAbsus == _expireTime)
                            {
                                _expireTime = _eventsQueue.First.Key;
                                SetTimer();
                            }
                        }
                    });
                }

                public void UpdateEvent(long absus, waitable_event_handle eventHandle)
                {
                    _workEngine.Service.Post(delegate ()
                    {
                        if (_exited) return;
                        if (null != eventHandle.steadyTimer._waitableNode)
                        {
                            _eventsQueue.Insert(_eventsQueue.ReNewNode(eventHandle.steadyTimer._waitableNode, absus, eventHandle));
                        }
                        else
                        {
                            eventHandle.steadyTimer._waitableNode = _eventsQueue.Insert(absus, eventHandle);
                        }
                        long newAbsus = _eventsQueue.First.Key;
                        if (newAbsus < _expireTime)
                        {
                            _expireTime = newAbsus;
                            SetTimer();
                        }
                    });
                }

                private void TimerThread()
                {
                    Action timerHandler = TimerHandler;
                    while (0 == WaitForSingleObject(_timerHandle, -1) && !_exited)
                    {
                        _workEngine.Service.Post(timerHandler);
                    }
                }

                private void TimerHandler()
                {
                    _expireTime = long.MaxValue;
                    while (0 != _eventsQueue.Count)
                    {
                        MapNode<long, waitable_event_handle> First = _eventsQueue.First;
                        long absus = First.Key;
                        long ct = _utcMode ? UtcTick.GetTickUs() : SystemTick.GetTickUs();
                        if (absus > ct)
                        {
                            _expireTime = absus;
                            SetTimer();
                            break;
                        }
                        First.Value.steadyTimer._waitableNode = null;
                        First.Value.steadyTimer.TimerHandler(First.Value.id);
                        _eventsQueue.Remove(First);
                    }
                }
            }

            bool _utcMode;
            bool _looping;
            int _timerCount;
            long _expireTime;
            SharedStrand _strand;
            MapNode<long, waitable_event_handle> _waitableNode;
            Map<long, AsyncTimer> _timerQueue;

            public SteadyTimer(SharedStrand Strand, bool utcMode)
            {
                _utcMode = utcMode;
                _timerCount = 0;
                _looping = false;
                _expireTime = long.MaxValue;
                _strand = Strand;
                _timerQueue = new Map<long, AsyncTimer>(true);
            }

            public void Timeout(AsyncTimer asyncTimer)
            {
                long absus = asyncTimer._timerHandle.absus;
                asyncTimer._timerHandle.node = _timerQueue.Insert(absus, asyncTimer);
                if (!_looping)
                {
                    _looping = true;
                    _expireTime = absus;
                    timer_loop(absus);
                }
                else if (absus < _expireTime)
                {
                    _expireTime = absus;
                    timer_reloop(absus);
                }
            }

            public void Cancel(AsyncTimer asyncTimer)
            {
                if (null != asyncTimer._timerHandle.node)
                {
                    _timerQueue.Remove(asyncTimer._timerHandle.node);
                    asyncTimer._timerHandle.node = null;
                    if (0 == _timerQueue.Count)
                    {
                        _timerCount++;
                        _expireTime = 0;
                        _looping = false;
                        if (_utcMode)
                        {
                            WaitableTimer.utcTimer.RemoveEvent(this);
                        }
                        else
                        {
                            WaitableTimer.sysTimer.RemoveEvent(this);
                        }
                    }
                    else if (asyncTimer._timerHandle.absus == _expireTime)
                    {
                        _expireTime = _timerQueue.First.Key;
                        timer_reloop(_expireTime);
                    }
                }
            }

            public void ReTimeout(AsyncTimer asyncTimer)
            {
                long absus = asyncTimer._timerHandle.absus;
                if (null != asyncTimer._timerHandle.node)
                {
                    _timerQueue.Insert(_timerQueue.ReNewNode(asyncTimer._timerHandle.node, absus, asyncTimer));
                }
                else
                {
                    asyncTimer._timerHandle.node = _timerQueue.Insert(absus, asyncTimer);
                }
                long newAbsus = _timerQueue.First.Key;
                if (!_looping)
                {
                    _looping = true;
                    _expireTime = newAbsus;
                    timer_loop(newAbsus);
                }
                else if (newAbsus < _expireTime)
                {
                    _expireTime = newAbsus;
                    timer_reloop(newAbsus);
                }
            }

            public void TimerHandler(int id)
            {
                if (id != _timerCount)
                {
                    return;
                }
                _strand.Post(delegate ()
                {
                    if (id == _timerCount)
                    {
                        _expireTime = long.MinValue;
                        while (0 != _timerQueue.Count)
                        {
                            MapNode<long, AsyncTimer> First = _timerQueue.First;
                            if (First.Key > (_utcMode ? UtcTick.GetTickUs() : SystemTick.GetTickUs()))
                            {
                                _expireTime = First.Key;
                                timer_loop(_expireTime);
                                return;
                            }
                            else
                            {
                                First.Value._timerHandle.node = null;
                                First.Value.TimerHandler();
                                _timerQueue.Remove(First);
                            }
                        }
                        _looping = false;
                    }
                });
            }

            void timer_loop(long absus)
            {
                if (_utcMode)
                {
                    WaitableTimer.utcTimer.AppendEvent(absus, new waitable_event_handle { id = ++_timerCount, steadyTimer = this });
                }
                else
                {
                    WaitableTimer.sysTimer.AppendEvent(absus, new waitable_event_handle { id = ++_timerCount, steadyTimer = this });
                }
            }

            void timer_reloop(long absus)
            {
                if (_utcMode)
                {
                    WaitableTimer.utcTimer.UpdateEvent(absus, new waitable_event_handle { id = ++_timerCount, steadyTimer = this });
                }
                else
                {
                    WaitableTimer.sysTimer.UpdateEvent(absus, new waitable_event_handle { id = ++_timerCount, steadyTimer = this });
                }
            }
        }

        SharedStrand _strand;
        Action _handler;
        SteadyTimerHandle _timerHandle;
        int _timerCount;
        long _beginTick;
        bool _isInterval;
        bool _onTopCall;
        bool _utcMode;

        public AsyncTimer(SharedStrand Strand, bool utcMode = false)
        {
            _strand = Strand;
            _timerCount = 0;
            _beginTick = 0;
            _isInterval = false;
            _onTopCall = false;
            _utcMode = utcMode;
        }

        public AsyncTimer(bool utcMode = false) : this(SharedStrand.DefaultStrand(), utcMode) { }

        public SharedStrand SelfStrand()
        {
            return _strand;
        }

        private void TimerHandler()
        {
            _onTopCall = true;
            if (_isInterval)
            {
                int lastTc = _timerCount;
                Functional.CatchInvoke(_handler);
                if (lastTc == _timerCount)
                {
                    BeginTimer(_beginTick, _timerHandle.absus += _timerHandle.period, _timerHandle.period);
                }
            }
            else
            {
                Action handler = _handler;
                _handler = null;
                _strand.ReleaseWork();
                Functional.CatchInvoke(handler);
            }
            _onTopCall = false;
        }

        private void BeginTimer(long nowus, long absus, long period)
        {
            _beginTick = nowus;
            if (nowus < absus)
            {
                _timerCount++;
                _timerHandle.absus = absus;
                _timerHandle.period = period;
                if (_utcMode)
                {
                    _strand._utcTimer.Timeout(this);
                }
                else
                {
                    _strand._sysTimer.Timeout(this);
                }
            }
            else
            {
                int tmId = ++_timerCount;
                _timerHandle.absus = absus;
                _timerHandle.period = period;
                _strand.Post(delegate ()
                {
                    if (tmId == _timerCount)
                    {
                        TimerHandler();
                    }
                });
            }
        }

        private void ReBeginTimer(long nowus, long absus, long period)
        {
            _beginTick = nowus;
            if (nowus < absus)
            {
                _timerCount++;
                _timerHandle.absus = absus;
                _timerHandle.period = period;
                if (_utcMode)
                {
                    _strand._utcTimer.ReTimeout(this);
                }
                else
                {
                    _strand._sysTimer.ReTimeout(this);
                }
            }
            else
            {
                if (_utcMode)
                {
                    _strand._utcTimer.Cancel(this);
                }
                else
                {
                    _strand._sysTimer.Cancel(this);
                }
                int tmId = ++_timerCount;
                _timerHandle.absus = absus;
                _timerHandle.period = period;
                _strand.Post(delegate ()
                {
                    if (tmId == _timerCount)
                    {
                        TimerHandler();
                    }
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="us"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public long TimeoutUs(long us, Action handler)
        {
            Debug.Assert(_strand.RunningInThisThread() && null == _handler && null != handler, "不正确的 timeout_us 调用!");
            _isInterval = false;
            _handler = handler;
            _strand.HoldWork();
            long nowus = _utcMode ? UtcTick.GetTickUs() : SystemTick.GetTickUs();
            BeginTimer(nowus, us > 0 ? nowus + us : nowus, us > 0 ? us : 0);
            return nowus;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="us"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public long DeadlineUs(long us, Action handler)
        {
            Debug.Assert(_strand.RunningInThisThread() && null == _handler && null != handler, "不正确的 deadline_us 调用!");
            _isInterval = false;
            _handler = handler;
            _strand.HoldWork();
            long nowus = _utcMode ? UtcTick.GetTickUs() : SystemTick.GetTickUs();
            BeginTimer(nowus, us, us > nowus ? us - nowus : 0);
            return nowus;
        }

        public long Deadline(DateTime date, Action handler)
        {
            if (_utcMode)
            {
                if (DateTimeKind.Utc == date.Kind)
                {
                    return DeadlineUs((date.Ticks - UtcTick.fileTimeOffset) / 10, handler);
                }
                return DeadlineUs((date.Ticks - TimeZoneInfo.Local.BaseUtcOffset.Ticks - UtcTick.fileTimeOffset) / 10, handler);
            }
            else
            {
                if (DateTimeKind.Utc == date.Kind)
                {
                    return TimeoutUs((date.Ticks - DateTime.UtcNow.Ticks) / 10, handler);
                }
                return TimeoutUs((date.Ticks - DateTime.Now.Ticks) / 10, handler);
            }
        }

        public long Timeout(int ms, Action handler)
        {
            return TimeoutUs((long)ms * 1000, handler);
        }

        public long Deadline(long ms, Action handler)
        {
            return DeadlineUs(ms * 1000, handler);
        }

        public long Interval(int ms, Action handler, bool immed = false)
        {
            return IntervalUs((long)ms * 1000, handler, immed);
        }

        public long Interval2(int ms1, int ms2, Action handler, bool immed = false)
        {
            return Interval2Us((long)ms1 * 1000, (long)ms2 * 1000, handler, immed);
        }

        public long IntervalUs(long us, Action handler, bool immed = false)
        {
            return Interval2Us(us, us, handler, immed);
        }

        public long Interval2Us(long us1, long us2, Action handler, bool immed = false)
        {
            Debug.Assert(_strand.RunningInThisThread() && null == _handler && null != handler, "不正确的 interval2_us 调用!");
            _isInterval = true;
            _handler = handler;
            _strand.HoldWork();
            long nowus = _utcMode ? UtcTick.GetTickUs() : SystemTick.GetTickUs();
            BeginTimer(nowus, us1 > 0 ? nowus + us1 : nowus, us2 > 0 ? us2 : 0);
            if (immed)
            {
                Functional.CatchInvoke(_handler);
            }
            return nowus;
        }

        public bool Restart(int ms = -1)
        {
            return RestartUs((long)ms * 1000);
        }

        public bool RestartUs(long us = -1)
        {
            Debug.Assert(_strand.RunningInThisThread(), "不正确的 restart_us 调用!");
            if (null != _handler)
            {
                long nowus = _utcMode ? UtcTick.GetTickUs() : SystemTick.GetTickUs();
                ReBeginTimer(nowus, us < 0 ? nowus + _timerHandle.period : nowus + us, _timerHandle.period);
                return true;
            }
            return false;
        }

        public bool Advance()
        {
            Debug.Assert(_strand.RunningInThisThread(), "不正确的 advance 调用!");
            if (null != _handler)
            {
                if (!_isInterval)
                {
                    _timerCount++;
                    if (_utcMode)
                    {
                        _strand._utcTimer.Cancel(this);
                    }
                    else
                    {
                        _strand._sysTimer.Cancel(this);
                    }
                    _beginTick = 0;
                    Action handler = _handler;
                    _handler = null;
                    Functional.CatchInvoke(handler);
                    _strand.ReleaseWork();
                    return true;
                }
                else if (!_onTopCall)
                {
                    Functional.CatchInvoke(_handler);
                    return true;
                }
            }
            return false;
        }

        public long Cancel()
        {
            Debug.Assert(_strand.RunningInThisThread(), "不正确的 Cancel 调用!");
            if (null != _handler)
            {
                _timerCount++;
                if (_utcMode)
                {
                    _strand._utcTimer.Cancel(this);
                }
                else
                {
                    _strand._sysTimer.Cancel(this);
                }
                long lastBegin = _beginTick;
                _beginTick = 0;
                _handler = null;
                _strand.ReleaseWork();
                return lastBegin;
            }
            return 0;
        }
    }
}
