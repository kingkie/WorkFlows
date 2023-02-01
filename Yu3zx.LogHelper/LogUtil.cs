using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Yu3zx.LogHelper
{
    /// <summary>
    /// 日志类 -- 低速环境下
    /// 队列 可年/月/周/日/大小分割
    /// 调用方法：
    ///  Log.Instance.LogDirectory=@"C:\"; 默认为程序运行目录
    ///  Log.Instance.FileNamePrefix="cxd";默认为log_
    ///  Log.Instance.CurrentMsgType = MsgLevel.Debug;默认为Error
    ///  Log.Instance.logFileSplit = LogFileSplit.Daily; 日志拆分类型LogFileSplit.Sizely 大小
    ///  Log.Instance.MaxFileSize = 5; 默认大小为2M，只有LogFileSplit.Sizely的时候配置有效
    ///  Log.Instance.LogWrite("aa", MsgLevel.Debug);
    /// </summary>
    public class LogUtil : IDisposable
    {
        private static LogUtil _instance = null;
        private static readonly object _synObject = new object();

        public static LogUtil Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (_synObject)
                    {
                        if (null == _instance)
                        {
                            _instance = new LogUtil();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 日志对象的缓存队列
        /// </summary>
        private static Queue<Msg> _msgs;

        /// <summary>
        /// 日志写入线程的控制标记  ture写中|false没有写
        /// </summary>
        private bool _state;

        #region 日志属性

        private string _logDirectory = AppDomain.CurrentDomain.BaseDirectory + "Log\\";
        /// <summary>
        /// 日志文件存放目录,默认根目录下的Log文件夹
        /// </summary>
        public string LogDirectory
        {
            get { return _logDirectory; }
            set 
            { 
                _logDirectory = value; 
            }
        }

        private LogFileSplit _logFileSplit = LogFileSplit.Sizely;
        /// <summary>
        /// 日志拆分类型
        /// </summary>
        public LogFileSplit logFileSplit
        {
            get { return _logFileSplit; }
            set { _logFileSplit = value; }
        }

        private string _fileNamePrefix = "";
        /// <summary>
        /// 日志的前缀名称，默认没有，当日志等级为自定义时才采用
        /// </summary>
        public string FileNamePrefix
        {
            get { return _fileNamePrefix; }
            set { _fileNamePrefix = value; }
        }

        private int _maxFileSize = 5;
        /// <summary>
        /// 单个日志文件默认大小(单位：兆)
        /// </summary>
        public int MaxFileSize
        {
            get { return _maxFileSize; }
            set { _maxFileSize = value; }
        }

        #endregion End

        #region 变量定义
        /// <summary>
        /// 当前负责记录日志文件的名称
        /// </summary>
        private string _currentFileName = "info_" + DateTime.Now.ToString("yyyyMMdd") + "-1.log";
        private string _preFileName = "info_" + DateTime.Now.ToString("yyyyMMdd") + "-1.log";

        /// <summary>
        /// 日志文件生命周期的时间标记
        /// </summary>
        private DateTime _CurrentFileTimeSign = new DateTime();

        /// <summary>
        /// 文件后缀号
        /// </summary>
        private int _fileSymbol = 0;

        /// <summary>
        /// 当前文件大小(单位：B)
        /// </summary>
        private long _fileSize = 0;

        /// <summary>
        /// 日志文件写入流对象
        /// </summary>
        private StreamWriter _writer;

        Dictionary<string, int> dctFileSysbol;
        /// <summary>
        /// 记录当前
        /// </summary>
        private MsgLevel mCntLevel = MsgLevel.Info;

        #endregion End

        /// <summary>
        /// 创建日志对象的新实例,根据指定的日志文件路径和指定的日志文件创建类型
        /// </summary>
        private LogUtil()
        {
            mCntLevel = MsgLevel.Info;
            if (dctFileSysbol == null)
            {
                dctFileSysbol = new Dictionary<string, int>();
                foreach (MsgLevel msgl in Enum.GetValues(typeof(MsgLevel)))
                {
                    if (msgl.ToString() == "Manual")
                    {
                        if (_fileNamePrefix != "")
                        {
                            if (!dctFileSysbol.ContainsKey(_fileNamePrefix))
                            {
                                dctFileSysbol.Add(_fileNamePrefix, 0);
                            }
                        }
                    }
                    else
                    {
                        dctFileSysbol.Add(msgl.ToString(), 0);
                    }
                }
            }
            if (_msgs == null)
            {
                GetCurrentFilename(null);
                _state = true;
                _msgs = new Queue<Msg>();
                Thread thread = new Thread(ProcessLog);
                thread.IsBackground = true;
                thread.Name = "LogDealThread";
                thread.Start();
            }
        }

        //日志文件写入线程执行的方法
        private void ProcessLog()
        {
            try
            {
                while (_state)
                {
                    if (_msgs.Count > 0)
                    {
                        Msg msg = null;
                        lock (_msgs)
                        {
                            msg = _msgs.Dequeue();
                            if (msg != null)
                            {
                                FileWrite(msg);
                            }
                        }
                    }
                    else
                    {
                        //判断是否已经发出终止日志并关闭的消息
                        if (_state)
                        {
                            Thread.Sleep(100);
                        }
                    }
                }
            }
            finally
            {
                FileClose();
            }
        }

        /// <summary>
        /// 根据日志类型获取日志文件名,并同时创建文件到期的时间标记
        /// 通过判断文件的到期时间标记将决定是否创建新文件。
        /// </summary>
        /// <returns>无用</returns>
        private void GetCurrentFilename(Msg _msg)
        {
            DateTime now = DateTime.Now;
            string format = "";
            string filePrefix = "info_";
            filePrefix = GetFilePrefix(_msg);


            switch (_logFileSplit)
            {
                case LogFileSplit.Daily:
                    _CurrentFileTimeSign = new DateTime(now.Year, now.Month, now.Day);
                    _CurrentFileTimeSign = _CurrentFileTimeSign.AddDays(1);
                    _fileSymbol++;
                    format = now.ToString("yyyyMMdd") + "-" + _fileSymbol.ToString() + ".log";
                    break;
                case LogFileSplit.Weekly:
                    _CurrentFileTimeSign = new DateTime(now.Year, now.Month, now.Day);
                    _CurrentFileTimeSign = _CurrentFileTimeSign.AddDays(7);
                    _fileSymbol++;
                    format = now.ToString("yyyyMMdd") + "-" + _fileSymbol.ToString() + ".log";
                    break;
                case LogFileSplit.Monthly:
                    _CurrentFileTimeSign = new DateTime(now.Year, now.Month, 1);
                    _CurrentFileTimeSign = _CurrentFileTimeSign.AddMonths(1);
                    _fileSymbol++;
                    format = now.ToString("yyyyMM") + "-" + _fileSymbol.ToString() + ".log";
                    break;
                case LogFileSplit.Annually:
                    _CurrentFileTimeSign = new DateTime(now.Year, 1, 1);
                    _CurrentFileTimeSign = _CurrentFileTimeSign.AddYears(1);
                    _fileSymbol++;
                    format = now.ToString("yyyy") + "-" + _fileSymbol.ToString() + ".log";
                    break;
                default:
                    _fileSymbol++;
                    format = now.ToString("yyyyMMdd") + "-" + _fileSymbol.ToString() + ".log";
                    break;
            }
            _currentFileName = filePrefix + format.Trim();

            if (File.Exists(Path.Combine(LogDirectory, _currentFileName)))
            {
                _fileSize = new FileInfo(Path.Combine(LogDirectory, _currentFileName)).Length;
            }
            else
            {
                _fileSize = 0;
            }
        }

        private void UpDateDiction(Msg _msg)
        {
            if (_msg.type == MsgLevel.Manual)
            {
                if (_fileNamePrefix != "")
                {
                    if (dctFileSysbol.ContainsKey(_fileNamePrefix))
                    {
                        dctFileSysbol[_fileNamePrefix] = dctFileSysbol[_fileNamePrefix] + 1;
                    }
                    else
                    {
                        dctFileSysbol.Add(_fileNamePrefix, 0);
                    }
                }
                else
                {
                    dctFileSysbol[MsgLevel.Info.ToString()] = dctFileSysbol[MsgLevel.Info.ToString()]+1;
                }
            }
            else
            {
                dctFileSysbol[_msg.type.ToString()] = dctFileSysbol[_msg.type.ToString()] + 1;
            }

            DateTime now = DateTime.Now;
            switch (_logFileSplit)
            {
                case LogFileSplit.Daily:
                    _CurrentFileTimeSign = new DateTime(now.Year, now.Month, now.Day);
                    _CurrentFileTimeSign = _CurrentFileTimeSign.AddDays(1);
                    break;
                case LogFileSplit.Weekly:
                    _CurrentFileTimeSign = new DateTime(now.Year, now.Month, now.Day);
                    _CurrentFileTimeSign = _CurrentFileTimeSign.AddDays(7);
                    break;
                case LogFileSplit.Monthly:
                    _CurrentFileTimeSign = new DateTime(now.Year, now.Month, 1);
                    _CurrentFileTimeSign = _CurrentFileTimeSign.AddMonths(1);
                    break;
                case LogFileSplit.Annually:
                    _CurrentFileTimeSign = new DateTime(now.Year, 1, 1);
                    _CurrentFileTimeSign = _CurrentFileTimeSign.AddYears(1);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_msg"></param>
        private void GetCurrentFileName(Msg _msg)
        {
            DateTime now = DateTime.Now;
            string format = "";
            string filePrefix = "info_";
            filePrefix = GetFilePrefix(_msg);

            string _fileSuffixname = "";

            int _symbol = 0;
            if (_msg.type == MsgLevel.Manual)
            {
                if (_fileNamePrefix != "")
                {
                    if (dctFileSysbol.ContainsKey(_fileNamePrefix))
                    {
                        _symbol = dctFileSysbol[_fileNamePrefix];
                    }
                    else
                    {
                        dctFileSysbol.Add(_fileNamePrefix, _symbol);
                    }
                }
                else
                {
                    _symbol = dctFileSysbol[MsgLevel.Info.ToString()];
                }
            }
            else
            {
                _symbol = dctFileSysbol[_msg.type.ToString()];
            }

            switch (_logFileSplit)
            {
                case LogFileSplit.Daily:
                    _fileSuffixname = now.ToString("yyyyMMdd") + "-" + _symbol.ToString() + ".log";
                    break;
                case LogFileSplit.Weekly:
                    _fileSuffixname = now.ToString("yyyyMMdd") + "-" + _symbol.ToString() + ".log";
                    break;
                case LogFileSplit.Monthly:
                    _fileSuffixname = now.ToString("yyyyMM") + "-" + _symbol.ToString() + ".log";
                    break;
                case LogFileSplit.Annually:
                    _fileSuffixname = now.ToString("yyyy") + "-" + _symbol.ToString() + ".log";
                    break;
                default:
                    _fileSuffixname = now.ToString("yyyyMMdd") + "-" + _symbol.ToString() + ".log";
                    break;
            }
            string _filefullname = filePrefix + _fileSuffixname;
            if (File.Exists(Path.Combine(LogDirectory, _filefullname)))
            {
                _fileSize = new FileInfo(Path.Combine(LogDirectory, _filefullname)).Length;
            }
            else
            {
                _fileSize = 0;
            }
            _currentFileName = _filefullname;
        }
        /// <summary>
        /// 获取文件前缀
        /// </summary>
        /// <param name="_msg"></param>
        /// <returns></returns>
        private string GetFilePrefix(Msg _msg)
        {
            string filePrefix = "info_";
            if (_msg != null)
            {
                switch (_msg.type)
                {
                    case MsgLevel.Comm:
                    case MsgLevel.Debug:
                    case MsgLevel.Err:
                    case MsgLevel.Info:
                    case MsgLevel.Warn:
                        filePrefix = _msg.type.ToString().ToLower()+"_";
                        break;
                    case MsgLevel.Manual:
                        filePrefix = _fileNamePrefix;
                        break;
                    default:
                        filePrefix = "info_";
                        break;
                }
            }
            return filePrefix;
        }

        //写入日志文本到文件的方法
        private void FileWrite(Msg msg)
        {
            try
            {
                if (msg.type == mCntLevel)
                {
                    if (_writer == null)
                    {
                        GetCurrentFileName(msg);
                        FileOpen();
                    }
                }
                else
                {
                    if (_writer != null)
                    {
                        FileClose();
                    }
                    mCntLevel = msg.type;
                    GetCurrentFileName(msg);
                    FileOpen();
                }

                if (_writer != null)
                {
                    //判断文件到期标志,如果当前文件到期则关闭当前文件创建新的日志文件
                    if ((_logFileSplit != LogFileSplit.Sizely
                            && DateTime.Now >= _CurrentFileTimeSign)
                        || (((double)_fileSize / 1048576) > _maxFileSize))
                    {
                        if (((double)_fileSize / 1048576) > _maxFileSize)
                        {
                            while (((double)_fileSize / 1048576) > _maxFileSize)
                            {
                                UpDateDiction(msg);
                                GetCurrentFileName(msg);
                            }
                        }
                        else
                        {
                            FileClose();
                            UpDateDiction(msg);
                        }
                        FileOpen();
                    }
                }
                //============写日志===============
                _writer.Write("[" +msg.datetime.ToString("HH:mm:ss.fff") + "]:");
                _writer.Write('\t');
                _writer.WriteLine(msg.text);
                _fileSize += System.Text.Encoding.UTF8.GetBytes(msg.ToString()).Length;
                _writer.Flush();
            }
            catch (Exception e)
            {
                Console.Out.Write(e);
            }
        }

        //打开文件准备写入
        private void FileOpen()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }
            catch
            {

            }
            _writer = new StreamWriter(LogDirectory + _currentFileName, true, Encoding.UTF8);
        }

        //关闭打开的日志文件
        private void FileClose()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Close();
                _writer.Dispose();
                _writer = null;
            }
        }

        /// <summary>
        /// 写入新日志,根据指定的日志对象Msg
        /// </summary>
        /// <param name="msg">日志内容对象</param>
        private void LogWrite(Msg msg)
        {
            //if (msg.type < CurrentMsgType)
            //    return;
            if (_msgs != null)
            {
                lock (_msgs)
                {
                    _msgs.Enqueue(msg);
                }
            }
        }

        /// <summary>
        /// 写入新日志,根据指定的日志内容和信息类型,采用当前时间为日志时间写入新日志
        /// </summary>
        /// <param name="text">日志内容</param>
        /// <param name="type">信息类型</param>
        public void LogWrite(string text, MsgLevel type)
        {
            LogWrite(new Msg(text, type));
        }


        /// <summary>
        /// 写入新日志,根据指定的日志内容
        /// </summary>
        /// <param name="text">日志内容</param>
        public void LogWrite(string text)
        {
            LogWrite(text, MsgLevel.Debug);
        }

        /// <summary>
        /// 写入新日志,根据指定的日志时间、日志内容和信息类型写入新日志
        /// </summary>
        /// <param name="dt">日志时间</param>
        /// <param name="text">日志内容</param>
        /// <param name="type">信息类型</param>
        public void LogWrite(DateTime dt, string text, MsgLevel type)
        {
            LogWrite(new Msg(dt, text, type));
        }

        /// <summary>
        /// 写入新日志,根据指定的异常类和信息类型写入新日志
        /// </summary>
        /// <param name="e">异常对象</param>
        /// <param name="type">信息类型</param>
        public void LogWrite(Exception e)
        {
            LogWrite(new Msg(e.Message, MsgLevel.Err));
        }

        /// <summary>
        /// 销毁日志对象
        /// </summary>
        public void Dispose()
        {
            _state = false;
        }
    }


    /// <summary>
    /// 一个日志记录的对象
    /// </summary>
    public class Msg
    {
        /// <summary>
        /// 创建新的日志记录实例;日志记录的内容为空,消息类型为MsgType.Unknown,日志时间为当前时间
        /// </summary>
        public Msg()
            : this("", MsgLevel.Info)
        {
        }

        /// <summary>
        /// 创建新的日志记录实例;日志事件为当前时间
        /// </summary>
        /// <param name="t">日志记录的文本内容</param>
        /// <param name="p">日志记录的消息类型</param>
        public Msg(string t, MsgLevel p)
            : this(DateTime.Now, t, p)
        {
        }

        /// <summary>
        /// 创建新的日志记录实例;
        /// </summary>
        /// <param name="dt">日志记录的时间</param>
        /// <param name="t">日志记录的文本内容</param>
        /// <param name="p">日志记录的消息类型</param>
        public Msg(DateTime dt, string t, MsgLevel p)
        {
            datetime = dt;
            type = p;
            text = t;
        }

        /// <summary>
        /// 日志记录的时间
        /// </summary>
        public DateTime datetime { get; set; }

        /// <summary>
        ///日志记录的内容
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// 日志等级
        /// </summary>
        public MsgLevel type { get; set; }

        public new string ToString()
        {
            return datetime.ToString(CultureInfo.InvariantCulture) + "\t" + text + "\n";
        }
    }

    /// <summary>
    /// 日志文件拆分的枚举
    /// </summary>
    /// <remarks>日志类型枚举指示日志文件创建的方式,如果日志比较多可考虑每天创建一个日志文件
    /// 如果日志量比较小可考虑每周、每月或每年创建一个日志文件</remarks>
    public enum LogFileSplit
    {
        /// <summary>
        /// 此枚举指示每天创建一个新的日志文件
        /// </summary>
        Daily,

        /// <summary>
        /// 此枚举指示每周创建一个新的日志文件
        /// </summary>
        Weekly,

        /// <summary>
        /// 此枚举指示每月创建一个新的日志文件
        /// </summary>
        Monthly,

        /// <summary>
        /// 此枚举指示每年创建一个新的日志文件
        /// </summary>
        Annually,

        /// <summary>
        /// 日志文件大小超过指定的创建一个新的日志文件，MaxFileSize指定大小
        /// </summary>
        Sizely
    }

    /// <summary>
    /// 日志等级类型 Debug=0 Infor Warn Error Comm
    /// </summary>
    public enum MsgLevel
    {
        /// <summary>
        /// 调试信息
        /// </summary>
        Debug = 0,
        /// <summary>
        /// 指示普通信息类型的日志记录
        /// </summary>
        Info = 1,
        /// <summary>
        /// 指示警告信息类型的日志记录
        /// </summary>
        Warn = 2,
        /// <summary>
        /// 指示错误信息类型的日志记录
        /// </summary>
        Err = 3,
        /// <summary>
        /// 通信记录信息创建一个新的日志文件夹
        /// </summary>
        Comm = 4,
        /// <summary>
        /// 自定义
        /// </summary>
        Manual = 5
    }

}


