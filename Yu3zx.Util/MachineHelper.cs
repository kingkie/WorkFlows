using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Management;
using System.Net;

namespace Yu3zx.Util
{
    public static class MachineHelper
    {
        private static PerformanceCounter CpuPerformanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private static PerformanceCounter MemPerformanceCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use", "");//Available MBytes

        #region 获取主机名称
        /// <summary>
        /// 获取本地机器名称
        /// </summary>
        /// <returns>机器名称</returns>
        public static string GetHostName()
        {
            string hostName = "";
            try
            {
                hostName = Dns.GetHostName();
                return hostName;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        /// <summary>
        /// 获取本机IP地址，使用枚举器
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetHostIP()
        {
            System.Net.IPAddress[] addressList = Dns.GetHostByName(Dns.GetHostName()).AddressList;
            foreach(IPAddress ipaddr in addressList)
            {
                if (ipaddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    yield return ipaddr.ToString();
            }
        }
        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns></returns>
        //public static List<string> GetHostIP()
        //{
        //    List<string> lrtn = new List<string>();
        //    System.Net.IPAddress[] addressList = Dns.GetHostByName(Dns.GetHostName()).AddressList;
        //    foreach (IPAddress ipaddr in addressList)
        //    {
        //        if (ipaddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        //            lrtn.Add(ipaddr.ToString());
        //    }
        //    return lrtn;
        ////}
        
        #region GetMacAddress
        /// <summary>
        /// GetMacAddress 获取网卡mac地址
        /// </summary>        
        public static IList<string> GetMacAddress()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            IList<string> strArr = new List<string>();

            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"])
                {
                    strArr.Add(mo["MacAddress"].ToString().Replace(":", ""));
                }
                mo.Dispose();
            }
            mc.Dispose();
            return strArr;
        }
        #endregion

        #region IsCurrentMachine
        public static bool IsCurrentMachine(string macAddress)
        {
            IList<string> addList = MachineHelper.GetMacAddress();
            return addList.Contains(macAddress);
        }
        #endregion

        #region GetPhysicalMemorySize
        /// <summary>
        /// 获取物理内存大小
        /// </summary>
        /// <returns></returns>
        public static ulong GetPhysicalMemorySize()
        {
            ulong PhysicalMemorySize = 0, FreePhysicalMemory = 0;
            ManagementClass osClass = new ManagementClass("Win32_OperatingSystem");
            foreach (ManagementObject obj in osClass.GetInstances())
            {
                if (obj["TotalVisibleMemorySize"] != null)
                    PhysicalMemorySize = (ulong)obj["TotalVisibleMemorySize"];

                if (obj["FreePhysicalMemory"] != null)
                    FreePhysicalMemory = (ulong)obj["FreePhysicalMemory"];
                break;
            }
            osClass.Dispose();
            return PhysicalMemorySize;

        }
        #endregion

        #region GetCpuInfo
        /// <summary>
        /// 获取CPU信息
        /// </summary>        
        public static List<CpuInfo> GetCpuInfo()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_Processor");
            ManagementObjectCollection list = searcher.Get();
            uint count = 0;
            foreach (ManagementObject obj2 in list)
            {
                ++count;
            }
            List<CpuInfo> cpuList = new List<CpuInfo>();
            foreach (ManagementObject obj2 in list)
            {
                cpuList.Add(new CpuInfo(obj2.GetPropertyValue("Name").ToString(), (uint)obj2.GetPropertyValue("CurrentClockSpeed"), (uint)(Environment.ProcessorCount / count)));
            }
            return cpuList;
        }
        #endregion

        #region GetPerformanceUsage
        /// <summary>
        /// GetPerformanceUsage 获取性能参数。
        /// </summary>
        /// <param name="cpuUsage">CPU利用率。</param>
        /// <param name="memoryUsage">物理内存利用率</param>
        public static void GetPerformanceUsage(out float cpuUsage, out float memoryUsage)
        {
            cpuUsage = MachineHelper.CpuPerformanceCounter.NextValue();
            ulong PhysicalMemorySize = 0, FreePhysicalMemory = 0;
            ManagementClass osClass = new ManagementClass("Win32_OperatingSystem");
            foreach (ManagementObject obj in osClass.GetInstances())
            {
                if (obj["TotalVisibleMemorySize"] != null)
                    PhysicalMemorySize = (ulong)obj["TotalVisibleMemorySize"];

                if (obj["FreePhysicalMemory"] != null)
                    FreePhysicalMemory = (ulong)obj["FreePhysicalMemory"];
                break;
            }
            osClass.Dispose();

            if (PhysicalMemorySize == 0)
            {
                memoryUsage = 0;
            }
            else
            {
                memoryUsage = (PhysicalMemorySize - FreePhysicalMemory) * 100 / PhysicalMemorySize;
            }
        }
        #endregion

