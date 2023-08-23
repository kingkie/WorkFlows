using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.CommNetwork.Ports
{
    /// <summary>
    /// RF433通信模式
    /// </summary>
    public class RF433Bus : BaseBus
    {
        private Random rd = new Random();

        private SerialPort sp = null;

        //public event ThrowUpOnDataReciver ThrowUpHandle;
        //public override event ThrowUpOnDataReciver ThrowUpHandle;

        private string portName;
        private int baudRate = 115200;
        private int databits = 8;
        private StopBits stopbits = StopBits.One;
        private Parity parity = Parity.None;

        #region 属性区
        /// <summary>
        /// 串口号
        /// </summary>
        public string CommName
        {
            get { return portName; }
            set
            {
                portName = value;
                if (sp != null)
                    sp.PortName = portName;
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

        public RF433Bus()
        {
            if (string.IsNullOrEmpty(this.PortId))
            {
                this.PortId = "RF433Bus" + DateTime.Now.ToString("HHmmssfff") + rd.Next(10000, 99999).ToString();
            }

            if (string.IsNullOrEmpty(this.PortName))
            {
                this.PortName = "RF433(RS232_SP)";
            }

            if (string.IsNullOrEmpty(this.PortDesc))
            {
                this.PortDesc = "RF433通信模块";
            }
        }

        public override bool Init()
        {
            try
            {
                if (sp == null)
                {
                    sp = new SerialPort();
                    sp.PortName = this.CommName;
                    sp.BaudRate = CommBaudRate;
                    sp.DataBits = CommDataBits;
                    sp.Parity = CommParity;
                    sp.StopBits = CommStopBits;
                    sp.DataReceived += Sp_DataReceived; ;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }

        public override bool Open()
        {
            if (sp != null)
            {
                sp.Open();
                return sp.IsOpen;
            }
            else
            {
                return false;
            }
        }

        public override bool Close()
        {
            if (sp != null)
            {
                try
                {
                    sp.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            //上抛数据
            //if (ThrowUpHandle != null)
            //{
            //    if (EquipIDs.Count > 0)
            //    {
            //        ThrowUpHandle.Invoke(EquipIDs[0], buf);
            //    }
            //}
            return true;
        }

        public override bool Write(byte[] data)
        {
            return base.Write(data);
        }

        public override string ToString()
        {
            return this.PortDesc;
        }
    }
}
