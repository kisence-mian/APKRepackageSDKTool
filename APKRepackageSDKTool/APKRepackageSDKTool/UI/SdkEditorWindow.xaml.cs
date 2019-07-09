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

        StringList permissionList;
        KeyValueList configList;
        ActivityList activityList;
        ServiceList serviceList;
        KeyValueList mainActivityPropertyList;
        ProviderList providerList;
        KeyValueList metaList;
        StringList libraryList;
        KeyValueList customJavaList;

        ActivityInfo currentActivityInfo;
        KeyValue currentMainActivityProperty;
        ServiceInfo currentServiceInfo;
        ProviderInfo currentProviderInfo;
        KeyValue currentMetaContent;
        KeyValue currentJavaContent;

        #region 属性

        private StringList Permission
        {
            get
            {
                if(permissionList == null)
                {
                    permissionList = new StringList();
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

        private StringList LibraryList
        {
            get
            {
                if (libraryList == null)
                {
                    libraryList = new StringList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.customJavaLibrary.Count; i++)
                    {
                        libraryList.Add(EditorData.CurrentSDKConfig.customJavaLibrary[i]);
                    }
                }

                return libraryList;
            }

            set
            {
                libraryList = value;
                libraryList.Change();

                EditorData.CurrentSDKConfig.customJavaLibrary.Clear();
                for (int i = 0; i < libraryList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.customJavaLibrary.Add(libraryList[i]);
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

        private KeyValueList CustomJavaList
        {
            get
            {
                if (customJavaList == null)
                {
                    customJavaList = new KeyValueList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.CustomJavaClass.Count; i++)
                    {
                        customJavaList.Add(EditorData.CurrentSDKConfig.CustomJavaClass[i]);
                    }
                }

                return customJavaList;
            }

            set
            {
                customJavaList = value;
                customJavaList.Change();

                EditorData.CurrentSDKConfig.CustomJavaClass.Clear();
                for (int i = 0; i < customJavaList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.CustomJavaClass.Add(customJavaList[i]);
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

        private KeyValueList _MainActivityProperty
        {
            get
            {
                if (mainActivityPropertyList == null)
                {
                    mainActivityPropertyList = new KeyValueList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.mainActivityPropertyList.Count; i++)
                    {
                        mainActivityPropertyList.Add(EditorData.CurrentSDKConfig.mainActivityPropertyList[i]);
                    }
                }

                return mainActivityPropertyList;
            }

            set
            {
                mainActivityPropertyList = value;
                mainActivityPropertyList.Change();

                EditorData.CurrentSDKConfig.mainActivityPropertyList.Clear();
                for (int i = 0; i < mainActivityPropertyList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.mainActivityPropertyList.Add(mainActivityPropertyList[i]);
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

        private ProviderList _ProviderInfo
        {
            get
            {
                if (providerList == null)
                {
                    providerList = new ProviderList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.providerInfoList.Count; i++)
                    {
                        providerList.Add(EditorData.CurrentSDKConfig.providerInfoList[i]);
                    }
                }

                return providerList;
            }

            set
            {
                providerList = value;
                providerList.Change();

                EditorData.CurrentSDKConfig.providerInfoList.Clear();
                for (int i = 0; i < providerList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.providerInfoList.Add(providerList[i]);
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

        private KeyValueList MetaList
        {
            get
            {
                if (metaList == null)
                {
                    metaList = new KeyValueList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.metaInfoList.Count; i++)
                    {
                        metaList.Add(EditorData.CurrentSDKConfig.metaInfoList[i]);
                    }
                }

                return metaList;
            }

            set
            {
                metaList = value;
                metaList.Change();

                EditorData.CurrentSDKConfig.metaInfoList.Clear();
                for (int i = 0; i < metaList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.metaInfoList.Add(metaList[i]);
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
            ListBox_ProviderList.ItemsSource = _ProviderInfo;
            ListBox_MetaList.ItemsSource = MetaList;
            ListBox_CustomJava.ItemsSource = CustomJavaList;
            ListBox_CustomLibrary.ItemsSource = LibraryList;
            ListBox_mainPropertyList.ItemsSource = _MainActivityProperty;

            BindingEnum();

            if (CheckBox_customJavaClass.IsChecked == true)
            {
                Grid_customJava.IsEnabled = true;
            }
            else
            {
                Grid_customJava.IsEnabled = false;
            }
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

        #region 自定义Java类

        private void CheckBox_customJavaClass_checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (checkBox.IsChecked == true)
            {
                Grid_customJava.IsEnabled = true;
                EditorData.CurrentSDKConfig.useCustomJavaClass = true;
            }
            else
            {
                Grid_customJava.IsEnabled = false;
                EditorData.CurrentSDKConfig.useCustomJavaClass = false;
            }
        }

        private void Button_addCustomJavaLibrary_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Jar Files (*.*)|*.*"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                LibraryList.Add(openFileDialog.FileName);
            }

            LibraryList = LibraryList;
        }

        private void Button_CustomJavaLibrary_Delete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string key = (string)btn.Tag;

            for (int i = 0; i < LibraryList.Count; i++)
            {
                if (LibraryList[i] == key)
                {
                    LibraryList.RemoveAt(i);
                }
            }

            LibraryList = LibraryList;
        }

        private void Button_addCustomJava_Click(object sender, RoutedEventArgs e)
        {
            if(TextBox_customJavaName.Text != "")
            {
                KeyValue keyValue = new KeyValue();
                keyValue.key = TextBox_customJavaName.Text;
                keyValue.value = "";

                TextBox_customJavaName.Text = "";

                CustomJavaList.Add(keyValue);
            }

            CustomJavaList = customJavaList;
        }

        private void Button_addCustomJava_Delete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string key = (string)btn.Tag;

            for (int i = 0; i < customJavaList.Count; i++)
            {
                if (customJavaList[i].key == key)
                {
                    customJavaList.RemoveAt(i);
                }
            }

            CustomJavaList = customJavaList;
        }

        private void ListBox_CustomJava_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //自动保存
            EditorData.SaveSDKConfig(EditorData.CurrentSDKConfig);

            ListBox lb = sender as ListBox;

            string Name = (string)lb.SelectedValue;

            for (int i = 0; i < customJavaList.Count; i++)
            {
                if (customJavaList[i].key == Name)
                {
                    currentJavaContent = customJavaList[i];
                }
            }

            TextBox_ClassTemplate.Text = currentJavaContent.value;
        }

        private void TextBox_ClassTemplate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (currentJavaContent != null)
            {
                currentJavaContent.value = TextBox_ClassTemplate.Text;

                //Encoding utf8 = Encoding.UTF8;
                //Encoding gb2312 = Encoding.GetEncoding("GB2312");

                //byte[] gb = gb2312.GetBytes(TextBox_ClassTemplate.Text);
                //gb = Encoding.Convert(gb2312, utf8, gb);

                //currentJavacontent.value = utf8.GetString(gb);
            }
        }

        #endregion

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
            TextBox_ActivityContent.IsEnabled = true;
            CheckBox_main.IsChecked = currentActivityInfo.MainActivity;
        }

        private void TextBox_ActivityContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            currentActivityInfo.Content = tb.Text;
        }

        private void CheckBox_main_Checked(object sender, RoutedEventArgs e)
        {
            currentActivityInfo.MainActivity = true;
        }

        private void CheckBox_main_Unchecked(object sender, RoutedEventArgs e)
        {
            currentActivityInfo.MainActivity = false;
        }

        #endregion

        #region MainActivityProperty

        private void Button_ClickAddMainActivityProperty(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_mainPropertyName.Text))
            {
                KeyValue kv = new KeyValue();
                kv.key = TextBox_mainPropertyName.Text;

                _MainActivityProperty.Add(kv);

                _MainActivityProperty = _MainActivityProperty;

                TextBox_mainPropertyName.Text = "";
            }
        }

        private void Button_ClickDeleteMainActivityProperty(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string name = (string)btn.Tag;

            for (int i = 0; i < _MainActivityProperty.Count; i++)
            {
                if (_MainActivityProperty[i].key == name)
                {
                    _MainActivityProperty.RemoveAt(i);
                }
            }

            _MainActivityProperty = _MainActivityProperty;
        }

        private void ListBox_MainActivityPropertyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBox_mainPropertyContent.IsEnabled = true;

            ListBox lb = sender as ListBox;

            string Name = (string)lb.SelectedValue;

            for (int i = 0; i < _MainActivityProperty.Count; i++)
            {
                if (_MainActivityProperty[i].key == Name)
                {
                    currentMainActivityProperty = _MainActivityProperty[i];
                }
            }

            TextBox_mainPropertyContent.Text = currentMainActivityProperty.value;
        }

        private void TextBox_MainActivityPropertyContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            currentMainActivityProperty.value = tb.Text;
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
            TextBox_serviceContent.IsEnabled = true;
        }

        private void TextBox_serviceContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            currentServiceInfo.Content = tb.Text;
        }

        #endregion

        #region Provider

        private void Button_ClickAddProvider(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_ProviderName.Text))
            {
                ProviderInfo pi = new ProviderInfo();
                pi.Name = TextBox_ProviderName.Text;

                _ProviderInfo.Add(pi);

                _ProviderInfo = _ProviderInfo;

                TextBox_ProviderName.Text = "";
            }
        }

        private void Button_ClickDeleteProvider(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string name = (string)btn.Tag;

            for (int i = 0; i < _ProviderInfo.Count; i++)
            {
                if (_ProviderInfo[i].Name == name)
                {
                    _ProviderInfo.RemoveAt(i);
                }
            }

            _ProviderInfo = _ProviderInfo;
        }

        private void ListBox_ProviderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;

            string Name = (string)lb.SelectedValue;

            for (int i = 0; i < _ProviderInfo.Count; i++)
            {
                if (_ProviderInfo[i].name == Name)
                {
                    currentProviderInfo = _ProviderInfo[i];
                }
            }

            TextBox_ProviderContent.Text = currentProviderInfo.Content;
        }

        private void TextBox_ProviderContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            currentProviderInfo.Content = tb.Text;
        }

        #endregion

        #region META

        private void Button_ClickAddMeta(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_MetaName.Text))
            {
                KeyValue kv = new KeyValue();
                kv.key = TextBox_MetaName.Text;

                MetaList.Add(kv);

                MetaList = MetaList;

                TextBox_MetaName.Text = "";
            }
        }

        private void Button_ClickDeleteMeta(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string key = (string)btn.Tag;

            for (int i = 0; i < MetaList.Count; i++)
            {
                if (MetaList[i].key == key)
                {
                    MetaList.RemoveAt(i);
                }
            }

            MetaList = MetaList;
        }

        private void ListBox_MetaList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;

            string Key = (string)lb.SelectedValue;

            for (int i = 0; i < MetaList.Count; i++)
            {
                if (MetaList[i].key == Key)
                {
                    currentMetaContent = MetaList[i];
                }
            }

            TextBox_MetaContent.Text = currentMetaContent.value;
            TextBox_MetaContent.IsEnabled = true;
        }

        private void TextBox_MetaContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            currentMetaContent.value = tb.Text;
        }

        #endregion

        #endregion

        #region 类声明

        class StringList : List<string>, INotifyCollectionChanged
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

        class ProviderList : List<ProviderInfo>, INotifyCollectionChanged
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

        private void Button_ClickInfo(object sender, RoutedEventArgs e)
        {
            string content = "{PackageName}会自动替换成包名\n";
            content += "{字段名}会自动替换成对应字段";

            MessageBox.Show(content);
        }


    }
}
