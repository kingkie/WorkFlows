using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Yu3zx.Util
{
    /// <summary>
    /// EnumHelper 的摘要说明。
    /// </summary>
    public static class EnumHelper
    {
        #region ConvertEnumToList
        /// <summary>
        /// ConvertEnumToFieldDescriptionList 将Enum的所有枚举值放到IList中，以绑定到如ComoboBox等控件
        /// </summary>		
        public static IList<string> ConvertEnumToFieldDescriptionList(Type enumType)
        {
            IList<string> resultList = new List<string>();

            FieldInfo[] fields = enumType.GetFields();
            foreach (FieldInfo fi in fields)
            {
                object[] attrs = fi.GetCustomAttributes(typeof(EnumDescription), false);
                if ((attrs != null) && (attrs.Length > 0))
                {
                    EnumDescription des = (EnumDescription)attrs[0];
                    resultList.Add(des.Description);
                }
                else
                {
                    if (fi.Name != "value__")
                    {
                        resultList.Add(fi.Name);
                    }
                }
            }

            return resultList;
        }
        #endregion

        #region ConvertEnumToFieldTextList
        /// <summary>
        /// ConvertEnumToFieldTextList 获取Enum的所有Field的文本表示。
        /// </summary>       
        public static IList<string> ConvertEnumToFieldTextList(Type enumType)
        {
            IList<string> resultList = new List<string>();

            FieldInfo[] fields = enumType.GetFields();
            foreach (FieldInfo fi in fields)
            {
                if (fi.Name != "value__")
                {
                    resultList.Add(fi.Name);
                }
            }

            return resultList;
        }
        #endregion

        #region ParseEnumValue
        /// <summary>
        /// ParseEnumValue 与ConvertEnumToList结合使用，将ComoboBox等控件中选中的string转换为枚举值
        /// </summary>       
        public static object ParseEnumValue(Type enumType, string filedValOrDesc)
        {
            if ((enumType == null) || (filedValOrDesc == null))
            {
                return null;
            }

            FieldInfo[] fields = enumType.GetFields();
            foreach (FieldInfo fi in fields)
            {
                object[] attrs = fi.GetCustomAttributes(typeof(EnumDescription), false);
                if ((attrs != null) && (attrs.Length > 0))
                {
                    EnumDescription des = (EnumDescription)attrs[0];
                    if (filedValOrDesc == des.Description)
                    {
                        return Enum.Parse(enumType, fi.Name);
                    }
                }
            }

            return Enum.Parse(enumType, filedValOrDesc);
        }
        #endregion
    }

    /// <summary>
    /// EnumDescription 用于描述枚举的特性。	
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class EnumDescription : Attribute
    {
        private static IDictionary<string, IList<EnumDescription>> EnumDescriptionCache = new Dictionary<string, IList<EnumDescription>>(); //EnumType.FullName - IList<EnumDescription>

        #region Ctor
        public EnumDescription(string _description)
            : this(_description, null)
        {
        }

        public EnumDescription(string _description, object _tag)
        {
            this.description = _description;
            this.tag = _tag;
        }
        #endregion

        #region property
        #region Description
        private string description = "";
        public string Description
        {
            get
            {
                return this.description;
            }
        }
        #endregion

        #region EnumValue
        private object enumValue = null;
        public object EnumValue
        {
            get
            {
                return this.enumValue;
            }
        }
        #endregion

        #region Tag
        private object tag = null;
        public object Tag
        {
            get
            {
                return this.tag;
            }
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            return this.description;
        }
        #endregion

        #endregion

        #region DoGetFieldTexts
        /// <summary>
        /// DoGetFieldTexts 得到枚举类型定义的所有枚举值的描述文本		
        /// </summary>	
        private static IList<EnumDescription> DoGetFieldTexts(Type enumType)
        {
            if (!EnumDescription.EnumDescriptionCache.ContainsKey(enumType.FullName))
            {
                FieldInfo[] fields = enumType.GetFields();
                IList<EnumDescription> list = new List<EnumDescription>();
                foreach (FieldInfo fi in fields)
                {
                    object[] eds = fi.GetCustomAttributes(typeof(EnumDescription), false);
                    if (eds.Length == 1)
                    {
                        EnumDescription enumDescription = (EnumDescription)eds[0];
                        enumDescription.enumValue = fi.GetValue(null);
                        list.Add(enumDescription);
                    }
                }

                EnumDescription.EnumDescriptionCache.Add(enumType.FullName, list);
            }

            return EnumDescription.EnumDescriptionCache[enumType.FullName];
        }
        #endregion

        #region GetEnumDescriptionText
        /// <summary>
        /// GetEnumDescriptionText 获取枚举类型的描述文本。
        /// </summary>	   
        public static string GetEnumDescriptionText(Type enumType)
        {
            EnumDescription[] enumDescriptionAry = (EnumDescription[])enumType.GetCustomAttributes(typeof(EnumDescription), false);
            if (enumDescriptionAry.Length < 1)
            {
                return string.Empty;
            }

            return enumDescriptionAry[0].Description;
        }
        #endregion

        #region GetEnumTag
        /// <summary>
        /// GetEnumTag 获取枚举类型携带的Tag。
        /// </summary>
        public static object GetEnumTag(Type enumType)
        {
            EnumDescription[] eds = (EnumDescription[])enumType.GetCustomAttributes(typeof(EnumDescription), false);
            if (eds.Length != 1) return string.Empty;
            return eds[0].Tag;
        }
        #endregion

        #region GetFieldText
        /// <summary>
        /// GetFieldDescriptionText 获得指定枚举值的描述文本。
        /// </summary>		
        public static string GetFieldText(object enumValue)
        {
            IList<EnumDescription> list = EnumDescription.DoGetFieldTexts(enumValue.GetType());
            if (list == null)
            {
                return null;
            }

            return CollectionConverter.ConvertFirstSpecification<EnumDescription, string>(list, delegate(EnumDescription ed) { return ed.Description; }, delegate(EnumDescription ed) { return ed.enumValue.ToString() == enumValue.ToString(); });
        }
        #endregion

        #region GetFieldTag
        /// <summary>
        /// GetFieldTag 获得指定枚举值的Tag。
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static object GetFieldTag(object enumValue)
        {
            IList<EnumDescription> list = EnumDescription.DoGetFieldTexts(enumValue.GetType());
            if (list == null)
            {
                return null;
            }

            return CollectionConverter.ConvertFirstSpecification<EnumDescription, object>(list, delegate(EnumDescription ed) { return ed.Tag; }, delegate(EnumDescription ed) { return ed.enumValue.ToString() == enumValue.ToString(); });
        }
        #endregion

        #region GetEnumValueByTag
        /// <summary>
        /// GetEnumValueByTag 根据描述Tag获取对应的枚举值
        /// </summary>     
        public static object GetEnumValueByTag(Type enumType, object tag)
        {
            IList<EnumDescription> list = EnumDescription.DoGetFieldTexts(enumType);
            if (list == null)
            {
                return null;
            }

            return CollectionConverter.ConvertFirstSpecification<EnumDescription, object>(list, delegate(EnumDescription des) { return des.enumValue; }, delegate(EnumDescription des) { return des.tag.ToString() == tag.ToString(); });
        }
        #endregion

    }

    /// <summary>
    /// CollectionConverter 用于转换集合内的元素或集合类型。
    /// </summary>
    public static class CollectionConverter
    {
        #region ConvertAll
        /// <summary>
        /// ConvertAll 将source中的每个元素转换为TResult类型
        /// </summary>       
        public static List<TResult> ConvertAll<TObject, TResult>(IEnumerable<TObject> source, Func<TObject, TResult> converter)
        {
            return CollectionConverter.ConvertSpecification<TObject, TResult>(source, converter, null);
        }
        #endregion

        #region ConvertSpecification
        /// <summary>
        /// ConvertSpecification 将source中的符合predicate条件元素转换为TResult类型
        /// </summary>       
        public static List<TResult> ConvertSpecification<TObject, TResult>(IEnumerable<TObject> source, Func<TObject, TResult> converter, Predicate<TObject> predicate)
        {
            List<TResult> list = new List<TResult>();
            CollectionHelper.ActionOnSpecification<TObject>(source, delegate(TObject ele) { list.Add(converter(ele)); }, predicate);
            return list;
        }
        #endregion

        #region ConvertFirstSpecification
        /// <summary>
        /// ConvertSpecification 将source中的符合predicate条件的第一个元素转换为TResult类型
        /// </summary>       
        public static TResult ConvertFirstSpecification<TObject, TResult>(IEnumerable<TObject> source, Func<TObject, TResult> converter, Predicate<TObject> predicate)
        {
            TObject target = CollectionHelper.FindFirstSpecification<TObject>(source, predicate);

            if (target == null)
            {
                return default(TResult);
            }

            return converter(target);
        }
        #endregion

        #region CopyAllToList
        public static List<TObject> CopyAllToList<TObject>(IEnumerable<TObject> source)
        {
            List<TObject> copy = new List<TObject>();
            CollectionHelper.ActionOnEach<TObject>(source, delegate(TObject t) { copy.Add(t); });
            return copy;
        }
        #endregion

        #region CopySpecificationToList
        public static List<TObject> CopySpecificationToList<TObject>(IEnumerable<TObject> source, Predicate<TObject> predicate)
        {
            List<TObject> copy = new List<TObject>();
            CollectionHelper.ActionOnSpecification<TObject>(source, delegate(TObject t) { copy.Add(t); }, predicate);
            return copy;
        }
        #endregion

        #region ConvertListUpper
        /// <summary>
        /// ConvertListUpper 将子类对象集合转换为基类对象集合
        /// </summary>        
        public static List<TBase> ConvertListUpper<TBase, T>(IList<T> list) where T : TBase
        {
            List<TBase> baseList = new List<TBase>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                baseList.Add(list[i]);
            }

            return baseList;
        }
        #endregion

        #region ConvertListDown
        /// <summary>
        /// ConvertListDown 将基类对象集合强制转换为子类对象集合
        /// </summary>        
        public static List<T> ConvertListDown<TBase, T>(IList<TBase> baseList) where T : TBase
        {
            List<T> list = new List<T>(baseList.Count);
            for (int i = 0; i < baseList.Count; i++)
            {
                list.Add((T)baseList[i]);
            }

            return list;
        }
        #endregion

        #region ConvertArrayToList
        /// <summary>
        /// ConverArrayToList 将数组转换为List
        /// </summary>      
        public static List<TElement> ConvertArrayToList<TElement>(TElement[] ary)
        {
            if (ary == null)
            {
                return null;
            }

            return CollectionHelper.Find<TElement>(ary, null);
        }
        #endregion

        #region ConvertListToArray
        /// <summary>
        /// ConverListToArray 将List转换为数组
        /// </summary>      
        public static TElement[] ConvertListToArray<TElement>(IList<TElement> list)
        {
            if (list == null)
            {
                return null;
            }

            TElement[] ary = new TElement[list.Count];
            for (int i = 0; i < ary.Length; i++)
            {
                ary[i] = list[i];
            }

            return ary;
        }
        #endregion

    }

    public static class CollectionHelper
    {
        #region Find
        /// <summary>
        /// Find 从集合中选取符合条件的元素
        /// </summary>       
        public static List<TObject> Find<TObject>(IEnumerable<TObject> source, Predicate<TObject> predicate)
        {
            List<TObject> list = new List<TObject>();
            CollectionHelper.ActionOnSpecification(source, delegate(TObject ele) { list.Add(ele); }, predicate);
            return list;
        }
        #endregion

        #region FindFirstSpecification
        /// <summary>
        /// FindFirstSpecification 返回符合条件的第一个元素
        /// </summary>      
        public static TObject FindFirstSpecification<TObject>(IEnumerable<TObject> source, Predicate<TObject> predicate)
        {
            foreach (TObject element in source)
            {
                if (predicate(element))
                {
                    return element;
                }
            }

            return default(TObject);
        }
        #endregion

        #region ContainsSpecification
        /// <summary>
        /// ContainsSpecification 集合中是否包含满足predicate条件的元素。
        /// </summary>       
        public static bool ContainsSpecification<TObject>(IEnumerable<TObject> source, Predicate<TObject> predicate, out TObject specification)
        {
            specification = default(TObject);
            foreach (TObject element in source)
            {
                if (predicate(element))
                {
                    specification = element;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ContainsSpecification 集合中是否包含满足predicate条件的元素。
        /// </summary>       
        public static bool ContainsSpecification<TObject>(IEnumerable<TObject> source, Predicate<TObject> predicate)
        {
            TObject specification;
            return CollectionHelper.ContainsSpecification<TObject>(source, predicate, out specification);
        }
        #endregion

        #region ActionOnSpecification
        /// <summary>
        /// ActionOnSpecification 对集合中满足predicate条件的元素执行action。如果没有条件，predicate传入null。
        /// </summary>       
        public static void ActionOnSpecification<TObject>(IEnumerable<TObject> collection, Action<TObject> action, Predicate<TObject> predicate)
        {
            if (collection == null)
            {
                return;
            }

            if (predicate == null)
            {
                foreach (TObject obj in collection)
                {
                    action(obj);
                }

                return;
            }

            foreach (TObject obj in collection)
            {
                if (predicate(obj))
                {
                    action(obj);
                }
            }
        }
        #endregion

        #region ActionOnEach
        /// <summary>
        /// ActionOnEach  对集合中的每个元素执行action。
        /// </summary>        
        public static void ActionOnEach<TObject>(IEnumerable<TObject> collection, Action<TObject> action)
        {
            CollectionHelper.ActionOnSpecification<TObject>(collection, action, null);
        }
        #endregion

        #region GetPart
        public static T[] GetPart<T>(T[] ary, int startIndex, int count)
        {
            return CollectionHelper.GetPart<T>(ary, startIndex, count, false);
        }

        public static T[] GetPart<T>(T[] ary, int startIndex, int count, bool reverse)
        {
            if (startIndex >= ary.Length)
            {
                return null;
            }

            if (ary.Length < startIndex + count)
            {
                count = ary.Length - startIndex;
            }

            T[] result = new T[count];

            if (!reverse)
            {
                for (int i = 0; i < count; i++)
                {
                    result[i] = ary[startIndex + i];
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    result[i] = ary[ary.Length - startIndex - 1 - i];
                }
            }

            return result;
        }
        #endregion

        #region BinarySearch
        /// <summary>
        /// BinarySearch 从已排序的列表中，采用二分查找找到目标在列表中的位置。
        /// 如果刚好有个元素与目标相等，则返回true，且minIndex会被赋予该元素的位置；否则，返回false，且minIndex会被赋予比目标小且最接近目标的元素的位置
        /// </summary>       
        public static bool BinarySearch<T>(IList<T> sortedList, T target, out int minIndex) where T : IComparable
        {
            if (target.CompareTo(sortedList[0]) == 0)
            {
                minIndex = 0;
                return true;
            }

            if (target.CompareTo(sortedList[0]) < 0)
            {
                minIndex = -1;
                return false;
            }

            if (target.CompareTo(sortedList[sortedList.Count - 1]) == 0)
            {
                minIndex = sortedList.Count - 1;
                return true;
            }

            if (target.CompareTo(sortedList[sortedList.Count - 1]) > 0)
            {
                minIndex = sortedList.Count - 1;
                return false;
            }

            int targetPosIndex = -1;
            int left = 0;
            int right = sortedList.Count - 1;

            while (right - left > 1)
            {
                int middle = (left + right) / 2;

                if (target.CompareTo(sortedList[middle]) == 0)
                {
                    minIndex = middle;
                    return true;
                }

                if (target.CompareTo(sortedList[middle]) < 0)
                {
                    right = middle;
                }
                else
                {
                    left = middle;
                }
            }

            minIndex = left;
            return false;
        }
        #endregion

        #region GetIntersection 、GetUnion
        /// <summary>
        /// GetIntersection 高效地求两个List元素的交集。
        /// </summary>        
        public static List<T> GetIntersection<T>(List<T> list1, List<T> list2) where T : IComparable
        {
            List<T> largList = list1.Count > list2.Count ? list1 : list2;
            List<T> smallList = largList == list1 ? list2 : list1;

            largList.Sort();

            int minIndex = 0;

            List<T> result = new List<T>();
            foreach (T tmp in smallList)
            {
                if (CollectionHelper.BinarySearch<T>(largList, tmp, out minIndex))
                {
                    result.Add(tmp);
                }
            }

            return result;
        }

        /// <summary>
        /// GetUnion 高效地求两个List元素的并集。
        /// </summary> 
        public static List<T> GetUnion<T>(List<T> list1, List<T> list2)
        {
            SortedDictionary<T, int> result = new SortedDictionary<T, int>();
            foreach (T tmp in list1)
            {
                if (!result.ContainsKey(tmp))
                {
                    result.Add(tmp, 0);
                }
            }

            foreach (T tmp in list2)
            {
                if (!result.ContainsKey(tmp))
                {
                    result.Add(tmp, 0);
                }
            }

            return (List<T>)CollectionConverter.CopyAllToList<T>(result.Keys);
        }
        #endregion
    }
}
