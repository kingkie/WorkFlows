using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.CommNetwork
{
    /// <summary>
    /// 通信接口
    /// </summary>
    interface IPort : IConnector
    {
        bool Init();
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="buf">缓冲区</param>
        /// <param name="count">读取个数</param>
        /// <param name="bytesread">实际读取个数</param>
        /// <returns></returns>
        bool Read(byte[] buf, int count, ref int bytesread);
        /// <summary>
        /// 写数据
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        bool Write(byte[] buf);
    }
}
