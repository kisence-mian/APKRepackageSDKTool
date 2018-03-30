using System;
using System.Collections.Generic;
using System.IO;
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

namespace APKRepackageSDKTool
{
    /// <summary>
    /// ChannelEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelEditorWindow : Window
    {
        public ChannelEditorWindow()
        {
            InitializeComponent();

            Grid_root.DataContext = EditorData.CurrentChannel;

            PasswordBox_keyStore.Password = EditorData.CurrentChannel.KeyStorePassWord;
            PasswordBox_alias.Password = EditorData.CurrentChannel.KeyStoreAliasPassWord;

            ListBox_SDKList.ItemsSource = EditorData.TotalSDKInfo;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "KeyStore Files (*.keystore)|*.keystore"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                this.TextBox_keyStore.Text = openFileDialog.FileName;
                EditorData.CurrentChannel.keyStorePath = openFileDialog.FileName;
            }
        }

        private void Button_Click_SelectIcon(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "KeyStore Files (*.png)|*.png"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                this.TextBox_Icon.Text = openFileDialog.FileName;
                Image_icon.Source = new BitmapImage(new Uri("pack://" + openFileDialog.FileName));
                EditorData.CurrentChannel.AppIcon = openFileDialog.FileName;
            }
        }

        private void Button_Click_SelectBanner(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "KeyStore Files (*.png)|*.png"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                this.TextBox_Banner.Text = openFileDialog.FileName;
                Image_Banner.Source = new BitmapImage(new Uri("pack://" + openFileDialog.FileName));
                EditorData.CurrentChannel.AppBanner = openFileDialog.FileName;
            }
        }

        private void Button_ClickSave(object sender, RoutedEventArgs e)
        {
            EditorData.CurrentChannel.KeyStorePassWord = PasswordBox_keyStore.Password;
            EditorData.CurrentChannel.KeyStoreAliasPassWord = PasswordBox_alias.Password;

            EditorData.CurrentGameChannelList = EditorData.CurrentGameChannelList;
            MessageBox.Show("保存成功");
        }

        private void Button_Click_AddSDK(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty( TextBox_SDKName.Text)
                && !string.IsNullOrEmpty(EditorData.SdkLibPath)
                )
            {
                string sdkName = TextBox_SDKName.Text;
                string path = EditorData.SdkLibPath + "\\" + sdkName;

                //在SdkLibPath新建一个文件夹
                DirectoryInfo dir = new DirectoryInfo(path);
                dir.Create();//自行判断一下是否存在。

                //存放一个config文件
                SDKConfig config = new SDKConfig();
                config.SdkName = sdkName;
                EditorData.SaveSDKConfig(path + "\\config.json", config);

                //更新SDKList
                EditorData.UpdateTotalSDKInfo();
            }
        }

        private void CheckBox_MouseDoubleClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
