using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yu3zx.LogHelper;

namespace Yu3zx.CommNetwork.Ports
{
    /// <summary>
    /// 串口(RS232)
    /// </summary>
    public class SingleCommBus : BaseBus
    {
        private Random rd = new Random();

        private SerialPort sp = null;

        private string commName;
        private int baudRate = 115200;
        private int databits = 8;
        private StopBits stopbits = StopBits.One;
        private Parity parity = Parity.None;

        private List<string> lPorts = new List<string>(); //获取串口数组

        private bool haveInit = false;
        #region 属性区

        /// <summary>
        /// 串口号
        /// </summary>
        public string CommName
        {
            get { return commName; }
            set
            {
                commName = value;
                if (sp != null && !sp.IsOpen)
                    sp.PortName = commName;
            }
        }
        /// <summary>
        /// 波特率
        /// </summary>
        public int CommBaudRate
        {
            get { return baudRate; }
            set
            {
                baudRate = value;
                if (sp != null)
                    sp.BaudRate = baudRate;
            }
        }
        /// <summary>
        /// 数据位
        /// </summary>
        public int CommDataBits
        {
            get { return databits; }
            set
            {
                databits = value;
                if (sp != null)
                    sp.DataBits = databits;
            }
        }
        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits CommStopBits
        {
            get { return stopbits; }
            set
            {
                stopbits = value;
                if (sp != null)
                    sp.StopBits = stopbits;
            }
        }
        /// <summary>
        /// 校验方式
        /// </summary>
        public Parity CommParity
        {
            get { return parity; }
            set
            {
                parity = value;
                if (sp != null)
                    sp.Parity = parity;
            }
        }
        /// <summary>
        /// 连接
        /// </summary>
        [JsonIgnore]
        public override bool Connected
        {
            get
            {
                if (sp == null)
                {
                    return false;
                }
                else
                {
                    return sp.IsOpen;
                }
            }
        }

        #endregion 属性区

