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
    /// TwoPage.xaml 的交互逻辑
    /// </summary>
    public partial class TwoPage : Page
    {
        public TwoPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var item = TagNodesManager.CreateInstance().FindTagNode("IsGoEnd", "是否直接结束");
            if(chkResult.IsChecked == true)
            {
                item.NodeValue = true;
            }
            else
            {
                item.NodeValue = false;
            }

            WorkFlowEngine.CreateInstance().SubMit(this.GetType().Name);
        }
    }
}
