using Yu3zx.InstructModel;

namespace Yu3zx.Protocols
{
    public abstract class BaseProtocol : IProtocol
    {
        /// <summary>
        /// 通信接口ID
        /// </summary>
        public string ProtocolId
        {
            get;
            set;
        }
        /// <summary>
        /// 通道名称
        /// </summary>
        public string ProtocolName
        {
            get;
            set;
        }

        /// <summary>
        /// 是否有本机地址
        /// </summary>
        public bool HaveLocalAddr
        {
            get;
            set;
        }
        /// <summary>
        /// 本机设备地址
        /// </summary>
        public int LocalAddr
        {
            get;
            set;
        }
        /// <summary>
        /// 是否有远程地址
        /// </summary>
        public bool HaveRemoteAddr
        {
            get;
            set;
        }
        /// <summary>
        /// 远程设备地址
        /// </summary>
        public int RemoteAddr
        {
            get;
            set;
        }

        public virtual bool Validity(byte[] source, out byte[] dest)
        {
            dest = new byte[] { };
            return true;
        }

        public abstract byte[] PackFrame(OrderItem instruction);

        public abstract OrderItem UnPackFrame(byte[] data);
    }
}
