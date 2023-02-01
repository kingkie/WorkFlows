using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;

namespace Yu3zx.Util
{
    /// <summary>
    /// 对象深拷贝
    /// </summary>
    public class DeepCopy
    {
        public static T Copy<T>(T item)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, item);
            memoryStream.Seek(0L, SeekOrigin.Begin);
            T result = (T)((object)binaryFormatter.Deserialize(memoryStream));
            memoryStream.Close();
            return result;
        }

        public static Dictionary<string, string> GetEntityPropertyToDict<T>(T tEntity)
        {
            Dictionary<string, string> dictClass = new Dictionary<string, string>();
            Type ty = tEntity.GetType();//获取对象类型
            PropertyInfo[] infos = ty.GetProperties(BindingFlags.Public);
            foreach (PropertyInfo item in infos)
            {
                string pName = item.Name;//获取属性名称
                string pValue = item.GetValue(tEntity, null).ToString();
                dictClass.Add(pName, pValue);
            }
            return dictClass;
        }
    }
}
