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
    /// LanguageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LanguageWindow : Window
    {
        public static SaveCallBack saveCallBack;
        static List<KeyValue> currentLanuage = null;

        public static KeyValueList _currentLanuage = new KeyValueList();

        List<string> allLanguage = new List<string>();

        public static void SetLanguage(List<KeyValue> lanuages)
        {
            currentLanuage = lanuages;

            _currentLanuage = new KeyValueList();
            _currentLanuage.AddRange(currentLanuage);
        }

        public LanguageWindow()
        {
            GenerateAllLanguage();
            InitializeComponent();

            ListBox_LanguageList.ItemsSource = CurrentLanuage;
            ComboBox_SelectLanguage.ItemsSource = allLanguage;
        }

        void GenerateAllLanguage()
        {
            allLanguage.Add("zh:中文");
            allLanguage.Add("zh-rCN:中文-中国大陆");
            allLanguage.Add("zh-rSG:中文-新加坡");
            allLanguage.Add("zh-rTW:繁体中文-台湾");
            allLanguage.Add("zh-rHK:繁体中文-香港");

            allLanguage.Add("en:英语");
            allLanguage.Add("en-rUS:英语-美国");
            allLanguage.Add("en-rGB:英语-英国");
            allLanguage.Add("en-rIN:英语-印度");
            allLanguage.Add("en-rAU:英语-澳大利亚");
            allLanguage.Add("en-rSG:英语-新加坡");

            allLanguage.Add("pt:葡萄牙语");
            allLanguage.Add("es:西班牙语");

            allLanguage.Add("ja:日语");
            allLanguage.Add("ko:韩语");
            allLanguage.Add("ru:俄语");

            allLanguage.Add("fr:法语");
            allLanguage.Add("de:德语");

            allLanguage.Add("hi:印度语");
            allLanguage.Add("mn:蒙古语");
            allLanguage.Add("km:柬埔寨语");
            allLanguage.Add("vi:越南语");
            allLanguage.Add("th:泰语");
            allLanguage.Add("ms:马来语");
            allLanguage.Add("in:印度尼西亚语");

            //保留r 名称以备用
            //allLanguage.Add("pt_rPT:葡萄牙语");
            //allLanguage.Add("fr_rFR:法语");
            //allLanguage.Add("de_rDE:德语");

            //allLanguage.Add("ko_rKR:韩语");
            //allLanguage.Add("mn_rMN:蒙古语");
            //allLanguage.Add("km_rKH:柬埔寨语");
            //allLanguage.Add("vi_rVN:越南语");
            //allLanguage.Add("th_rTH:泰语");
            //allLanguage.Add("ms_rMY:马来语");
        }

        public KeyValueList CurrentLanuage {
            get
            {
                if(_currentLanuage == null)
                {
                    _currentLanuage = new KeyValueList();
                    _currentLanuage.AddRange(currentLanuage);
                }

                return _currentLanuage;
            }
            set
            {
                _currentLanuage = value;
                _currentLanuage.Change();

                currentLanuage.Clear();
                currentLanuage.AddRange(_currentLanuage);
            }
        }

        private void Button_ClickAdd(object sender, RoutedEventArgs e)
        {
            if(ComboBox_SelectLanguage.SelectedValue != null)
            {
                string key = GetLanguageKey();
                if (!_currentLanuage.ContainsKey(key))
                {
                    KeyValue kv = new KeyValue();
                    kv.key = key;
                    CurrentLanuage.Add(kv);

                    CurrentLanuage = CurrentLanuage;
                }
                else
                {
                    MessageBox.Show("已存在的语言 " + key);
                }
            }
            else
            {
                MessageBox.Show("请选择语言 ");
            }

        }

        private void Button_ClickSave(object sender, RoutedEventArgs e)
        {
            saveCallBack?.Invoke();
        }

        string GetLanguageKey()
        {
            return ((string)ComboBox_SelectLanguage.SelectedValue).Split(':')[0];
        }

        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            string Key = (string)btn.Tag;
            for (int i = 0; i < CurrentLanuage.Count; i++)
            {
                if (Key == CurrentLanuage[i].key)
                {
                    CurrentLanuage.RemoveAt(i);
                    break;
                }
            }

            CurrentLanuage = CurrentLanuage;
        }
    }

    public delegate void SaveCallBack();
}
