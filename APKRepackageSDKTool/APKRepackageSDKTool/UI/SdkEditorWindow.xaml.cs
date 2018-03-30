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
    /// SdkEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SdkEditorWindow : Window
    {
        public SdkEditorWindow()
        {
            InitializeComponent();

            DataContext = EditorData.CurrentSDKConfig;

            ListBox_ConfigList.ItemsSource = EditorData.CurrentSDKConfig.sdkConfig;
        }

        private void Button_ClickSave(object sender, RoutedEventArgs e)
        {

        }
    }
}
