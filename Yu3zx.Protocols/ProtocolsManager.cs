using System.Collections.Generic;

namespace Yu3zx.Protocols
{
    public class ProtocolsManager
    {
        private static object syncObj = new object();
        private static ProtocolsManager instance = null;
        private List<BaseProtocol> protocols = new List<BaseProtocol>();

        public static ProtocolsManager GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new ProtocolsManager();
                }
            }
            return instance;
        }
        /// <summary>
        /// 所有类型的通信协议
        /// </summary>
        public List<BaseProtocol> Protocols
        {
            get
            {
                return protocols;
            }
        }
    }
}
