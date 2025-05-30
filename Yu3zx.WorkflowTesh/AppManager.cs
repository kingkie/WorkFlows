using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Yu3zx.WorkFlowCore;

namespace Yu3zx.WorkflowTesh
{
    public class AppManager
    {
        #region 单例定义
        private static AppManager instance = null;
        private static object singleLock = new object(); //锁同步

        /// <summary>
        /// 创建单例
        /// </summary>
        /// <returns>返回单例对象</returns>
        public static AppManager CreateInstance()
        {
            lock (singleLock)
            {
                if (instance == null)
                {
                    instance = new AppManager();
                }
            }
            return instance;
        }

        private Dictionary<string, string> dictButtons = new Dictionary<string, string>();

        public static event Action<bool> BusyStatusChanged;

        #endregion End

        public static Frame frame;

        public void Init()
        {
            WorkFlowEngine.CreateInstance().OnFlowItemWork += OnFlowWork; //注册事件

            foreach (WorkFlow wf in WorkFlowManager.CreateInstance().Flows)
            {
                dictButtons.Add(wf.WorkFlowId.ToLower(), wf.WorkFlowName);
            }
        }

        /// <summary>
        /// 页面导航控制
        /// </summary>
        /// <param name="sender">控制对象</param>
        public void OnFlowWork(object sender)
        {
            try
            {
                if (sender == null || string.IsNullOrEmpty(sender.ToString()))
                {
                    Console.WriteLine("未配置导航页面");
                    return;
                }
                string url = string.Format("/{0}/{1}", "Views", sender);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if(frame.Source == null)
                    {
                        frame.Source = new Uri(url, UriKind.Relative);
                    }
                    else
                    {
                        //frame.Source = new Uri("/", UriKind.Relative);
                        if (frame.Source.OriginalString == url.TrimStart('/'))
                        {
                            frame.Refresh();
                        }
                        else
                        {
                            frame.Source = new Uri(url, UriKind.Relative);
                        }
                    }

                    if (frame.CanGoBack)
                    {
                        frame.RemoveBackEntry();
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误：" + ex.Message);
            }
        }
    }
}
