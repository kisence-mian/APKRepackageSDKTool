using APKRepackageSDKTool.UI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        SelectInfo selectInfo = new SelectInfo();

        private SelectInfo CurrentSelectInfo { get => selectInfo;
            set
            {
                selectInfo = value;
                selectInfo.Change();
            }
        }

        public ChannelEditorWindow()
        {
            InitializeComponent();

            Grid_root.DataContext = EditorData.CurrentChannel;

            PasswordBox_keyStore.Password = EditorData.CurrentChannel.KeyStorePassWord;
            PasswordBox_alias.Password = EditorData.CurrentChannel.KeyStoreAliasPassWord;

            ListBox_SDKList.ItemsSource = EditorData.TotalSDKInfo;
            ListBox_SdkConfigList.ItemsSource = CurrentSelectInfo;
        }

        #region 点击事件

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

                //存放一个config文件
                SDKConfig config = new SDKConfig();
                config.SdkName = sdkName;
                EditorData.SaveSDKConfig (config);

                //更新SDKList
                EditorData.UpdateTotalSDKInfo();

                TextBox_SDKName.Text = "";
            }
        }

        private void Button_ClickEditorSDK(object sender, RoutedEventArgs e)
        {
            Button cb = sender as Button;
            string sdkName = (string)cb.Tag;

            EditorData.SetCurrentSDKConfig(sdkName);

            SdkEditorWindow sew = new SdkEditorWindow();
            sew.ShowDialog();
        }


        private void Button_ClickSDKView(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string sdkName = (string)btn.Tag;

            SDKInfo info = EditorData.CurrentChannel.GetSDKInfo(sdkName);
            if (info != null)
            {
                CurrentSelectInfo.Clear();

                for (int i = 0; i < info.sdkConfig.Count; i++)
                {
                    CurrentSelectInfo.Add(info.sdkConfig[i]);
                }
            }
            else
            {
                CurrentSelectInfo.Clear();
            }

            CurrentSelectInfo = CurrentSelectInfo;
        }

        #endregion

        #region UI逻辑

        #endregion

        class SelectInfo : List<KeyValue>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public void Change()
            {
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged?.Invoke(this, e);
            }
        }

    }
}
