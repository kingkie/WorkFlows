using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Yu3zx.Util
{
    public class RegistryHelper
    {
        #region 获取注册本键值
        /// <summary>
        /// 获取CurrentUser下的某键值
        /// </summary>
        /// <param name="keyPath">@"Software\Microsoft\Internet   Explorer\Main"</param>
        /// <param name="keyName">"Window   Title"</param>
        /// <returns></returns>
        public string GetCurrentUserKeyValue(string keyPath, string keyName)
        {
            try
            {
                RegistryKey Key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
                return Key.GetValue(keyName).ToString(); 
            }
            catch
            {
                return "";
            }

        }

        /// <summary>
        /// 获取LocalMachine下的某键值
        /// </summary>
        /// <param name="keyPath">@"Software\Microsoft\Internet   Explorer\Main"</param>
        /// <param name="keyName">"Window   Title"</param>
        /// <returns></returns>
        public string GetLocalMachineKeyValue(string keyPath, string keyName)
        {
            try
            {
                RegistryKey Key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyPath);
                return Key.GetValue(keyName).ToString();
            }
            catch
            {
                return "";
            }

        }

        /// <summary>
        /// 获取ClassesRoot下的某键值
        /// </summary>
        /// <param name="keyPath">@"Software\Microsoft\Internet   Explorer\Main"</param>
        /// <param name="keyName">"Window   Title"</param>
        /// <returns></returns>
        public string GetClassesRootKeyValue(string keyPath, string keyName)
        {
            try
            {
                RegistryKey Key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(keyPath);
                return Key.GetValue(keyName).ToString();
            }
            catch
            {
                return "";
            }

        }
        #endregion End

        #region 设置注册表键值

        /// <summary>
        /// 获取CurrentUser下的某键值
        /// </summary>
        /// <param name="keyPath">@"Software\Microsoft\Internet   Explorer\Main"</param>
        /// <param name="keyName">"Window   Title"</param>
        /// <param name="value">设置的值</param>
        /// <param name="vType">设置值的类型</param>
        /// <returns></returns>
        public bool SetCurrentUserKeyValue(string keyPath, string keyName,object value,RegistryValueKind vType)
        {
            try
            {
                RegistryKey Key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(keyPath);
                Key.SetValue(keyName, value, vType);
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// 获取LocalMachine下的某键值
        /// </summary>
        /// <param name="keyPath">@"Software\Microsoft\Internet   Explorer\Main"</param>
        /// <param name="keyName">"Window   Title"</param>
        /// <param name="value">设置的值</param>
        /// <param name="vType">设置值的类型</param>
        /// <returns></returns>
        public bool SetLocalMachineKeyValue(string keyPath, string keyName,object value,RegistryValueKind vType)
        {
            try
            {
                RegistryKey Key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyPath);
                Key.SetValue(keyName, value, vType);
                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// 获取ClassesRoot下的某键值
        /// </summary>
        /// <param name="keyPath">@"Software\Microsoft\Internet   Explorer\Main"</param>
        /// <param name="keyName">"Window   Title"</param>
        /// <param name="value">设置的值</param>
        /// <param name="vType">设置值的类型</param>
        /// <returns></returns>
        public bool SetClassesRootKeyValue(string keyPath, string keyName,object value, RegistryValueKind vType)
        {
            try
            {
                RegistryKey Key = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(keyPath);
                Key.SetValue(keyName, value, vType);
                return true;
            }
            catch
            {
                return false;
            }

        }

        #endregion End
    }
}
