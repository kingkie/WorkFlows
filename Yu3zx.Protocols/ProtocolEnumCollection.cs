using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.Protocols
{
    /// <summary>
    /// 设备协议类型
    /// </summary>
    public enum DevProtocolType : byte
    {
        /// <summary>
        /// 未定义，未知类型
        /// </summary>
        None = 0x00,
        /// <summary>
        /// 
        /// </summary>
        HandRemoter = 0x01,
        /// <summary>
        /// 
        /// </summary>
        BoardCard = 0x02,
        /// <summary>
        /// 
        /// </summary>
        SubAdapterCard = 0x03,
        /// <summary>
        /// 
        /// </summary>
        PC = 0x04,
        /// <summary>
        /// 
        /// </summary>
        Headwear = 0x05,
        /// <summary>
        /// 
        /// </summary>
        Sync103 = 0x06,
        /// <summary>
        /// 
        /// </summary>
        Sync429 = 0x07
    }
}
