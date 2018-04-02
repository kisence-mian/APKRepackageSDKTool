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

            ListBox_ConfigList.ItemsSource = EditorData.CurrentSDKConfig;
        }

        private void Button_ClickSave(object sender, RoutedEventArgs e)
        {
            EditorData.SaveSDKConfig(EditorData.CurrentSDKConfig);
            MessageBox.Show("保存成功");
        }

        private void Button_ClickAddConfigKey(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(TextBox_ConfigName.Text))
            {
                KeyValue kv = new KeyValue();
                kv.Key = TextBox_ConfigName.Text;

                EditorData.CurrentSDKConfig.Add(kv);

                EditorData.CurrentSDKConfig = EditorData.CurrentSDKConfig;

                TextBox_ConfigName.Text = "";
            }
        }

        private void Button_ClickDelete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string key = (string)btn.Tag;

            for (int i = 0; i < EditorData.CurrentSDKConfig.Count; i++)
            {
                if(EditorData.CurrentSDKConfig[i].key == key)
                {
                    EditorData.CurrentSDKConfig.RemoveAt(i);
                }
            }

            EditorData.CurrentSDKConfig = EditorData.CurrentSDKConfig;
        }
    }
}
