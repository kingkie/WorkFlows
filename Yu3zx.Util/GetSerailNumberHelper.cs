using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace Yu3zx.Util
{
    /// <summary>
    /// 获取硬盘卷标号；CPU序列号；
    /// </summary>
    public class GetSerailNumberHelper
    {
        /// <summary>
        /// 取得设备硬盘C盘的卷标号
        /// </summary>
        /// <returns></returns>
        public string GetDiskVolumeSerialNumber()
        {
            ManagementClass mc =
                 new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObject disk =
                 new ManagementObject("win32_logicaldisk.deviceid=\"c:\"");
            disk.Get();
            return disk.GetPropertyValue("VolumeSerialNumber").ToString();
        }

        /// <summary>
        /// 取得设备硬盘的卷标号,盘符需要加引号
        /// </summary>
        /// <param name="deviceid">例: "C:"</param>
        /// <returns></returns>
        public string GetDiskVolumeSerialNumber(string deviceid)
        {
            try
            {
                ManagementClass mc =
                     new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObject disk =
                     new ManagementObject("win32_logicaldisk.deviceid=" + deviceid);
                disk.Get();
                return disk.GetPropertyValue("VolumeSerialNumber").ToString();
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// 获取CPU序列号
        /// </summary>
        /// <returns></returns>
        public string GetCpuSerialNumber()
        {
            string strCpu = null;
            ManagementClass myCpu = new ManagementClass("win32_Processor");
            ManagementObjectCollection myCpuConnection = myCpu.GetInstances();
            foreach (ManagementObject myObject in myCpuConnection)
            {
                strCpu = myObject.Properties["Processorid"].Value.ToString();
                break;
            }
            return strCpu;
        }
    }
}
