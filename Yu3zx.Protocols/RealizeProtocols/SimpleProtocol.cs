using System;
using Yu3zx.InstructModel;

namespace Yu3zx.Protocols.RealizeProtocols
{
    /// <summary>
    /// 简单无帧头帧尾协议
    /// </summary>
    public class SimpleProtocol : BaseProtocol
    {
        private Random rd = new Random();

        public SimpleProtocol()
        {
            if (string.IsNullOrEmpty(this.ProtocolId))
            {
                this.ProtocolId = "P" + DateTime.Now.Millisecond.ToString("X2") + rd.Next(10000, 99999).ToString();
            }

            if (string.IsNullOrEmpty(this.ProtocolName))
            {
                this.ProtocolName = "无帧头帧尾简单协议";
            }
        }

        public override byte[] PackFrame(OrderItem instruction)
        {
            throw new NotImplementedException();
        }

        public override OrderItem UnPackFrame(byte[] data)
        {
            return null;
        }
    }
}
