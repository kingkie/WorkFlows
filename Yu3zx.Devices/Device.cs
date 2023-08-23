using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using Yu3zx.CommNetwork;
using Yu3zx.InstructModel;
using Yu3zx.Protocols;

namespace Yu3zx.Devices
{
    public abstract class Device : IConnector, IOperate, IComparer
    {
        private Random rd = new Random();

        /// <summary>
        /// 接收指令队列
        /// </summary>
        [JsonIgnore]
        public ConcurrentQueue<OrderItem> InstructQueue = new ConcurrentQueue<OrderItem>();

        public Device()
        {
            this.DevId = "Dev" + DateTime.Now.ToString("HHmmssfff") + rd.Next(10000, 99999).ToString();
        }

        #region 属性区
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DevId
        {
            get;
            set;
        }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DevName
        {
            get;
            set;
        }
        /// <summary>
        /// 设备描述
        /// </summary>
        public string DevDesc
        {
            get;
            set;
        }
        /// <summary>
        /// 使用的接口ID
        /// </summary>
        public string PortID
        {
            get;
            set;
        } = string.Empty;
        /// <summary>
        /// 设备组网序列号
        /// </summary>
        public string GroupID
        {
            get;
            set;
        } = string.Empty;
        /// <summary>
        /// 设备类型
        /// </summary>
        public DeviceType DevType
        {
            get;
            set;
        } = DeviceType.None;

        /// <summary>
        /// 协议
        /// </summary>
        public BaseProtocol Protocol
        {
            get;
            set;
        }
        /// <summary>
        /// 公共通信总线
        /// </summary>
        [JsonIgnore]
        public virtual BaseBus CommBus
        {
            get;
            set;
        }

        [JsonIgnore]
        public virtual bool Connected
        {
            get;
        } = false;


        #endregion End

        public virtual bool Close()
        {
            return true;
        }

        public virtual bool Init()
        {
            BaseBus bb = PortsManager.GetInstance().Ports.Find(x => x.PortId == PortID);
            if (bb != null)
            {
                CommBus = bb;
            }
            return true;
        }

        public virtual bool Open()
        {
            return false;
        }

        public virtual bool Write(byte[] buff)
        {
            return false;
        }

        public virtual bool Read(ref byte[] buf, out int lenth)
        {
            lenth = 0;
            return false;
        }

        public virtual bool OnDataReceive(byte[] dataRev)
        {
            byte[] objIns = null;
            if (Protocol != null)
            {
                if (Protocol.Validity(dataRev, out objIns))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public virtual bool AutoConnect()
        {
            return true;
        }

        public int Compare(object x, object y)
        {
            if ((x is Device) && (y is Device))
            {
                Device a = (Device)x;
                Device b = (Device)y;

                return a.DevId.CompareTo(b.DevId);
            }
            return 0;
        }
    }
}