        public SingleCommBus()
        {
            if (string.IsNullOrEmpty(this.PortId))
            {
                this.PortId = "CommPort" + DateTime.Now.ToString("HHmmssfff") + rd.Next(10000, 99999).ToString();
            }
            if (string.IsNullOrEmpty(this.PortName))
            {
                this.PortName = "串口(RS232)_SP";
            }
            if (string.IsNullOrEmpty(this.PortDesc))
            {
                this.PortName = "串口通信(RS232),单个设备通信";
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            try
            {
                if (sp != null)
                {
                    try
                    {
                        sp.DataReceived -= Sp_DataReceived;
                        sp.Close();
                        //sp.Dispose();
                        //sp = null;
                    }
                    catch
                    { }
                }
                else
                {
                    sp = new SerialPort();
                }

                if (!haveInit)
                {
                    lPorts.Clear();
                    lPorts.AddRange(SerialPort.GetPortNames());
                    haveInit = true;
                }

                if (lPorts.Count > 0 && !lPorts.Contains(this.CommName))
                {
                    this.CommName = lPorts[0];
                }
                else
                {
                    return false;
                }

                sp.PortName = this.CommName;
                sp.BaudRate = CommBaudRate;
                sp.DataBits = CommDataBits;
                sp.Parity = CommParity;
                sp.StopBits = CommStopBits;
                sp.DtrEnable = true;
                //sp.RtsEnable = true;
                sp.DataReceived += Sp_DataReceived;

                sp.Open();
                return sp.IsOpen;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 与Init不需要
        /// </summary>
        /// <returns></returns>
        public override bool AutoConnect()
        {
            try
            {
                if (sp == null)
                {
                    haveInit = false;
                    this.Init();
                }
                else
                {
                    sp.Close();
                }
                if (lPorts.Count > 0)
                {
                    if (lPorts.Contains(this.CommName))
                    {
                        lPorts.Remove(this.CommName); //这次使用现在的，然后在移除已经存在的
                    }
                    else
                    {
                        this.CommName = lPorts[0]; //使用剩下的串口号
                        if (lPorts.Contains(this.CommName))
                        {
                            lPorts.Remove(this.CommName);
                        }
                    }

                    if (sp != null && !sp.IsOpen)
                    {
                        try
                        {
                            sp.DataReceived -= Sp_DataReceived;
                            sp.Close();
                        }
                        catch
                        { }
                        sp.PortName = this.CommName;
                        sp.DtrEnable = true;
                        //sp.RtsEnable = true;
                        sp.DataReceived += Sp_DataReceived;
                        sp.Open();
                        Console.WriteLine("注册事件成功214");
                    }
                    else
                    {
                        if (sp != null)
                        {
                            try
                            {
                                sp.DataReceived -= Sp_DataReceived;
                                sp.Close();
                            }
                            catch
                            { }

                            sp.PortName = this.CommName;
                            sp.DtrEnable = true;
                            sp.RtsEnable = true;
                            sp.DataReceived += Sp_DataReceived;
                            Console.WriteLine("注册事件成功230");
                            sp.Open();
                        }
                        else
                        {
                            haveInit = false;
                            this.Init();
                            sp.Open();
                        }
                    }
                    if (sp.IsOpen)
                    {
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "打开串口：" + sp.PortName + ";" + sp.BaudRate.ToString());
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (sp != null && !sp.IsOpen)
                    {
                        try
                        {
                            sp.DataReceived -= Sp_DataReceived;
                            sp.Close();
                        }
                        catch
                        { }
                        sp.PortName = this.CommName;
                        sp.DtrEnable = true;
                        //sp.RtsEnable = true;
                        sp.DataReceived += Sp_DataReceived;
                        sp.Open();
                        Console.WriteLine("注册事件成功274");
                    }
                    else
                    {
                        if (sp != null)
                        {
                            try
                            {
                                sp.DataReceived -= Sp_DataReceived;
                                sp.Close();
                            }
                            catch
                            { }

                            sp.PortName = this.CommName;
                            sp.DtrEnable = true;
                            //sp.RtsEnable = true;
                            sp.DataReceived += Sp_DataReceived;
                            Console.WriteLine("注册事件成功290");
                            sp.Open();
                        }
                        else
                        {
                            haveInit = false;
                            this.Init();
                            sp.Open();
                        }
                    }
                    lPorts.AddRange(SerialPort.GetPortNames());//重新获取系统所有串口列表

                    if (sp.IsOpen)
                    {
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "打开串口：" + sp.PortName + ";" + sp.BaudRate.ToString());
                        return true;
                    }
                    else
                    {
                        MessageNotify("已经尝试过所有串口了", 1);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                LogUtil.Instance.LogWrite(ex);
            }
            return false;
        }
        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(10);//
                int len = sp.BytesToRead;
                if (len == 0)
                {
                    return;
                }
                byte[] buff = new byte[len];
                int haveRead = sp.Read(buff, 0, len);
                ReceiveBuffers.Clear();
                ReceiveBuffers.AddRange(buff);
                if (haveRead > 0)
                {
                    if (EquipIds.Count > 0)
                    {
                        OnDataReceive(EquipIds[0], buff);
                    }
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// 打开
        /// </summary>
        /// <returns></returns>
        public override bool Open()
        {
            try
            {
                if (sp != null)
                {
                    if (!sp.IsOpen)
                    {
                        sp.Open();
                    }
                    ConnectStateChange(sp.IsOpen);
                    return sp.IsOpen;
                }
                else
                {
                    ConnectStateChange(false);
                    return false;
                }
            }
            catch
            {
            }
            return false;
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        public override bool Close()
        {
            if (sp != null)
            {
                sp.Close();
                return !sp.IsOpen;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="count"></param>
        /// <param name="bytesread"></param>
        /// <returns></returns>
        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            //上抛数据
            if (EquipIds.Count > 0)
            {
                OnDataReceive(EquipIds[0], buf);
            }
            return true;
        }

        public override bool Write(byte[] data)
        {
            try
            {
                ReceiveBuffers.Clear();
                if (!(data != null && data.Length > 0))
                {
                    return false;
                }

                if (sp != null && sp.IsOpen)
                {
                    sp.Write(data, 0, data.Length);
                }
                else
                {
                    if (sp != null)
                    {
                        try
                        {
                            sp.Open();
                            sp.Write(data, 0, data.Length);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return this.PortDesc;
        }

        #region 事件重写
        protected override void OnDataReceive(string IDs, byte[] buf)
        {
            base.OnDataReceive(IDs, buf);
        }

        protected override void ConnectStateChange(bool bConnected)
        {
            base.ConnectStateChange(bConnected);
        }

        #endregion End
    }
}
