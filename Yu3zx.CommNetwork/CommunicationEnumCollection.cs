using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.CommNetwork
{
    public delegate void ThrowUpOnDataReciver(string DevIDs, byte[] buffer); //通信接口接收到数据上抛到设备

    public delegate void OnDataReceiveNotifyHandle(string devIds, byte[] buffer);//通信接口接收到数据上抛到设备

    public delegate void ConnectStateChangeHandle(bool bConnect);

    public delegate void MessageNotifyHandle(string msg, int msgNum = 0);
    /// <summary>
    /// 通信总线类型
    /// </summary>
    public enum BusType
    {
        /// <summary>
        /// 未知，未定义类型
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// 串口通信
        /// </summary>
        CommPort = 1,
        /// <summary>
        /// TCP
        /// </summary>
        Tcp = 2,
        /// <summary>
        /// 
        /// </summary>
        Udp = 3
    }
}
