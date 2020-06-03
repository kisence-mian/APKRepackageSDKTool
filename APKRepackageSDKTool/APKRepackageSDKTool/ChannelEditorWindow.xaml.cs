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
        PropertiesList propertiesList;

        static ChannelInfo s_CopyChannelInfo;

        private SelectInfo CurrentSelectInfo
        {
            get => selectInfo;

            set
            {
                selectInfo = value;
                selectInfo.Change();
            }
        }

        public PropertiesList _PropertiesList
        {
            get
            {
                if(propertiesList == null)
                {
                    ResetPropertiesList();
                }

                return propertiesList;
            }
            set
            {
                propertiesList = value;
                propertiesList.Change();

                EditorData.CurrentChannel.PropertiesList.Clear();
                for (int i = 0; i < propertiesList.Count; i++)
                {
                    EditorData.CurrentChannel.PropertiesList.Add(propertiesList[i]);
                }
            }
        }

        public ChannelEditorWindow()
        {
            InitializeComponent();
            UpdateUI();
        }

        void UpdateUI()
        {
            Grid_root.DataContext = EditorData.CurrentChannel;

            PasswordBox_keyStore.Password = EditorData.CurrentChannel.KeyStorePassWord;
            PasswordBox_alias.Password = EditorData.CurrentChannel.KeyStoreAliasPassWord;

            ListBox_SDKList.ItemsSource = EditorData.TotalSDKInfo;
            ListBox_SdkConfigList.ItemsSource = CurrentSelectInfo;
            ListBox_PropertiesList.ItemsSource = _PropertiesList;
        }

        void ResetPropertiesList()
        {
            propertiesList = new PropertiesList();
            for (int i = 0; i < EditorData.CurrentChannel.PropertiesList.Count; i++)
            {
                propertiesList.Add(EditorData.CurrentChannel.PropertiesList[i]);
            }
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

            Save();
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

        #region 属性编辑

        private void Button_AddProperties(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if(!string.IsNullOrEmpty(TextBox_PropertiesKey.Text))
            {
                KeyValue kv = new KeyValue();
                kv.key = TextBox_PropertiesKey.Text;

                TextBox_PropertiesKey.Text = "";

                _PropertiesList.Add(kv);

                _PropertiesList = _PropertiesList;
            }
        }

        private void Button_DeleteProperties(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string Key = (string)btn.Tag;
            for (int i = 0; i < _PropertiesList.Count; i++)
            {
                if(Key == _PropertiesList[i].key)
                {
                    _PropertiesList.RemoveAt(i);
                    break;
                }
            }

            _PropertiesList = _PropertiesList;
        }

        private void Button_QuickAddConfigKey(object sender, RoutedEventArgs e)
        {
            AddConfig(SDKInterfaceDefine.PropertiesKey_ChannelName, EditorData.CurrentChannel.Name);
            AddConfig(SDKInterfaceDefine.PropertiesKey_SelectNetworkPath, "https://xxx");
            AddConfig(SDKInterfaceDefine.PropertiesKey_UpdateDownLoadPath, "https://xxx");
            AddConfig(SDKInterfaceDefine.PropertiesKey_StoreName, EditorData.CurrentChannel.Name);
            AddConfig(SDKInterfaceDefine.PropertiesKey_LoginPlatform, "Tourist|AccountLogin|" + EditorData.CurrentChannel.Name);
            AddConfig(SDKInterfaceDefine.PropertiesKey_NetworkID, "0");
        }

        void AddConfig(string key, string value)
        {
            KeyValue kv = new KeyValue();
            kv.Key = key;
            kv.value = value;

            _PropertiesList.Add(kv);
            _PropertiesList = _PropertiesList;
        }

        #endregion

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

        public class PropertiesList : List<KeyValue>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public void Change()
            {
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged?.Invoke(this, e);
            }
        }

        private void Button_Click_Copy(object sender, RoutedEventArgs e)
        {
            s_CopyChannelInfo = (ChannelInfo)EditorData.CurrentChannel.Clone();

            string content = Serializer.Serialize(s_CopyChannelInfo);

            Clipboard.SetDataObject(content, true);

            MessageBox.Show("LanguageWindow.currentLanuage.Count " + EditorData.CurrentChannel.appNameLanguages.Count);
            MessageBox.Show("复制成功");
        }

        private void Button_Click_paste(object sender, RoutedEventArgs e)
        {
            if(s_CopyChannelInfo == null)
            {
                string content = Clipboard.GetText();
                if (!string.IsNullOrEmpty(content))
                {
                    try
                    {
                        Deserializer des = new Deserializer();
                        ChannelInfo info = des.Deserialize<ChannelInfo>(content);

                        PasteChannel(info);
                    }
                    catch
                    {
                        MessageBox.Show("剪贴板格式错误");
                    }
                }
                else
                {
                    MessageBox.Show("剪贴板无内容");
                }
            }
            else
            {
                PasteChannel(s_CopyChannelInfo);

                //string name = EditorData.CurrentChannel.channelName;
                //EditorData.CurrentChannel = s_CopyChannelInfo;
                //EditorData.CurrentChannel.Name = name;

                //for (int i = 0; i < EditorData.CurrentGameChannelList.Count; i++)
                //{
                //    if (EditorData.CurrentGameChannelList[i].channelName == name)
                //    {
                //        EditorData.CurrentGameChannelList[i] = EditorData.CurrentChannel;
                //    }
                //}

                //EditorData.CurrentGameChannelList = EditorData.CurrentGameChannelList;

                //ResetPropertiesList();
                //UpdateUI();
                //MessageBox.Show("粘贴成功");
            }
        }

        void PasteChannel(ChannelInfo info)
        {
            string name = EditorData.CurrentChannel.channelName;
            EditorData.CurrentChannel = info;
            EditorData.CurrentChannel.Name = name;

            for (int i = 0; i < EditorData.CurrentGameChannelList.Count; i++)
            {
                if (EditorData.CurrentGameChannelList[i].channelName == name)
                {
                    EditorData.CurrentGameChannelList[i] = EditorData.CurrentChannel;
                }
            }

            EditorData.CurrentGameChannelList = EditorData.CurrentGameChannelList;

            ResetPropertiesList();
            UpdateUI();
            MessageBox.Show("粘贴成功");
        }

        private void Button_Click_EditorLanguage(object sender, RoutedEventArgs e)
        {
            //if(EditorData.CurrentChannel.AppNameLanguages == null)
            //{
            //    EditorData.CurrentChannel.AppNameLanguages = new KeyValueList();
            //}

            LanguageWindow.SetLanguage(EditorData.CurrentChannel.AppNameLanguages);
            LanguageWindow.saveCallBack = Save;

            LanguageWindow cew = new LanguageWindow();
            cew.ShowDialog();
        }

        void Save()
        {
            EditorData.CurrentGameChannelList = EditorData.CurrentGameChannelList;
            MessageBox.Show("保存成功");
        }
    }
}
