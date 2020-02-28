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
using System.Windows.Shapes;

namespace APKRepackageSDKTool.UI
{
    /// <summary>
    /// OutPutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OutPutWindow : Window
    {

        string content = "";
        public OutPutWindow()
        {
            InitializeComponent();
        }


        public void ReceviceOutPut(string output)
        {
            content += output + "\n";

            Action ac = new Action(UpdateContent);
            Dispatcher.BeginInvoke(ac);
        }

        private void UpdateContent()
        {

            TextBox_output.Text = content;
        }
    }
}
