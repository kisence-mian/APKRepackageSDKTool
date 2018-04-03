using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        PermissionList permissionList;
        KeyValueList configList;

        private PermissionList Permission
        {
            get
            {
                if(permissionList == null)
                {
                    permissionList = new PermissionList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.permissionList.Count; i++)
                    {
                        permissionList.Add(EditorData.CurrentSDKConfig.permissionList[i]);
                    }
                }

                return permissionList;
            }

            set
            {
                permissionList = value;
                permissionList.Change();

                EditorData.CurrentSDKConfig.permissionList.Clear();
                for (int i = 0; i < permissionList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.permissionList.Add(permissionList[i]);
                }
            }
        }

        private KeyValueList ConfigList
        {
            get
            {
                if (configList == null)
                {
                    configList = new KeyValueList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.configList.Count; i++)
                    {
                        configList.Add(EditorData.CurrentSDKConfig.configList[i]);
                    }
                }

                return configList;
            }

            set
            {
                configList = value;
                configList.Change();

                EditorData.CurrentSDKConfig.configList.Clear();
                for (int i = 0; i < configList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.configList.Add(configList[i]);
                }
            }
        }

        public SdkEditorWindow()
        {
            InitializeComponent();

            DataContext = EditorData.CurrentSDKConfig;

            ListBox_ConfigList.ItemsSource = ConfigList;
            ListBox_PermissionList.ItemsSource = Permission;
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

                ConfigList.Add(kv);

                ConfigList = ConfigList;

                TextBox_ConfigName.Text = "";
            }
        }

        private void Button_ClickDelete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string key = (string)btn.Tag;

            for (int i = 0; i < ConfigList.Count; i++)
            {
                if(ConfigList[i].key == key)
                {
                    ConfigList.RemoveAt(i);
                }
            }

            ConfigList = ConfigList;
        }

        private void Button_ClickAddPermission(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(TextBox_PermissionName.Text))
            {
                Permission.Add(TextBox_PermissionName.Text);

                Permission = Permission;

                TextBox_PermissionName.Text = "";
            }
        }

        private void Button_ClickDeletePermission(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string key = (string)btn.Tag;

            Permission.Remove(key);

            Permission = Permission;
        }

        class PermissionList : List<string>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public void Change()
            {
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged?.Invoke(this, e);
            }
        }

        class KeyValueList : List<KeyValue>,INotifyCollectionChanged
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
