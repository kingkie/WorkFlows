using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.Devices
{
    interface IOperate
    {
        bool Write(byte[] buff);

        bool Read(ref byte[] buf, out int lenth);
    }
}
