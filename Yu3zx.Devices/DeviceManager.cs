using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SKII.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace Yu3zx.Devices
{
    /// <summary>
    /// 新版本设备管理模块，优化了复杂的内容
    /// </summary>
    public class DeviceManager
    {
        private static object syncObj = new object();
        private static DeviceManager instance = null;

        #region 单例
        public static DeviceManager GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = Read();

                    if (instance == null)
                    {
                        instance = new DeviceManager();
                    }
                }
            }
            return instance;
        }

        private static string defaultFilePathString = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            lock (syncObj)
            {
                try
                {
                    string filepathstring = Path.Combine(defaultFilePathString, "Config") + "\\DeviceManager.json";
                    //为了断电等原因导致xml文件保存出错，文件损坏，采用先写副本再替换的方式。
                    using (TextWriter textWriter = File.CreateText(filepathstring))
                    {
                        string jsonsavestr = JSONUtil.SerializeJSON(this);
                        textWriter.Write(jsonsavestr);
                        textWriter.Flush();
                    }
                    if (File.Exists(filepathstring))
                    {
                        FileInfo fi = new FileInfo(filepathstring);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        {
                            fi.Attributes = FileAttributes.Normal;
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        private static DeviceManager Read()
        {
            string filepathstring = Path.Combine(defaultFilePathString, "Config") + "\\DeviceManager.json";
            if (filepathstring == "")
                return null;
            if (!File.Exists(filepathstring))
            {
                using (TextWriter textWriter = File.CreateText(filepathstring))
                {
                    textWriter.Write("{}");
                    textWriter.Flush();
                }
            }
            using (StreamReader sr = new StreamReader(filepathstring))
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Add(new JavaScriptDateTimeConverter());
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    serializer.TypeNameHandling = TypeNameHandling.Objects;//这一行就是设置Json.NET能够序列化接口或继承类的关键，将TypeNameHandling设置为All
                    //构建Json.net的读取流
                    JsonReader reader = new JsonTextReader(sr);
                    //对读取出的Json.net的reader流进行反序列化，并装载到模型中
                    DeviceManager instancetmp = serializer.Deserialize<DeviceManager>(reader);

                    return instancetmp;
                }
                catch
                {
                    return new DeviceManager();
                }
            }
        }

        #endregion End

        public List<Device> DevicesList = new List<Device>();

        /// <summary>
        /// 通过设备ID查找设备
        /// </summary>
        /// <param name="devId"></param>
        /// <returns></returns>
        public Device GetDeviceById(string devId)
        {
            if (string.IsNullOrEmpty(devId))
            {
                return null;
            }
            return DevicesList.Find(x => x.DevId == devId);
        }

        /// <summary>
        /// 增加设备
        /// </summary>
        /// <param name="dev"></param>
        public void AddDevice(Device dev)
        {
            DevicesList.Add(dev);
        }

        /// <summary>
        /// 增加设备
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="itemindex"></param>
        public void AddDevice(Device dev, int itemindex)
        {
            if (itemindex > DevicesList.Count)
            {
                DevicesList.Add(dev);
            }
            else
            {
                DevicesList.Insert(itemindex, dev);
            }
        }

        /// <summary>
        /// 移除设备
        /// </summary>
        /// <param name="dev"></param>
        public void ReMoveDevice(Device dev)
        {
            DevicesList.Remove(dev);
        }
        /// <summary>
        /// 移除设备
        /// </summary>
        /// <param name="itemindex"></param>
        public void ReMoveDevice(int itemindex)
        {
            DevicesList.RemoveAt(itemindex);
        }
        public void ReMoveDevice(string devid)
        {
            Device devtmp = DevicesList.Find(x => x.DevId == devid);
            if (devtmp != null)
            {
                DevicesList.Remove(devtmp);
            }
        }

    }
}
