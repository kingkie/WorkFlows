using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.Devices
{
    public class ConverterHelper
    {
        public static string Bytes2String(byte[] buf)
        {
            if (buf == null || buf.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                return string.Join(" ", buf.Select(x => x.ToString("X2")));
            }
        }
    }
}
