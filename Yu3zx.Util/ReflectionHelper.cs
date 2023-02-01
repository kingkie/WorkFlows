using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Yu3zx.Util
{
    /// <summary>
    /// ReflectionHelper 的摘要说明。
    /// </summary>
    public static class ReflectionHelper
    {
        #region GetType
        /// <summary>
        /// GetType  通过完全限定的类型名来加载对应的类型。typeAndAssName如"ESBasic.Filters.SourceFilter,ESBasic"。
        /// 如果为系统简单类型，则可以不带程序集名称。
        /// </summary>       
        public static Type GetType(string typeAndAssName)
        {
            string[] names = typeAndAssName.Split(',');
            if (names.Length < 2)
            {
                return Type.GetType(typeAndAssName);
            }

            return ReflectionHelper.GetType(names[0].Trim(), names[1].Trim());
        }

        /// <summary>
        /// GetType 加载assemblyName程序集中的名为typeFullName的类型。assemblyName不用带扩展名，如果目标类型在当前程序集中，assemblyName传入null	
        /// </summary>		
        public static Type GetType(string typeFullName, string assemblyName)
        {
            if (assemblyName == null)
            {
                return Type.GetType(typeFullName);
            }

            //搜索当前域中已加载的程序集
            Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ass in asses)
            {
                string[] names = ass.FullName.Split(',');
                if (names[0].Trim() == assemblyName.Trim())
                {
                    return ass.GetType(typeFullName);
                }
            }

            //加载目标程序集
            Assembly tarAssem = Assembly.Load(assemblyName);
            if (tarAssem != null)
            {
                return tarAssem.GetType(typeFullName);
            }

            return null;
        }
        #endregion

        #region GetTypeFullName
        public static string GetTypeFullName(Type t)
        {
            return t.FullName + "," + t.Assembly.FullName.Split(',')[0];
        }
        #endregion

        #region LoadDerivedInstance
        /// <summary>
        /// LoadDerivedInstance 将程序集中所有继承自TBase的类型实例化
        /// </summary>
        /// <typeparam name="TBase">基础类型（或接口类型）</typeparam>
        /// <param name="asm">目标程序集</param>
        /// <returns>TBase实例列表</returns>
        public static IList<TBase> LoadDerivedInstance<TBase>(Assembly asm)
        {
            IList<TBase> list = new List<TBase>();

            Type supType = typeof(TBase);
            foreach (Type t in asm.GetTypes())
            {
                if (supType.IsAssignableFrom(t) && (!t.IsAbstract) && (!t.IsInterface))
                {
                    TBase instance = (TBase)Activator.CreateInstance(t);
                    list.Add(instance);
                }
            }

            return list;
        }
        #endregion

        #region LoadDerivedType
        /// <summary>
        /// LoadDerivedType 加载directorySearched目录下所有程序集中的所有派生自baseType的类型
        /// </summary>
        /// <typeparam name="baseType">基类（或接口）类型</typeparam>
        /// <param name="directorySearched">搜索的目录</param>
        /// <param name="searchChildFolder">是否搜索子目录中的程序集</param>
        /// <param name="config">高级配置，可以传入null采用默认配置</param>        
        /// <returns>所有从BaseType派生的类型列表</returns>
        public static IList<Type> LoadDerivedType(Type baseType, string directorySearched, bool searchChildFolder, TypeLoadConfig config)
        {
            if (config == null)
            {
                config = new TypeLoadConfig();
            }

            IList<Type> derivedTypeList = new List<Type>();
            if (searchChildFolder)
            {
                ReflectionHelper.LoadDerivedTypeInAllFolder(baseType, derivedTypeList, directorySearched, config);
            }
            else
            {
                ReflectionHelper.LoadDerivedTypeInOneFolder(baseType, derivedTypeList, directorySearched, config);
            }

            return derivedTypeList;
        }

        #region TypeLoadConfig
        public class TypeLoadConfig
        {
            #region Ctor
            public TypeLoadConfig() { }
            public TypeLoadConfig(bool copyToMem, bool loadAbstract, string postfix)
            {
                this.copyToMemory = copyToMem;
                this.loadAbstractType = loadAbstract;
                this.targetFilePostfix = postfix;
            }
            #endregion

            #region CopyToMemory
            private bool copyToMemory = false;
            /// <summary>
            /// CopyToMem 是否将程序集拷贝到内存后加载
            /// </summary>
            public bool CopyToMemory
            {
                get { return copyToMemory; }
                set { copyToMemory = value; }
            }
            #endregion

            #region LoadAbstractType
            private bool loadAbstractType = false;
            /// <summary>
            /// LoadAbstractType 是否加载抽象类型
            /// </summary>
            public bool LoadAbstractType
            {
                get { return loadAbstractType; }
                set { loadAbstractType = value; }
            }
            #endregion

            #region TargetFilePostfix
            private string targetFilePostfix = ".dll";
            /// <summary>
            /// TargetFilePostfix 搜索的目标程序集的后缀名
            /// </summary>
            public string TargetFilePostfix
            {
                get { return targetFilePostfix; }
                set { targetFilePostfix = value; }
            }
            #endregion
        }
        #endregion

        #region LoadDerivedTypeInAllFolder
        private static void LoadDerivedTypeInAllFolder(Type baseType, IList<Type> derivedTypeList, string folderPath, TypeLoadConfig config)
        {
            ReflectionHelper.LoadDerivedTypeInOneFolder(baseType, derivedTypeList, folderPath, config);
            string[] folders = Directory.GetDirectories(folderPath);
            if (folders != null)
            {
                foreach (string nextFolder in folders)
                {
                    ReflectionHelper.LoadDerivedTypeInAllFolder(baseType, derivedTypeList, nextFolder, config);
                }
            }
        }
        #endregion

        #region LoadDerivedTypeInOneFolder
        private static void LoadDerivedTypeInOneFolder(Type baseType, IList<Type> derivedTypeList, string folderPath, TypeLoadConfig config)
        {
            string[] files = Directory.GetFiles(folderPath);
            foreach (string file in files)
            {
                if (config.TargetFilePostfix != null)
                {
                    if (!file.EndsWith(config.TargetFilePostfix))
                    {
                        continue;
                    }
                }

                Assembly asm = null;

                #region Asm
                try
                {
                    if (config.CopyToMemory)
                    {
                        byte[] addinStream = ReadFileReturnBytes(file);
                        asm = Assembly.Load(addinStream);
                    }
                    else
                    {
                        asm = Assembly.LoadFrom(file);
                    }
                }
                catch (Exception ee)
                {
                    ee = ee;
                }

                if (asm == null)
                {
                    continue;
                }
                #endregion

                Type[] types = asm.GetTypes();

                foreach (Type t in types)
                {
                    if (t.IsSubclassOf(baseType) || baseType.IsAssignableFrom(t))
                    {
                        bool canLoad = config.LoadAbstractType ? true : (!t.IsAbstract);
                        if (canLoad)
                        {
                            derivedTypeList.Add(t);
                        }
                    }
                }
            }

        }

        #region ReadFileReturnBytes
        /// <summary>
        /// ReadFileReturnBytes 从文件中读取二进制数据
        /// </summary>      
        public static byte[] ReadFileReturnBytes(string filePath)
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
        #endregion

        #region SetProperty
        /// <summary>
        /// SetProperty 如果list中的object具有指定的propertyName属性，则设置该属性的值为proValue
        /// </summary>		
        public static void SetProperty(IList<object> objs, string propertyName, object proValue)
        {
            object[] args = { proValue };
            foreach (object target in objs)
            {
                ReflectionHelper.SetProperty(target, propertyName, proValue);
            }
        }

        public static void SetProperty(object obj, string propertyName, object proValue)
        {
            ReflectionHelper.SetProperty(obj, propertyName, proValue, true);
        }

        /// <summary>
        /// SetProperty 如果object具有指定的propertyName属性，则设置该属性的值为proValue
        /// </summary>		
        public static void SetProperty(object obj, string propertyName, object proValue, bool ignoreError)
        {
            Type t = obj.GetType();
            PropertyInfo pro = t.GetProperty(propertyName);
            if ((pro == null) || (!pro.CanWrite))
            {
                if (!ignoreError)
                {
                    string msg = string.Format("The setter of property named '{0}' not found in '{1}'.", propertyName, t);
                    throw new Exception(msg);
                }

                return;
            }

            #region 尝试转换类型
            try
            {
                proValue = ChangeType(pro.PropertyType, proValue);
            }
            catch { }
            #endregion

            object[] args = { proValue };
            t.InvokeMember(propertyName, BindingFlags.Public | BindingFlags.IgnoreCase |
                        BindingFlags.Instance | BindingFlags.SetProperty, null, obj, args);
        }

        #region ChangeType
        /// <summary>
        /// ChangeType 对System.Convert.ChangeType进行了增强，支持(0,1)到bool的转换，字符串->枚举、int->枚举、字符串->Type
        /// </summary>       
        public static object ChangeType(Type targetType, object val)
        {
            #region null
            if (val == null)
            {
                return null;
            }
            #endregion

            if (targetType.IsAssignableFrom(val.GetType()))
            {
                return val;
            }

            #region Same Type
            if (targetType == val.GetType())
            {
                return val;
            }
            #endregion

            #region bool 1,0
            if (targetType == typeof(bool))
            {
                if (val.ToString() == "0")
                {
                    return false;
                }

                if (val.ToString() == "1")
                {
                    return true;
                }
            }
            #endregion

            #region Enum
            if (targetType.IsEnum)
            {
                int intVal = 0;
                bool suc = int.TryParse(val.ToString(), out intVal);
                if (!suc)
                {
                    return Enum.Parse(targetType, val.ToString());
                }
                else
                {
                    return val;
                }
            }
            #endregion

            #region Type
            if (targetType == typeof(Type))
            {
                return ReflectionHelper.GetType(val.ToString());
            }
            #endregion

            if (targetType == typeof(IComparable))
            {
                return val;
            }

            //将double赋值给数值型的DataRow的字段是可以的，但是通过反射赋值给object的非double的其它数值类型的属性，却不行        
            return System.Convert.ChangeType(val, targetType);

        }
        #endregion
        #endregion

        #region GetProperty
        /// <summary>
        /// GetProperty 根据指定的属性名获取目标对象该属性的值
        /// </summary>
        public static object GetProperty(object obj, string propertyName)
        {
            Type t = obj.GetType();

            return t.InvokeMember(propertyName, BindingFlags.Default | BindingFlags.GetProperty, null, obj, null);
        }
        #endregion

        #region GetFieldValue
        /// <summary>
        /// GetFieldValue 取得目标对象的指定field的值，field可以是private
        /// </summary>      
        public static object GetFieldValue(object obj, string fieldName)
        {
            Type t = obj.GetType();
            FieldInfo field = t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            if (field == null)
            {
                string msg = string.Format("The field named '{0}' not found in '{1}'.", fieldName, t);
                throw new Exception(msg);
            }

            return field.GetValue(obj);
        }
        #endregion

        #region SetFieldValue
        /// <summary>
        /// SetFieldValue 设置目标对象的指定field的值，field可以是private
        /// </summary>      
        public static void SetFieldValue(object obj, string fieldName, object val)
        {
            Type t = obj.GetType();
            FieldInfo field = t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.Instance);
            if (field == null)
            {
                string msg = string.Format("The field named '{0}' not found in '{1}'.", fieldName, t);
                throw new Exception(msg);
            }

            field.SetValue(obj, val);
        }
        #endregion

        #region CopyProperty
        /// <summary>
        /// CopyProperty 将source中的属性的值赋给target上同名的属性
        /// 使用CopyProperty可以方便的实现拷贝构造函数
        /// </summary>        
        public static void CopyProperty(object source, object target)
        {
            ReflectionHelper.CopyProperty(source, target, null);
        }

        /// <summary>
        /// CopyProperty 将source中的属性的值赋给target上想匹配的属性，匹配关系通过propertyMapItemList确定
        /// </summary>        
        public static void CopyProperty(object source, object target, IList<MapItem> propertyMapItemList)
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();
            PropertyInfo[] sourcePros = sourceType.GetProperties();

            if (propertyMapItemList != null)
            {
                foreach (MapItem item in propertyMapItemList)
                {
                    object val = ReflectionHelper.GetProperty(source, item.Source);
                    ReflectionHelper.SetProperty(target, item.Target, val);
                }
            }
            else
            {
                foreach (PropertyInfo sourceProperty in sourcePros)
                {
                    if (sourceProperty.CanRead)
                    {
                        object val = ReflectionHelper.GetProperty(source, sourceProperty.Name);
                        ReflectionHelper.SetProperty(target, sourceProperty.Name, val);
                    }
                }
            }
        }
        #endregion

        #region GetAllMethods、SearchMethod
        /// <summary>
        /// GetAllMethods 获取接口的所有方法信息，包括继承的
        /// </summary>       
        public static IList<MethodInfo> GetAllMethods(params Type[] interfaceTypes)
        {
            foreach (Type interfaceType in interfaceTypes)
            {
                if (!interfaceType.IsInterface)
                {
                    throw new Exception("Target Type must be interface!");
                }
            }

            IList<MethodInfo> list = new List<MethodInfo>();
            foreach (Type interfaceType in interfaceTypes)
            {
                ReflectionHelper.DistillMethods(interfaceType, ref list);
            }

            return list;
        }

        private static void DistillMethods(Type interfaceType, ref IList<MethodInfo> methodList)
        {
            foreach (MethodInfo meth in interfaceType.GetMethods())
            {
                bool isExist = false;
                foreach (MethodInfo temp in methodList)
                {
                    if ((temp.Name == meth.Name) && (temp.ReturnType == meth.ReturnType))
                    {
                        ParameterInfo[] para1 = temp.GetParameters();
                        ParameterInfo[] para2 = meth.GetParameters();
                        if (para1.Length == para2.Length)
                        {
                            bool same = true;
                            for (int i = 0; i < para1.Length; i++)
                            {
                                if (para1[i].ParameterType != para2[i].ParameterType)
                                {
                                    same = false;
                                }
                            }

                            if (same)
                            {
                                isExist = true;
                                break;
                            }
                        }
                    }
                }

                if (!isExist)
                {
                    methodList.Add(meth);
                }
            }

            foreach (Type superInterfaceType in interfaceType.GetInterfaces())
            {
                ReflectionHelper.DistillMethods(superInterfaceType, ref methodList);
            }
        }



        /// <summary>
        /// SearchGenericMethodInType 搜索指定类型定义的泛型方法，不包括继承的。
        /// </summary>       
        public static MethodInfo SearchGenericMethodInType(Type originType, string methodName, Type[] argTypes)
        {
            foreach (MethodInfo method in originType.GetMethods())
            {
                if (method.ContainsGenericParameters && method.Name == methodName)
                {
                    bool succeed = true;
                    ParameterInfo[] paras = method.GetParameters();
                    if (paras.Length == argTypes.Length)
                    {
                        for (int i = 0; i < paras.Length; i++)
                        {
                            if (!paras[i].ParameterType.IsGenericParameter) //跳过泛型参数
                            {
                                if (paras[i].ParameterType.IsGenericType) //如果参数本身就是泛型类型，如IList<T>
                                {
                                    if (paras[i].ParameterType.GetGenericTypeDefinition() != argTypes[i].GetGenericTypeDefinition())
                                    {
                                        succeed = false;
                                        break;
                                    }
                                }
                                else //普通类型的参数
                                {
                                    if (paras[i].ParameterType != argTypes[i])
                                    {
                                        succeed = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (succeed)
                        {
                            return method;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// SearchMethod 包括被继承的所有方法，也包括泛型方法。
        /// </summary>       
        public static MethodInfo SearchMethod(Type originType, string methodName, Type[] argTypes)
        {
            MethodInfo meth = originType.GetMethod(methodName, argTypes);
            if (meth != null)
            {
                return meth;
            }

            meth = ReflectionHelper.SearchGenericMethodInType(originType, methodName, argTypes);
            if (meth != null)
            {
                return meth;
            }

            //搜索基类 
            Type baseType = originType.BaseType;
            if (baseType != null)
            {
                while (baseType != typeof(object))
                {
                    MethodInfo target = baseType.GetMethod(methodName, argTypes);
                    if (target != null)
                    {
                        return target;
                    }

                    target = ReflectionHelper.SearchGenericMethodInType(baseType, methodName, argTypes);
                    if (target != null)
                    {
                        return target;
                    }

                    baseType = baseType.BaseType;
                }
            }

            //搜索基接口
            if (originType.GetInterfaces() != null)
            {
                IList<MethodInfo> list = ReflectionHelper.GetAllMethods(originType.GetInterfaces());
                foreach (MethodInfo theMethod in list)
                {
                    if (theMethod.Name != methodName)
                    {
                        continue;
                    }
                    ParameterInfo[] args = theMethod.GetParameters();
                    if (args.Length != argTypes.Length)
                    {
                        continue;
                    }

                    bool correctArgType = true;
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i].ParameterType != argTypes[i])
                        {
                            correctArgType = false;
                            break;
                        }
                    }

                    if (correctArgType)
                    {
                        return theMethod;
                    }
                }
            }

            return null;
        }

        #endregion

        #region GetFullMethodName
        public static string GetMethodFullName(MethodInfo method)
        {
            return string.Format("{0}.{1}()", method.DeclaringType, method.Name);
        }
        #endregion

        public static T DeepCopy<T>(T obj)
        {
            //如果是字符串或值类型则直接返回
            if (obj == null || obj is string || obj.GetType().IsValueType) return obj;

            object retval = Activator.CreateInstance(obj.GetType());
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (FieldInfo field in fields)
            {
                try { field.SetValue(retval, DeepCopy(field.GetValue(obj))); }
                catch { }
            }
            return (T)retval;
        }
    }

    /// <summary>
    /// MapItem 映射项。
    /// </summary>
    public class MapItem
    {
        #region Ctor
        public MapItem()
        {
        }

        public MapItem(string theSource, string theTarget)
        {
            this.source = theSource;
            this.target = theTarget;
        }
        #endregion

        #region Source
        private string source;
        public string Source
        {
            get { return source; }
            set { source = value; }
        }
        #endregion

        #region Target
        private string target;
        public string Target
        {
            get { return target; }
            set { target = value; }
        }
        #endregion
    }
}
