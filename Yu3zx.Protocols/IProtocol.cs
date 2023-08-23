using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yu3zx.InstructModel;

namespace Yu3zx.Protocols
{
    /// <summary>
    /// 通信协议接口
    /// </summary>
    interface IProtocol
    {
        /// <summary>
        /// 验证数据
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        bool Validity(byte[] source, out byte[] dest);
        /// <summary>
        /// 分析器-拆包
        /// </summary>
        /// <param name="data">接收到的数据</param>
        /// <returns>拆开的数据</returns>
        OrderItem UnPackFrame(byte[] data);

        /// <summary>
        /// 装帧
        /// </summary>
        /// <param name="skIns">帧结构数据</param>
        /// <returns></returns>
        byte[] PackFrame(OrderItem skIns);

        //OrderItem AnalyFrame(byte[] data);

    }
}
