using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yu3zx.Util
{
    /// <summary>
    /// 对象属性集合，可以给对象增加临时属性
    /// 例：给wangwu这个人增加是否是关键人物属性；
    /// AttachedPropertyBuffer.SetPropertyValue(wangwu,"IsKeyPerson", true);
    /// </summary>
    public static class AttachedPropertyBuffer
    {
        private static readonly Dictionary<object, Dictionary<string, object>> BufferDictionary
            = new Dictionary<object, Dictionary<string, object>>();

        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            if (BufferDictionary.ContainsKey(obj))
            {
                var innerDic = BufferDictionary[obj];
                if (innerDic.ContainsKey(propertyName))
                {
                    innerDic[propertyName] = value;
                }
                else
                {
                    innerDic.Add(propertyName, value);
                }
            }
            else
            {
                var innerDic = new Dictionary<string, object> { { propertyName, value } };
                BufferDictionary.Add(obj, innerDic);
            }
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (BufferDictionary.ContainsKey(obj))
            {
                var innerDic = BufferDictionary[obj];
                if (innerDic.ContainsKey(propertyName))
                {
                    return innerDic[propertyName];
                }
            }
            return default(object);
        }
    }
}
