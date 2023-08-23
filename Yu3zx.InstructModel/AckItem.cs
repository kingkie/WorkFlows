using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.InstructModel
{
    public class AckItem
    {
        /// <summary>
        /// 指令码-以字符串形式表示兼容各类指令码
        /// </summary>
        public int CmdCode
        {
            get;
            set;
        }

        public int AckNum
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
        /// 远程设备地址
        /// </summary>
        public int RemoteAddr
        {
            get;
            set;
        }
        /// <summary>
        /// 帧数据-承载数据
        /// </summary>
        public List<byte> Payload
        {
            get;
            set;
        } = new List<byte>();
    }
}
