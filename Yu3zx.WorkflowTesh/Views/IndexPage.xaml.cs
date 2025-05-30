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

namespace Yu3zx.WorkflowTesh.Views
{
    /// <summary>
    /// IndexPage.xaml 的交互逻辑
    /// </summary>
    public partial class IndexPage : Page
    {
        public IndexPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(WorkFlowEngine.CreateInstance().CurrentWorkFlow == null)
            {
                WorkFlow wfTemp = WorkFlowManager.CreateInstance().FindWorkFlow("default");
                WorkFlow wfCurrent = WorkFlowManager.CreateInstance().CopyWorkFlow(wfTemp);
                WorkFlowEngine.CreateInstance().CurrentWorkFlow = wfCurrent;

                ItemBase iB = WorkFlowEngine.CreateInstance().CurrentWorkFlow.FindStartItem();
                if (iB != null)
                {
                    WorkFlowEngine.CreateInstance().DoFlowNext(iB, "IndexPage");
                }
            }

            WorkFlowEngine.CreateInstance().SubMit(this.GetType().Name);
        }
    }
}
