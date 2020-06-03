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
        bool isError = false;
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

        public void ReceviceErrorOutPut(string output)
        {
            content += output + "\n";

            Action ac = new Action(UpdateContent);
            Dispatcher.BeginInvoke(ac);

            isError = true;
        }

        private void UpdateContent()
        {
            if(isError)
            {
                Title = "错误输出！";
            }

            TextBox_output.Text = content;
        }
    }
}
