using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore
{
    /// <summary>
    /// 流程管理
    /// </summary>
    public class WorkFlowManager
    {
        private static WorkFlowManager instance = null;
        private static object Singleton_Lock = new object(); //锁同步
        public static WorkFlowManager CreateInstance()
        {
            lock (Singleton_Lock)
            {
                if (instance == null)
                {
                    instance = Read();
                    if (instance == null)
                    {
                        instance = new WorkFlowManager();
                    }
                }
            }
            return instance;
        }

        private static WorkFlowManager Read()
        {
            if (!File.Exists("Config\\WorkFlowManager.json"))
            {

            }
            FileStream file = new FileStream("Config\\WorkFlowManager.json", FileMode.OpenOrCreate);
            StreamReader sr = new StreamReader(file);
            string readJson = sr.ReadToEnd();   //从文件读出来
            if (readJson == "")
            {
                readJson = "{}";
            }
            sr.Close();
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;//
            jsonSerializerSettings.Formatting = Formatting.Indented;
            WorkFlowManager descJsonStu = JsonConvert.DeserializeObject<WorkFlowManager>(readJson, jsonSerializerSettings);//反序列化

            return descJsonStu;
        }

        public bool Save()
        {
            try
            {
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;//
                jsonSerializerSettings.Formatting = Formatting.Indented;
                string jsonData = JsonConvert.SerializeObject(this, jsonSerializerSettings);
                FileStream nFile = new FileStream("Config\\WorkFlowManager.json", FileMode.OpenOrCreate);
                StreamWriter writer = new StreamWriter(nFile);
                writer.Write(jsonData);
                writer.Close();//写到文件
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<WorkFlow> Flows = new List<WorkFlow>();

        public WorkFlow FindWorkFlow(string _flowId)
        {
            WorkFlow rtnFlow = Flows.Find(u => u.WorkFlowId.ToLower() == _flowId.ToLower());
            return rtnFlow;
        }

        /// <summary>
        /// 拷贝流程到流程实例
        /// </summary>
        /// <param name="cntWorkFlow"></param>
        /// <returns></returns>
        public WorkFlow DeepCopyWorkFlow(WorkFlow cntWorkFlow)
        {
            BinaryFormatter bFormatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            bFormatter.Serialize(stream, cntWorkFlow);
            stream.Seek(0, SeekOrigin.Begin);
            return (WorkFlow)bFormatter.Deserialize(stream);
        }

        /// <summary>
        /// 拷贝流程到流程实例
        /// </summary>
        /// <param name="cntWorkFlow"></param>
        /// <returns></returns>
        public WorkFlow CopyWorkFlow(WorkFlow cntWorkFlow)
        {
            WorkFlow newWorkFlow = new WorkFlow();
            //对非属性公共项不能复制
            newWorkFlow = AutoMapper<WorkFlow, WorkFlow>.ConvertFrom(cntWorkFlow);
            newWorkFlow.WorkFlowId = cntWorkFlow.WorkFlowId;// WorkFlowUtil.BuildFlowId("NewInstance_");
            //newWorkFlow.WorkFlowName = cntWorkFlow.WorkFlowName + "";
            newWorkFlow.FlowType = cntWorkFlow.FlowType;
            newWorkFlow.FlowItems.Clear();
            newWorkFlow.FlowItems.AddRange(cntWorkFlow.FlowItems);
            return newWorkFlow;
        }
    }
}
