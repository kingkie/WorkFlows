using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Soap;


namespace Yu3zx.Util
{
    public static class SerializeHelper
    {
        #region BinaryFormatter
        #region SerializeObject
        public static byte[] SerializeObject(object obj) //obj 可以是数组
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();//此种情况下,mem_stream的缓冲区大小是可变的

            formatter.Serialize(memoryStream, obj);

            byte[] buff = memoryStream.ToArray();
            memoryStream.Close();

            return buff;
        }

        public static void SerializeObject(object obj, ref byte[] buff, int offset) //obj 可以是数组
        {
            byte[] rude_buff = SerializeHelper.SerializeObject(obj);
            for (int i = 0; i < rude_buff.Length; i++)
            {
                buff[offset + i] = rude_buff[i];
            }
        }
        #endregion

        #region DeserializeBytes
        public static object DeserializeBytes(byte[] buff, int index, int count)
        {
            IFormatter formatter = new BinaryFormatter();

            MemoryStream stream = new MemoryStream(buff, index, count);
            object obj = formatter.Deserialize(stream);
            stream.Close();

            return obj;
        }
        #endregion
        #endregion

        #region SoapFormatter
        #region SerializeObjectToString
        /// <summary>
        /// SerializeObjectToString 将对象序列化为SOAP XML 格式。
        /// 如果要将对象转化为简洁的xml格式，请使用ESBasic.Persistence.SimpleXmlConverter类。
        /// </summary>        
        public static string SerializeObjectToString(object obj)
        {
            IFormatter formatter = new SoapFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, obj);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string res = reader.ReadToEnd();
            stream.Close();

            return res;
        }
        #endregion

        #region DeserializeString
        public static object DeserializeString(string str)
        {
            byte[] buff = System.Text.Encoding.Default.GetBytes(str);
            IFormatter formatter = new SoapFormatter();
            MemoryStream stream = new MemoryStream(buff, 0, buff.Length);
            object obj = formatter.Deserialize(stream);
            stream.Close();

            return obj;
        }
        #endregion
        #endregion

        #region XmlSerializer
        #region XmlObject
        public static string XmlObject(object obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            MemoryStream stream = new MemoryStream();
            xmlSerializer.Serialize(stream, obj);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string res = reader.ReadToEnd();
            stream.Close();

            return res;
        }
        #endregion

        #region ObjectXml
        public static T ObjectXml<T>(string str)
        {
            return (T)SerializeHelper.ObjectXml(str, typeof(T));
        }

        public static object ObjectXml(string str, Type targetType)
        {
            byte[] buff = System.Text.Encoding.Default.GetBytes(str);
            XmlSerializer xmlSerializer = new XmlSerializer(targetType);
            MemoryStream stream = new MemoryStream(buff, 0, buff.Length);
            object obj = xmlSerializer.Deserialize(stream);
            stream.Close();

            return obj;
        }
        #endregion
        #endregion

        #region SaveToFile
        /// <summary>
        /// SaveToFile 将对象的二进制序列化后保存到文件。
        /// </summary>       
        public static void SaveToFile(object obj, string filePath)
        {
            FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);

            stream.Flush();
            stream.Close();
        }
        #endregion

        #region ReadFromFile
        /// <summary>
        /// ReadFromFile 从文件读取二进制反序列化为对象。
        /// </summary> 
        public static object ReadFromFile(string filePath)
        {
            byte[] buff = ReadFileBytes(filePath);
            return SerializeHelper.DeserializeBytes(buff, 0, buff.Length);
        }

        #region ReadFileReturnBytes
        /// <summary>
        /// ReadFileReturnBytes 从文件中读取二进制数据
        /// </summary>      
        public static byte[] ReadFileBytes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            BinaryReader br = new BinaryReader(fs);

            byte[] buff = br.ReadBytes((int)fs.Length);

            br.Close();
            fs.Close();

            return buff;
        }
        #endregion
        #endregion
    }	
}
