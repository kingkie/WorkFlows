using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.CommNetwork
{
    /// <summary>
    /// 通信基类
    /// abstract修饰的只能重写；
    /// virtual(只能修饰方法和属性)的可以重写也可以使用基类的方法
    /// </summary>
    public abstract class BaseBus : IPort, IComparer
    {
        private List<string> deviceids = new List<string>(); //绑定的设备

        /// <summary>
        /// 数据接收通知事件
        /// </summary>
        public event OnDataReceiveNotifyHandle DataReceiveHandle;
        /// <summary>
        /// 连接状态通知事件
        /// </summary>
        public event ConnectStateChangeHandle ConnectStateChangeHandle;
        /// <summary>
        /// 
        /// </summary>
        public event MessageNotifyHandle MessageNtifyHandle;
        /// <summary>
        /// 通信接口ID
        /// </summary>
        public string PortId
        {
            get;
            set;
        }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string PortName
        {
            get;
            set;
        }

        /// <summary>
        /// 通信接口描述
        /// </summary>
        public string PortDesc
        {
            get;
            set;
        }

        /// <summary>
        /// 是否同步
        /// </summary>
        [JsonIgnore]
        public virtual bool SyncOrNo
        {
            get;
            set;
        } = false;

        [JsonIgnore]
        public virtual bool Connected
        {
            get;
        } = false;

        [JsonIgnore]
        public List<string> EquipIds
        {
            get { return deviceids; }
        }

        /// <summary>
        /// 不序列化
        /// </summary>
        [JsonIgnore]
        public List<byte> ReceiveBuffers = new List<byte>();

        public BaseBus()
        {

        }

        #region 设备绑定与解绑
        /// <summary>
        /// 增加设备绑定
        /// </summary>
        /// <param name="devId"></param>
        /// <returns></returns>
        public virtual bool AddDeviceBind(string devId)
        {
            if (!string.IsNullOrEmpty(devId))
            {
                if (!deviceids.Contains(devId))
                {
                    deviceids.Add(devId);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 增加设备绑定
        /// </summary>
        /// <param name="devId"></param>
        /// <returns></returns>
        public virtual bool AddDeviceBind(List<string> devId)
        {
            return true;
        }

        /// <summary>
        /// 移除设备绑定
        /// </summary>
        /// <param name="devId"></param>
        /// <returns></returns>
        public virtual bool RemoveDevicesBind(string devId)
        {
            if (!string.IsNullOrEmpty(devId))
            {
                if (deviceids.Contains(devId))
                {
                    deviceids.Remove(devId);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 移除设备绑定
        /// </summary>
        /// <param name="devId"></param>
        /// <returns></returns>
        public virtual bool RemoveDevicesBind(List<string> devIds)
        {
            return true;
        }

        public virtual bool CanFindBindD(string devId)
        {
            if (deviceids != null && deviceids.Count > 0 && !string.IsNullOrEmpty(devId))
            {
                return deviceids.Contains(devId);
            }
            else
            {
                return false;
            }
        }
        #endregion End

        #region IPort接口实现

        public virtual bool Close()
        {
            return true;
        }

        public virtual bool Open()
        {
            return false; //默认没有打开
        }

        public virtual bool AutoConnect()
        {
            return true;
        }

        public virtual bool Init()
        {
            return true;
        }


        public virtual bool Write(byte[] buf)
        {
            return false;
        }

        public virtual bool Read(byte[] buf, int count, ref int bytesread)
        {
            return false;
        }
        #endregion End

        #region 排序接口实现
        public int Compare(object x, object y)
        {
            if ((x is BaseBus) && (y is BaseBus))
            {
                BaseBus a = (BaseBus)x;
                BaseBus b = (BaseBus)y;

                return a.PortId.CompareTo(b.PortId);
            }
            return 0;
        }

        #endregion End

        protected virtual void ConnectStateChange(bool bConnected)
        {
            if (ConnectStateChangeHandle != null)
            {
                ConnectStateChangeHandle.Invoke(bConnected);
            }
        }

        protected virtual void OnDataReceive(string IDs, byte[] buf)
        {
            if (DataReceiveHandle != null)
            {
                DataReceiveHandle.Invoke(IDs, buf);
            }
        }

        protected virtual void MessageNotify(string msg, int msgnum)
        {
            MessageNtifyHandle?.Invoke(msg, msgnum);
        }
    }
}
