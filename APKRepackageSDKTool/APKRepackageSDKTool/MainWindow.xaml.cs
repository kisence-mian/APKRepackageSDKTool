using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace APKRepackageSDKTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        const string c_ConfigRecord = "Config";

        string apkPath;
        string keyStorePath;
        string exportPath;
        string SDKLibPath;

        List<ChannelInfo> currentGameChannelList = new List<ChannelInfo>();

        public List<ChannelInfo> CurrentGameChannelList
        {
            get => currentGameChannelList;
            set
            {
                currentGameChannelList = value;
                ListBox_channel.ItemsSource = currentGameChannelList;
             }
        }

        public MainWindow()
        {
            InitializeComponent();

            apkPath = RecordManager.GetRecord(c_ConfigRecord, "apkPath", "null");
            keyStorePath = RecordManager.GetRecord(c_ConfigRecord, "keyStorePath", "null");
            exportPath = RecordManager.GetRecord(c_ConfigRecord, "exportPath", "null");
            SDKLibPath = RecordManager.GetRecord(c_ConfigRecord, "SDKLibPath", "null");

            UpdateContent();

            //展示到游戏选择界面上
            ComboBox_gameList.ItemsSource = EditorData.GameList;
            ComboBox_gameList.SelectedIndex = RecordManager.GetRecord(c_ConfigRecord, "index", -1);

            //再显示该游戏的所有渠道
            UpdateChannel();
        }

        #region 点击事件

        private void Button_ClickSelectAPK(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("点击了click事件");

            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "APK Files (*.apk)|*.apk"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                this.Text_APKPath.Text = openFileDialog.FileName;
                apkPath = openFileDialog.FileName;
                RecordManager.SaveRecord(c_ConfigRecord, "apkPath", apkPath);
            }
        }

        private void Button_ClickSelectExportPath(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.Text_APKExportPath.Text = m_Dir;
            exportPath = m_Dir;
            RecordManager.SaveRecord(c_ConfigRecord, "exportPath", exportPath);
        }

        private void Button_ClickSelectSDKLibPath(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.Text_SDKLibPath.Text = m_Dir;
            SDKLibPath = m_Dir;
            RecordManager.SaveRecord(c_ConfigRecord, "SDKLibPath", SDKLibPath);
        }

        /// <summary>
        /// 通过执行cmd的办法去实现apk的反编译
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ClicRepackageAPK(object sender, RoutedEventArgs e)
        {
            RepackageInfo ri = new RepackageInfo();
            ri.apkPath = apkPath;
            ri.exportPath = exportPath;

            ChannelInfo ci = new ChannelInfo();
            ci.KeyStorePath = "C:\\Users\\GaiKai\\Desktop\\密钥\\fire.keystore";
            ci.KeyStorePassWord = "hello9123";
            ci.KeyStoreAlias = "huoyu";

            RepackageManager rm = new RepackageManager();
            rm.Repackage(ri, ci, RepackCallBack);
        }

        private void Button_ClickClean(object sender, RoutedEventArgs e)
        {
            line = 0;
            output.Clear();
            Text_output.Text = "";
        }

        private void Button_ClickEditorGame(object sender, RoutedEventArgs e)
        {
            GameSetting gs = new GameSetting();
            gs.Show();
        }

        private void Button_ClickDelete(object sender, RoutedEventArgs e)
        {

        }

        private void Button_ClickChannelConfig(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region 界面改变事件

        private void ComboBox_gameList_Selected(object sender, RoutedEventArgs e)
        {
            RecordManager.SaveRecord(c_ConfigRecord, "index", ComboBox_gameList.SelectedIndex);
            UpdateChannel();
        }

        #endregion

        #region 事件接收

        int line = 0;
        StringBuilder output = new StringBuilder();

        float progress = 0;
        string content;

        void RepackCallBack(float progress, string content, string output)
        {
            this.progress = progress;
            this.content = content;

            line++;
            this.output.Append("["+line+"]"+ output+"\n");

            Action ac = new Action(UpdateContent);
            Dispatcher.BeginInvoke(ac);
        }
        #endregion

        #region Update

        void UpdateContent()
        {
            Progress_repackage.Maximum = 5;
            Progress_repackage.Value = progress;

            Text_output.Text = output.ToString();
            Text_output.ScrollToEnd();

            Text_progressName.Content = content;

            Text_APKExportPath.Text = exportPath;
            Text_APKPath.Text = apkPath;
            //Text_KeyStorePath.Text = keyStorePath;
            Text_SDKLibPath.Text = SDKLibPath;
        }

        void UpdateChannel()
        {
            if(ComboBox_gameList.SelectedIndex != -1
                && ComboBox_gameList.SelectedIndex < EditorData.GameList.Count
                )
            {
                ListBox_channel.ItemsSource = EditorData.GameList[ComboBox_gameList.SelectedIndex].channelInfo;
            }
            else
            {
                ListBox_channel.ItemsSource = null;
            }
        }



        #endregion

        private void CheckBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.CheckBox cb = sender as System.Windows.Controls.CheckBox;

            

            Grid gd = cb.Parent as Grid;

            int index = -1;
            int count = VisualTreeHelper.GetChildrenCount(ListBox_channel);
            for (int i = 0; i < count; i++)
            {
                var item = VisualTreeHelper.GetChild(ListBox_channel, i);
                if (item == gd)
                {
                    index = i;
                }
            }

            System.Windows.MessageBox.Show("双击了 " + cb.Tag);
        }
    }
}
