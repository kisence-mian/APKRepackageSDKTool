using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

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
        KeyValueList manifestHeadList;
        KeyValueList usesList;
        KeyValueList applicationHeadList;
        StringList firstDexList;

        StringList mavenPathList;
        StringList mavenLibList;

        ActivityInfo currentActivityInfo;
        KeyValue currentMainActivityProperty;
        ServiceInfo currentServiceInfo;
        ProviderInfo currentProviderInfo;
        KeyValue currentMetaContent;
        KeyValue currentJavaContent;
        KeyValue currentManifestHeadContent;
        KeyValue currentUsesContent;
        KeyValue currentApplicationHead;

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

        private KeyValueList ManifestHeadList
        {
            get
            {
                if (manifestHeadList == null)
                {
                    manifestHeadList = new KeyValueList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.xmlHeadList.Count; i++)
                    {
                        manifestHeadList.Add(EditorData.CurrentSDKConfig.xmlHeadList[i]);
                    }
                }
                return manifestHeadList;
            }

            set
            {
                manifestHeadList = value;
                manifestHeadList.Change();

                EditorData.CurrentSDKConfig.xmlHeadList.Clear();
                for (int i = 0; i < manifestHeadList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.xmlHeadList.Add(manifestHeadList[i]);
                }
            }
        }

        private KeyValueList UsesList
        {
            get
            {
                if (usesList == null)
                {
                    usesList = new KeyValueList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.usesList.Count; i++)
                    {
                        usesList.Add(EditorData.CurrentSDKConfig.usesList[i]);
                    }
                }

                return usesList;
            }

            set
            {
                usesList = value;
                usesList.Change();

                EditorData.CurrentSDKConfig.usesList.Clear();
                for (int i = 0; i < usesList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.usesList.Add(usesList[i]);
                }
            }
        }

        private KeyValueList ApplicationHeadList
        {
            get
            {
                if (applicationHeadList == null)
                {
                    applicationHeadList = new KeyValueList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.applicationHeadList.Count; i++)
                    {
                        applicationHeadList.Add(EditorData.CurrentSDKConfig.applicationHeadList[i]);
                    }
                }

                return applicationHeadList;
            }

            set
            {
                applicationHeadList = value;
                applicationHeadList.Change();

                EditorData.CurrentSDKConfig.applicationHeadList.Clear();
                for (int i = 0; i < applicationHeadList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.applicationHeadList.Add(applicationHeadList[i]);
                }
            }
        }

        private StringList FirstDexList {
            get
            {
                if (firstDexList == null)
                {
                    firstDexList = new StringList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.firstDexList.Count; i++)
                    {
                        firstDexList.Add(EditorData.CurrentSDKConfig.firstDexList[i]);
                    }
                }

                return firstDexList;
            }

            set
            {
                firstDexList = value;
                firstDexList.Change();

                EditorData.CurrentSDKConfig.firstDexList.Clear();
                for (int i = 0; i < firstDexList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.firstDexList.Add(firstDexList[i]);
                }
            }
        }

        private StringList MavenPathList
        {
            get
            {
                if (mavenPathList == null)
                {
                    mavenPathList = new StringList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.mavenPathList.Count; i++)
                    {
                        mavenPathList.Add(EditorData.CurrentSDKConfig.mavenPathList[i]);
                    }
                }

                return mavenPathList;
            }

            set
            {
                mavenPathList = value;
                mavenPathList.Change();

                EditorData.CurrentSDKConfig.mavenPathList.Clear();
                for (int i = 0; i < mavenPathList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.mavenPathList.Add(mavenPathList[i]);
                }
            }
        }

        private StringList MavenLibList
        {
            get
            {
                if (mavenLibList == null)
                {
                    mavenLibList = new StringList();
                    for (int i = 0; i < EditorData.CurrentSDKConfig.mavenLibList.Count; i++)
                    {
                        mavenLibList.Add(EditorData.CurrentSDKConfig.mavenLibList[i]);
                    }
                }

                return mavenLibList;
            }

            set
            {
                mavenLibList = value;
                mavenLibList.Change();

                EditorData.CurrentSDKConfig.mavenLibList.Clear();
                for (int i = 0; i < mavenLibList.Count; i++)
                {
                    EditorData.CurrentSDKConfig.mavenLibList.Add(mavenLibList[i]);
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
            ListBox_ManifestHeadList.ItemsSource = ManifestHeadList;
            ListBox_UsesList.ItemsSource = UsesList;
            ListBox_ApplicationHeadList.ItemsSource = ApplicationHeadList;
            ListBox_FirstDexList.ItemsSource = FirstDexList;

            ListBox_MavenPath.ItemsSource = MavenPathList;
            ListBox_MavenLib.ItemsSource = MavenLibList;

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

        #region 自定义字段

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

        #endregion

        #region 首Dex包

        private void Button_ClickDeleteFirstDex(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string key = (string)btn.Tag;
            FirstDexList.Remove(key);

            FirstDexList = FirstDexList;
        }

        private void Button_ClickAddFirstDex(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_FirstDexName.Text))
            {
                FirstDexList.Add(TextBox_FirstDexName.Text);

                FirstDexList = FirstDexList;

                TextBox_FirstDexName.Text = "";
            }
        }

        #endregion

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

            if(openFileDialog.FileName.Contains(EditorData.SdkLibPath))
            {
                if (result == true)
                {
                    string path = openFileDialog.FileName.Replace(EditorData.SdkLibPath, "");

                    LibraryList.Add(path);
                    LibraryList = LibraryList;
                }
            }
            else
            {
                MessageBox.Show("导入的Jar包必须是SDKLibrary的子路径 \n" + EditorData.SdkLibPath);
            }
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

        #region ManifestXMLHead

        private void TextBox_ManifestHeadContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            currentManifestHeadContent.value = tb.Text;
        }

        private void Button_ClickDeleteManifestHead(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string key = (string)btn.Tag;

            for (int i = 0; i < ManifestHeadList.Count; i++)
            {
                if (ManifestHeadList[i].key == key)
                {
                    ManifestHeadList.RemoveAt(i);
                }
            }

            ManifestHeadList = ManifestHeadList;
        }

        private void Button_ClickAddManifestHead(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_ManifestHeadName.Text))
            {
                KeyValue kv = new KeyValue();
                kv.key = TextBox_ManifestHeadName.Text;

                ManifestHeadList.Add(kv);

                ManifestHeadList = ManifestHeadList;

                TextBox_ManifestHeadName.Text = "";
            }
        }

        private void ListBox_ManifestHeadList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;

            string Key = (string)lb.SelectedValue;

            for (int i = 0; i < ManifestHeadList.Count; i++)
            {
                if (ManifestHeadList[i].key == Key)
                {
                    currentManifestHeadContent = ManifestHeadList[i];
                }
            }

            TextBox_ManifestHeadContent.Text = currentManifestHeadContent.value;
            TextBox_ManifestHeadContent.IsEnabled = true;
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

        #region ApplicationHead

        private void TextBox_ApplicationHeadContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            currentApplicationHead.value = tb.Text;
        }

        private void Button_ClickDeleteApplicationHead(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string key = (string)btn.Tag;

            for (int i = 0; i < ApplicationHeadList.Count; i++)
            {
                if (ApplicationHeadList[i].key == key)
                {
                    ApplicationHeadList.RemoveAt(i);
                }
            }

            ApplicationHeadList = ApplicationHeadList;
        }

        private void Button_ClickAddApplicationHead(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_ApplicationHeadName.Text))
            {
                KeyValue kv = new KeyValue();
                kv.key = TextBox_ApplicationHeadName.Text;

                ApplicationHeadList.Add(kv);
                ApplicationHeadList = ApplicationHeadList;

                TextBox_ApplicationHeadName.Text = "";
            }
        }

        private void ListBox_ApplicationHeadList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;

            string Key = (string)lb.SelectedValue;

            for (int i = 0; i < ApplicationHeadList.Count; i++)
            {
                if (ApplicationHeadList[i].key == Key)
                {
                    currentApplicationHead = ApplicationHeadList[i];
                }
            }

            TextBox_ApplicationHeadContent.Text = currentApplicationHead.value;
            TextBox_ApplicationHeadContent.IsEnabled = true;
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

        #region Maven


        private void Button_ClickAddMavenPath(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_AddMavenPath.Text))
            {
                string value = TextBox_AddMavenPath.Text.TrimEnd('\\').TrimEnd('/');

                if (!MavenPathList.Contains(value))
                {
                    MavenPathList.Add(value);
                    MavenPathList = MavenPathList;
                }

                TextBox_AddMavenPath.Text = "";
            }
        }

        private void Button_mavenPath_Delete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string name = (string)btn.Tag;

            for (int i = 0; i < MavenPathList.Count; i++)
            {
                if (MavenPathList[i] == name)
                {
                    MavenPathList.RemoveAt(i);
                }
            }

            MavenPathList = MavenPathList;
        }

        private void Button_ClickAddMavenLib(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_AddMavenLib.Text))
            {
                string value = TextBox_AddMavenLib.Text.TrimEnd('\\').TrimEnd('/');
                if (!MavenLibList.Contains(value))
                {
                    MavenLibList.Add(value);
                    MavenLibList = MavenLibList;
                }

                TextBox_AddMavenLib.Text = "";
            }
        }

        private void Button_mavenLib_Delete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string name = (string)btn.Tag;

            for (int i = 0; i < MavenLibList.Count; i++)
            {
                if (MavenLibList[i] == name)
                {
                    MavenLibList.RemoveAt(i);
                }
            }

            MavenLibList = MavenLibList;
        }

        private void Button_AddCommonMaven(object sender, RoutedEventArgs e)
        {
            MavenPathList.Add("https://maven.aliyun.com/repository/google");
            MavenPathList.Add("https://maven.aliyun.com/repository/public");
            MavenPathList.Add("https://maven.aliyun.com/repository/jcenter");
            MavenPathList.Add("https://maven.aliyun.com/repository/gradle-plugin");

            MavenPathList.Add("https://repo1.maven.org/maven2");
            MavenPathList.Add("http://developer.huawei.com/repo");
            MavenPathList.Add("https://maven.google.com");

            MavenPathList.Add("https://maven.aliyun.com/repository/central");
            MavenPathList.Add("https://maven.aliyun.com/repository/gradle-plugin");
            MavenPathList.Add("https://maven.aliyun.com/repository/gradle-plugin");
            MavenPathList.Add("https://maven.aliyun.com/repository/spring");
            MavenPathList.Add("https://maven.aliyun.com/repository/spring-plugin");

            MavenPathList.Add("https://maven.aliyun.com/repository/grails-core");
            MavenPathList.Add("https://maven.aliyun.com/repository/apache-snapshots");

            MavenPathList = MavenPathList;
        }

        private void Button_TestConnectMaven(object sender, RoutedEventArgs e)
        {
            OutPutWindow opw = new OutPutWindow();
            opw.Show();

            var thread = new Thread(() =>
            {
                MavenTool mt = new MavenTool(EditorData.MavenCachePath,opw.ReceviceOutPut, opw.ReceviceErrorOutPut);
                mt.TestConnectMaven(MavenPathList);
            });

            thread.Start();
        }

        private void Button_PreDownLoadMavenLib(object sender, RoutedEventArgs e)
        {
            OutPutWindow opw = new OutPutWindow();
            opw.Show();

            MavenTool mt = new MavenTool(EditorData.MavenCachePath, opw.ReceviceOutPut, opw.ReceviceErrorOutPut);

            var thread = new Thread(() =>
            {
                mt.DowmLoadMaven(MavenPathList, MavenLibList);
            });

            thread.Start();
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

            if (key == currentMetaContent.key)
            {
                TextBox_MetaContent.IsEnabled = false;
                currentMetaContent.value = "";
            }
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

        #region Uses设置


        private void TextBox_UsesContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            currentUsesContent.value = tb.Text;
        }

        private void Button_ClickAddUsesHead(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextBox_UsesName.Text))
            {
                KeyValue kv = new KeyValue();
                kv.key = TextBox_UsesName.Text;

                UsesList.Add(kv);

                UsesList = UsesList;

                TextBox_UsesName.Text = "";
            }
        }

        private void Button_ClickDeleteUsesHead(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string key = (string)btn.Tag;

            for (int i = 0; i < UsesList.Count; i++)
            {
                if (UsesList[i].key == key)
                {
                    UsesList.RemoveAt(i);
                }
            }

            UsesList = UsesList;

            if(key == currentUsesContent.key)
            {
                TextBox_UsesContent.IsEnabled = false;
                currentUsesContent.value = "";
            }
        }

        private void ListBox_UsesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;

            string Key = (string)lb.SelectedValue;

            for (int i = 0; i < UsesList.Count; i++)
            {
                if (UsesList[i].key == Key)
                {
                    currentUsesContent = UsesList[i];
                }
            }

            TextBox_UsesContent.Text = currentUsesContent.value;
            TextBox_UsesContent.IsEnabled = true;
        }


        #endregion

        #region 从AAR中导入

        private void Button_InputAARGroup_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;  // 这里一定要设置true，不然就是选择文件

            string path = "";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
            }
            else
            {
                return;
            }

            string[] dires = Directory.GetDirectories(path);
            for (int i = 0; i < dires.Length; i++)
            {
                ExtractAAR(dires[i]);
            }
        }

        private void Button_InputAAR_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;  // 这里一定要设置true，不然就是选择文件

            string path = "";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
            }
            else
            {
                return;
            }

            ExtractAAR(path);
        }

        private void Button_GenerateRTable_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;  // 这里一定要设置true，不然就是选择文件

            string path = "";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
            }
            else
            {
                return;
            }

            OutPutWindow opw = new OutPutWindow();

            string aimPath = EditorData.SdkLibPath + "/" + EditorData.CurrentSDKConfig.sdkName;
            string aarName = FileTool.GetFileNameByPath(path);

            AndroidTool ct = new AndroidTool(opw.ReceviceOutPut, opw.ReceviceErrorOutPut);
            opw.Show();

            //生成R表
            ct.BuildRTable(path, aarName, aimPath);
        }

        private void Button_GenerateRTableGroup_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;  // 这里一定要设置true，不然就是选择文件

            string path = "";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
            }
            else
            {
                return;
            }

            OutPutWindow opw = new OutPutWindow();

            string sdkPath = EditorData.SdkLibPath + "/" + EditorData.CurrentSDKConfig.sdkName;
            AndroidTool ct = new AndroidTool(opw.ReceviceOutPut, opw.ReceviceErrorOutPut);
            opw.Show();

            string[] dires = Directory.GetDirectories(path);
            for (int i = 0; i < dires.Length; i++)
            {
                string aarName = FileTool.GetFileNameByPath(dires[i]);

                //生成R表
                ct.BuildRTable(dires[i], aarName, sdkPath);
            }
        }


        void ExtractAAR(string path)
        {
            string sdkPath = EditorData.SdkLibPath + "/" + EditorData.CurrentSDKConfig.sdkName;
            string aarName = FileTool.GetFileNameByPath(path);
            bool isConvert2AndroidX = CheckBox_ConvertAndroidX.IsChecked ?? false;

            OutPutWindow opw = new OutPutWindow();
            opw.Show();

            try
            {
                CompileTool cot = new CompileTool(opw.ReceviceOutPut, opw.ReceviceErrorOutPut);

                //路径有效性判断

                //提取jar
                string jarResult = ExtractJar(path, sdkPath, aarName, isConvert2AndroidX, cot);

                opw.ReceviceOutPut(jarResult);

                if (File.Exists(path + "/AndroidManifest.xml"))
                {
                    //提取Manifest
                    string manifestResult = ExtractManifest(path);
                    opw.ReceviceOutPut(manifestResult);
                }
                else
                {
                    opw.ReceviceOutPut("找不到 manifest 文件 " + path + "/AndroidManifest.xml");
                }

                string repeatAssets = "";
                //复制assets
                if (Directory.Exists(path + "/assets"))
                {
                    FileTool.CopyDirectory(path + "/assets", sdkPath + "/assets", (pathA, pathB) => {
                        repeatAssets += FileTool.GetFileNameByPath(pathA) + "\n";

                        opw.ReceviceOutPut(repeatAssets);
                    });
                }

                //复制assets
                if (Directory.Exists(path + "/jni"))
                {
                    FileTool.CopyDirectory(path + "/jni", sdkPath + "/lib", (pathA, pathB) => {
                        repeatAssets += FileTool.GetFileNameByPath(pathA) + "\n";

                        opw.ReceviceOutPut(repeatAssets);
                    });
                }

                //合并res
                if (Directory.Exists(path + "/res"))
                {
                    MergeResTool mergeRes = new MergeResTool(opw.ReceviceOutPut, opw.ReceviceErrorOutPut);
                    AndroidTool at = new AndroidTool(opw.ReceviceOutPut, opw.ReceviceErrorOutPut);

                    //合并Res
                    mergeRes.Merge(path + "/res", sdkPath + "/res");

                    //生成R表
                    at.BuildRTable(path, aarName, sdkPath);
                }
            }
            catch( Exception e)
            {
                opw.ReceviceOutPut(e.ToString());
            }
        }


        string ExtractJar(string sourcePath,string aimPath,string className,bool isConvert2AndroidX, CompileTool ct)
        {
            String result = "";
            string[] paths = Directory.GetFiles(sourcePath);
            foreach (var item in paths)
            {
                try
                {
                    string fileName = FileTool.GetFileNameByPath(item);

                    if (fileName == "classes.jar")
                    {
                        fileName = className + ".jar";
                    }

                    //获取父文件夹的名字，如果是libs才加Lib前缀
                    string parentsName = FileTool.GetFileParentDirectory(item);

                    //防止重名
                    if(parentsName == "libs")
                    {
                        fileName = "Lib_" + fileName;
                    }

                    if (item.EndsWith(".jar"))
                    {
                        //转换为AndroidX
                        if(isConvert2AndroidX)
                        {
                            fileName = FileTool.RemoveExpandName(fileName) + "_AndroidX.jar";
                            ct.Convert2AndroidX(item, aimPath + "/" + fileName);
                        }
                        else
                        {
                            File.Copy(item, aimPath + "/" + fileName,true);
                        }

                        result += fileName + "\n";
                    }
                }
                catch (Exception e)
                {
                    result += "Error " + e;
                }
            }

            string[] dires = Directory.GetDirectories(sourcePath);
            for (int i = 0; i < dires.Length; i++)
            {
                result += ExtractJar(dires[i], aimPath, className,isConvert2AndroidX,ct);
            }

            return result;
        }

        string ExtractManifest(string sourcePath)
        {
            string result = "";
            sourcePath += "/AndroidManifest.xml";

            XmlDocument doca = new XmlDocument();
            doca.Load(sourcePath);

            XmlNode manifest = doca.SelectSingleNode("manifest");
            XmlNode application = manifest.SelectSingleNode("application");

            for (int i = 0; i < manifest.ChildNodes.Count; i++)
            {
                XmlNode node = manifest.ChildNodes[i];

                //跳过注释
                if (node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                XmlElement ele = (XmlElement)node;

                //权限
                if (ele.Name == "uses-permission")
                {
                    string permissionString = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android").Replace("android.permission.", "");

                    if (!Permission.Contains(permissionString))
                    {
                        Permission.Add(permissionString);
                        Permission = Permission;

                        result += "permission " +  permissionString + "\n";
                    }
                }
                //meta以及其他
                else if (ele.Name == "meta")
                {
                    if (!HasMeta(ele.OuterXml))
                    {
                        //Meta
                        KeyValue metaKV = new KeyValue();
                        metaKV.key = ele.OuterXml;
                        metaKV.value = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "");

                        MetaList.Add(metaKV);
                        MetaList = MetaList;

                        result += ele.Name + " " + ele.OuterXml + "\n";
                    }
                }
            }

            if(application != null)
            for (int i = 0; i < application.ChildNodes.Count; i++)
            {
                XmlNode node = application.ChildNodes[i];

                //跳过注释
                if(node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                XmlElement ele = (XmlElement)node;

                //Activity
                if (ele.Name == "activity")
                {
                    string name = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                    if(!HasActivity(name))
                    {
                        ActivityInfo activityInfo = new ActivityInfo();

                        activityInfo.mainActivity = false;
                        activityInfo.name = name;
                        activityInfo.content = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"","");

                        _ActivityList.Add(activityInfo);
                        _ActivityList = _ActivityList;

                        result += "Activity " + name + "\n";
                    }
                }

                //Service
                if (ele.Name == "service")
                {
                    string name = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                    if(!HasService(name))
                    {
                        //Service
                        ServiceInfo serviceInfo = new ServiceInfo();

                        serviceInfo.name = name;
                        serviceInfo.content = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "");

                        _ServiceInfo.Add(serviceInfo);
                        _ServiceInfo = _ServiceInfo;

                        result += "Service " + name + "\n";
                    }
                }

                if (ele.Name == "provider")
                {
                    string name = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                    if (!HasProvider(name))
                    {
                        //Provider
                        ProviderInfo providerInfo = new ProviderInfo();

                        providerInfo.name = name;
                        providerInfo.content = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "").Replace("${ApplicationID}","{PackageName}");

                            _ProviderInfo.Add(providerInfo);

                        _ProviderInfo = _ProviderInfo;
                        result += "Provider " + name + "\n";
                    }
                }
            }
            return result;
        }

        bool HasActivity(string  name)
        {
            for (int i = 0; i < _ActivityList.Count; i++)
            {
                if(_ActivityList[i].name == name)
                {
                    return true;
                }
            }

            return false;
        }


        bool HasService(string name)
        {
            for (int i = 0; i < _ServiceInfo.Count; i++)
            {
                if (_ServiceInfo[i].name == name)
                {
                    return true;
                }
            }

            return false;
        }

        bool HasProvider(string name)
        {
            for (int i = 0; i < _ProviderInfo.Count; i++)
            {
                if (_ProviderInfo[i].name == name)
                {
                    return true;
                }
            }

            return false;
        }

        bool HasMeta(string value)
        {
            for (int i = 0; i < MetaList.Count; i++)
            {
                if (MetaList[i].value == value)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region 替换 AndroidX
        private void Button_BatchConvertAndroid_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;  // 这里一定要设置true，不然就是选择文件

            string path = "";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
            }
            else
            {
                return;
            }

            OutPutWindow opw = new OutPutWindow();
            opw.Show();

            CompileTool cot = new CompileTool(opw.ReceviceOutPut, opw.ReceviceErrorOutPut);

            List<string> allJar =  FileTool.GetAllFileNamesByPath(path, new string[] { "jar" }, true);
            for (int i = 0; i < allJar.Count; i++)
            {
                ConvertAndroidX(allJar[i],cot);
            }

            List<string> allAar = FileTool.GetAllFileNamesByPath(path, new string[] { "aar" }, true);
            for (int i = 0; i < allAar.Count; i++)
            {
                ConvertAARAndroidX(allAar[i], cot);
            }
        }

        private void Button_ConvertAndroid_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "JAR Files (*.jar)|*.jar|AAR Files (*.aar)|*.aar"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                OutPutWindow opw = new OutPutWindow();
                opw.Show();

                CompileTool cot = new CompileTool(opw.ReceviceOutPut, opw.ReceviceErrorOutPut);

                if(FileTool.GetExpandName( openFileDialog.FileName) == "aar")
                {
                    ConvertAARAndroidX(openFileDialog.FileName, cot);
                }
                else
                {
                    ConvertAndroidX(openFileDialog.FileName, cot);
                }
            }
        }

        void ConvertAndroidX(string jarPath,CompileTool cot)
        {
            string path = FileTool.GetFileDirectory(jarPath);
            string name = FileTool.RemoveExpandName(FileTool.GetFileNameByPath(jarPath));
            string newJarPath = path + "/" + name + "_AndroidX.jar";
            cot.Convert2AndroidX(jarPath ,  newJarPath );

            //删除旧目录
            FileTool.DeleteFile(jarPath);
        }

        void ConvertAARAndroidX(string aarPath, CompileTool cot)
        {
            string path = FileTool.GetFileDirectory(aarPath);
            string name = FileTool.RemoveExpandName(FileTool.GetFileNameByPath(aarPath));
            string newAarPath = path + "/" + name + "_AndroidX.aar";
            cot.Convert2AndroidX(aarPath, newAarPath);

            //删除旧目录
            FileTool.DeleteFile(aarPath);
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

        #region 提示按钮

        private void Button_ClickInfo(object sender, RoutedEventArgs e)
        {
            string content = "{PackageName} ${applicationId}会自动替换成包名\n";
            content += "{字段名}会自动替换成对应字段";

            MessageBox.Show(content);
        }

        private void Button_MianActivityInfo_Click(object sender, RoutedEventArgs e)
        {
            string content = "{PackageName} ${applicationId}会自动替换成包名\n";
            content += "{字段名}会自动替换成对应字段\n\n";

            content += "Manifest中同名的字段将被替换\n";
            content += "不需要全文输入，只需要填写 key 和 value 即可\n";

            MessageBox.Show(content);
        }




        #endregion


    }
}
