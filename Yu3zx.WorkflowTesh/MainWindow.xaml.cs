using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Yu3zx.WorkFlowCore;

namespace Yu3zx.WorkflowTesh
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AppManager.frame = mainFrame;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppManager.CreateInstance().Init();

            WorkFlow wfTemp = WorkFlowManager.CreateInstance().FindWorkFlow("default");
            WorkFlow wfCurrent = WorkFlowManager.CreateInstance().CopyWorkFlow(wfTemp);
            WorkFlowEngine.CreateInstance().CurrentWorkFlow = wfCurrent;

            ItemBase iB = WorkFlowEngine.CreateInstance().CurrentWorkFlow.FindStartItem();
            if (iB != null)
            {
                WorkFlowEngine.CreateInstance().DoFlowNext(iB, "IndexPage");
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
