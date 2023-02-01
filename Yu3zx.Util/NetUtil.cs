using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Yu3zx.Util
{
    public class NetUtil
    {
        public static string[] getLocalIP()
        {
            //初始化本地IP下拉列表
            IPAddress[] ipArr = Dns.GetHostAddresses(Dns.GetHostName());
            List<string> lIPList = new List<string>();
            for (int i = 0; i < ipArr.Length; i++)
            {
                if (ipArr[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    lIPList.Add(ipArr[i].ToString());
                }
            }
            return lIPList.ToArray();
        }
    }
}
