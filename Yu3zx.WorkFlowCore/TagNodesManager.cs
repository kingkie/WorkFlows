using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore
{
    public class TagNodesManager
    {
        private static TagNodesManager instance = null;
        private static object Singleton_Lock = new object(); //锁同步
        public static TagNodesManager CreateInstance()
        {
            lock (Singleton_Lock)
            {
                if (instance == null)
                {
                    instance = Read();
                    if (instance == null)
                    {
                        instance = new TagNodesManager();
                    }
                }
            }
            return instance;
        }

        private static TagNodesManager Read()
        {
            if (!File.Exists("Config\\TagNodesManager.json"))
            {

            }
            FileStream file = new FileStream("Config\\TagNodesManager.json", FileMode.OpenOrCreate);
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
            TagNodesManager descJsonStu = JsonConvert.DeserializeObject<TagNodesManager>(readJson, jsonSerializerSettings);//反序列化

            return descJsonStu;
        }

        public bool Save()
        {
            try
            {
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                //jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;//
                jsonSerializerSettings.Formatting = Formatting.Indented;
                string jsonData = JsonConvert.SerializeObject(this, jsonSerializerSettings);
                FileStream nFile = new FileStream("Config\\TagNodesManager.json", FileMode.OpenOrCreate);
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

        public List<TagNode> TagNodes = new List<TagNode>();

        /// <summary>
        /// 查找结点
        /// </summary>
        /// <param name="nodeid"></param>
        /// <returns></returns>
        public TagNode FindTagNode(string nodeid)
        {
            TagNode rtnNode = TagNodes.Find(u => u.NodeId == nodeid);
            return rtnNode;
        }

        /// <summary>
        /// 查找结点,没有查到就创建个新的，并加到集合里
        /// </summary>
        /// <param name="nodeid"></param>
        /// <returns></returns>
        public TagNode FindTagNode(string nodeid,string nodename = "")
        {
            TagNode rtnNode = TagNodes.Find(u => u.NodeId == nodeid);
            if (rtnNode == null)
            {
                if(string.IsNullOrEmpty(nodename))
                {
                    nodename = nodeid;
                }
                rtnNode = new TagNode();
                rtnNode.NodeId = nodeid;
                rtnNode.NodeName = nodename;
                TagNodes.Add(rtnNode);
            }
            return rtnNode;
        }

        /// <summary>
        /// 获取结点值
        /// </summary>
        /// <param name="nodeid"></param>
        /// <returns></returns>
        public object GetNodeValue(string nodeid)
        {
            TagNode rtnNode = TagNodes.Find(u => u.NodeId == nodeid);
            if (rtnNode != null)
            {
                return rtnNode.NodeValue;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取结点值
        /// </summary>
        /// <param name="nodeid"></param>
        /// <returns></returns>
        public T GetNodeValue<T>(string nodeid)
        {
            TagNode rtnNode = TagNodes.Find(u => u.NodeId == nodeid);
            if (rtnNode != null && rtnNode.NodeValue != null)
            {
                return (T)Convert.ChangeType(rtnNode.NodeValue, typeof(T));
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// 清空所有TagNode值
        /// </summary>
        public void InitNode()
        {
            foreach (TagNode tn in TagNodes)
            {
                tn.NodeValue = null;
            }
        }
    }
}
