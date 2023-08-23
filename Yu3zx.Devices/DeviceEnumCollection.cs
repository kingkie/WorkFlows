using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.Devices
{
    public enum DeviceType : byte
    {
        /// <summary>
        /// 未定义，未知类型
        /// </summary>
        None = 0x00,
        /// <summary>
        /// 
        /// </summary>
        OderMod = 0x01,
        /// <summary>
        /// 
        /// </summary>
        Sync103 = 0x02
    }
}
