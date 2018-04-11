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
        List<SDKTypeCheck> checkList;

        PermissionList permissionList;
        KeyValueList configList;
        ActivityList activityList;
        ServiceList serviceList;

        ActivityInfo currentActivityInfo;
        ServiceInfo currentServiceInfo;

        #region 属性

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

        private ActivityList _ActivityList
        {
            get
            {
                if (activityList == null)
                {
                    activityList = new ActivityList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.activityInfoList.Count; i++)
                    {
                        activityList.Add(EditorData.CurrentSDKConfig.activityInfoList[i]);
                    }
                }

                return activityList;
            }

            set
            {
                activityList = value;
                activityList.Change();

                EditorData.CurrentSDKConfig.activityInfoList.Clear();
                for (int i = 0; i < activityList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.activityInfoList.Add(activityList[i]);
                }
            }
        }

        private ServiceList _ServiceInfo
        {
            get
            {
                if (serviceList == null)
                {
                    serviceList = new ServiceList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.serviceInfoList.Count; i++)
                    {
                        serviceList.Add(EditorData.CurrentSDKConfig.serviceInfoList[i]);
                    }
                }

                return serviceList;
            }

            set
            {
                serviceList = value;
                serviceList.Change();

                EditorData.CurrentSDKConfig.serviceInfoList.Clear();
                for (int i = 0; i < serviceList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.serviceInfoList.Add(serviceList[i]);
                }
            }
        }

        private List<SDKTypeCheck> CheckList
        {
            get
            {
                if(checkList == null)
                {
                    checkList = new List<SDKTypeCheck>();
                    foreach (SDKType item in Enum.GetValues(typeof(SDKType)))
                    {
                        SDKTypeCheck sc = new SDKTypeCheck();
                        sc.Name = item.ToString();
                        sc.type = item;
                        sc.IsChick = 0 != (item & EditorData.CurrentSDKConfig.SdkType);

                        checkList.Add(sc);
                    }

                }

                return checkList;
            }


            set
            {
                checkList = value;
                EditorData.CurrentSDKConfig.SdkType = 0;

                for (int i = 0; i < checkList.Count; i++)
                {
                    if(checkList[i].IsChick)
                    {
                        EditorData.CurrentSDKConfig.SdkType = EditorData.CurrentSDKConfig.SdkType | checkList[i].type;
                    }
                }

            }
        }

        #endregion

        public SdkEditorWindow()
        {
            InitializeComponent();

            DataContext = EditorData.CurrentSDKConfig;

            ListBox_ConfigList.ItemsSource = ConfigList;
            ListBox_PermissionList.ItemsSource = Permission;
            ListBox_ActivtyList.ItemsSource = _ActivityList;
            ListBox_serviceList.ItemsSource = _ServiceInfo;

            BindingEnum();
        }

        #region 枚举绑定

        public void BindingEnum()
        {
            ListBox_SDKType.ItemsSource = CheckList;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckList = CheckList;
        }

        #endregion

        #region 点击事件

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

        #region Activtity

        private void Button_ClickAddActivity(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_ActivityName.Text))
            {
                ActivityInfo ai = new ActivityInfo();
                ai.Name = TextBox_ActivityName.Text;

                _ActivityList.Add(ai);

                _ActivityList = _ActivityList;

                TextBox_ActivityName.Text = "";
            }
        }

        private void Button_ClickDeleteActivity(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string name = (string)btn.Tag;

            for (int i = 0; i < _ActivityList.Count; i++)
            {
                if (_ActivityList[i].Name == name)
                {
                    _ActivityList.RemoveAt(i);
                }
            }

            _ActivityList = _ActivityList;
        }

        private void ListBox_ActivtyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;

            string Name = (string)lb.SelectedValue;

            for (int i = 0; i <_ActivityList.Count; i++)
            {
                if(_ActivityList[i].name == Name)
                {
                   currentActivityInfo = _ActivityList[i];
                }
            }

            TextBox_ActivityContent.Text = currentActivityInfo.Content;
        }

        private void TextBox_ActivityContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            currentActivityInfo.Content = tb.Text;
        }

        #endregion

        #region Service

        private void Button_ClickAddService(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_serviceName.Text))
            {
                ServiceInfo ai = new ServiceInfo();
                ai.Name = TextBox_serviceName.Text;

                _ServiceInfo.Add(ai);

                _ServiceInfo = _ServiceInfo;

                TextBox_serviceName.Text = "";
            }
        }

        private void Button_ClickDeleteService(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string name = (string)btn.Tag;

            for (int i = 0; i < _ServiceInfo.Count; i++)
            {
                if (_ServiceInfo[i].Name == name)
                {
                    _ServiceInfo.RemoveAt(i);
                }
            }

            _ServiceInfo = _ServiceInfo;
        }

        private void ListBox_ServiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;

            string Name = (string)lb.SelectedValue;

            for (int i = 0; i < _ServiceInfo.Count; i++)
            {
                if (_ServiceInfo[i].name == Name)
                {
                    currentServiceInfo = _ServiceInfo[i];
                }
            }

            TextBox_serviceContent.Text = currentServiceInfo.Content;
        }

        private void TextBox_serviceContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            currentServiceInfo.Content = tb.Text;
        }

        #endregion

        #endregion

        #region 类声明

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

        class ActivityList : List<ActivityInfo>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public void Change()
            {
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged?.Invoke(this, e);
            }
        }

        class ServiceList : List<ServiceInfo>, INotifyCollectionChanged
        {
            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public void Change()
            {
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                CollectionChanged?.Invoke(this, e);
            }
        }

        class SDKTypeCheck
        {
            public SDKType type;
            private string name;
            private bool isChick;

            public string Name { get => name; set => name = value; }
            public bool IsChick { get => isChick; set => isChick = value; }
        }



        #endregion


    }
}