        #region GetDiskFreeSpace
        [DllImport("kernel32.dll")]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out UInt64 lpFreeBytesAvailable, out UInt64 lpTotalNumberOfBytes, out UInt64 lpTotalNumberOfFreeBytes);
        /// <summary>
        /// 获取磁盘的可用空间大小。
        /// </summary>
        /// <param name="diskName">磁盘的名称。如"C:\"</param>
        /// <returns>磁盘的剩余控件</returns>      
        public static ulong GetDiskFreeSpace(string diskName)
        {
            ulong freeBytesAvailable = 0;
            ulong totalNumberOfBytes = 0;
            ulong totalNumberOfFreeBytes = 0;

            GetDiskFreeSpaceEx(diskName, out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes);
            return freeBytesAvailable;
        }
        #endregion

        #region IsNetworkConnected2
        /// <summary>
        /// 可以及时反应网络连通情况，但是需要服务System Event Notification支持（系统默认自动启动该服务）
        /// </summary>       
        public static bool IsNetworkConnected()
        {
            int flags;//上网方式 
            bool online = IsNetworkAlive(out flags);
            return online;

            #region details
            int NETWORK_ALIVE_LAN = 0;
            int NETWORK_ALIVE_WAN = 2;
            int NETWORK_ALIVE_AOL = 4;
            string outPut = null;
            if (online)//在线   
            {
                if ((flags & NETWORK_ALIVE_LAN) == NETWORK_ALIVE_LAN)
                {
                    outPut = "在线：NETWORK_ALIVE_LAN\n";
                }
                if ((flags & NETWORK_ALIVE_WAN) == NETWORK_ALIVE_WAN)
                {
                    outPut = "在线：NETWORK_ALIVE_WAN\n";
                }
                if ((flags & NETWORK_ALIVE_AOL) == NETWORK_ALIVE_AOL)
                {
                    outPut = "在线：NETWORK_ALIVE_AOL\n";
                }
            }
            else
            {
                outPut = "不在线\n";
            }
            #endregion
        }

        /// <summary>
        /// 对网络状况不能及时反应
        /// </summary>        
        public static bool IsNetworkConnected2()
        {
            int flags;//上网方式 
            bool online = InternetGetConnectedState(out flags, 0);
            return online;

            #region Details
            int INTERNET_CONNECTION_MODEM = 1;
            int INTERNET_CONNECTION_LAN = 2;
            int INTERNET_CONNECTION_PROXY = 4;
            int INTERNET_CONNECTION_MODEM_BUSY = 8;
            string outPut = null;
            if (online)//在线   
            {
                if ((flags & INTERNET_CONNECTION_MODEM) == INTERNET_CONNECTION_MODEM)
                {
                    outPut = "在线：拨号上网\n";
                }
                if ((flags & INTERNET_CONNECTION_LAN) == INTERNET_CONNECTION_LAN)
                {
                    outPut = "在线：通过局域网\n";
                }
                if ((flags & INTERNET_CONNECTION_PROXY) == INTERNET_CONNECTION_PROXY)
                {
                    outPut = "在线：代理\n";
                }
                if ((flags & INTERNET_CONNECTION_MODEM_BUSY) == INTERNET_CONNECTION_MODEM_BUSY)
                {
                    outPut = "MODEM被其他非INTERNET连接占用\n";
                }
            }
            else
            {
                outPut = "不在线\n";
            }
            #endregion
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        [DllImport("sensapi.dll")]
        private extern static bool IsNetworkAlive(out int connectionDescription);
        #endregion
    }

    public struct CpuInfo
    {
        public CpuInfo(string name, uint speed, uint coreCount)
        {
            this.Name = name;
            this.ClockSpeed = speed;
            this.CoreCount = coreCount;
        }

        /// <summary>
        /// CPU名称。
        /// </summary>
        public string Name;
        /// <summary>
        /// 主频。
        /// </summary>
        public uint ClockSpeed;
        /// <summary>
        /// 核心数目
        /// </summary>
        public uint CoreCount;
    } 
}
