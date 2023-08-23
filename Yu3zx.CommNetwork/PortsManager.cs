using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SKII.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace Yu3zx.CommNetwork
{
    /// <summary>
    /// 通信接口管理单例
    /// </summary>
    public class PortsManager
    {
        #region  单例
        private static object syncObj = new object();
        private static PortsManager instance = null;
        public static PortsManager GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = Read();
                    if (instance == null)
                    {
                        instance = new PortsManager();
                    }
                }
            }
            return instance;
        }

        PortsManager()
        {
        }

        private static string _DefaultFilePathString = AppDomain.CurrentDomain.BaseDirectory;
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
                    //string filepathstring = Path.Combine(_DefaultFilePathString, "Config", "\\Devices.json");
                    string filepathstring = Path.Combine(_DefaultFilePathString, "Config") + "\\PortsManager.json";
                    //为了断电等原因导致xml文件保存出错，文件损坏，采用先写副本再替换的方式。
                    using (TextWriter textWriter = File.CreateText(filepathstring))
                    {
                        string jsonsavestr =  JSONUtil.SerializeJSON(this);
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

        private static PortsManager Read()
        {
            string filepathstring = Path.Combine(_DefaultFilePathString, "Config") + "\\PortsManager.json";
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

                    serializer.TypeNameHandling = TypeNameHandling.All;//
                    serializer.Formatting = Formatting.Indented;

                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    //serializer.TypeNameHandling = TypeNameHandling.Objects;//这一行就是设置Json.NET能够序列化接口或继承类的关键，将TypeNameHandling设置为All
                    //构建Json.net的读取流
                    JsonReader reader = new JsonTextReader(sr);
                    //对读取出的Json.net的reader流进行反序列化，并装载到模型中
                    PortsManager instancetmp = serializer.Deserialize<PortsManager>(reader);

                    return instancetmp;
                }
                catch
                {
                    return new PortsManager();
                }
            }
        }

        #endregion End

        public List<BaseBus> Ports = new List<BaseBus>();

    }
}
