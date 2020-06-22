using APKRepackageSDKTool.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        const int c_totalStep = 9;
        int currentTotalStep = 9;

        RepackageManager repackageManager = new RepackageManager();
        bool isBuilding = false;

        string gameName;
        string apkPath;
        string keyStorePath;
        string exportPath;

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
            try
            {
                InitializeComponent();

                //apkPath = RecordManager.GetRecord(EditorData.c_ConfigRecord, "apkPath", "null");
                //exportPath = RecordManager.GetRecord(EditorData.c_ConfigRecord, "exportPath", "null");
                keyStorePath = RecordManager.GetRecord(EditorData.c_ConfigRecord, "keyStorePath", "null");

                //展示到游戏选择界面上
                ComboBox_gameList.ItemsSource = EditorData.GameList;
                ComboBox_gameList.SelectedIndex = RecordManager.GetRecord(EditorData.c_ConfigRecord, "index", -1);

                //再显示该游戏的所有渠道
                UpdateChannel();

                //更新界面
                UpdateContent();
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("启动出错："+ex.ToString());
            }


            //测试代码
            //ChannelTool ct = new ChannelTool(OutPut, OutPut);
            //Dictionary<string, string> am = new Dictionary<string, string>();
            //ct.SplitDex(@"E:\Project\Tool\APKRepackageSDKTool\APKRepackageSDKTool\APKRepackageSDKTool\bin\Debug\CPH20200114_1540");

            //ct.CalcSmaliMethodCountBySmali(@"E:/Project/Tool/APKRepackageSDKTool/APKRepackageSDKTool/APKRepackageSDKTool/bin/Debug/CPH20200114_1540/smali/com/google/android/gms/internal/ads/zzbwc.smali", am);
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
                //RecordManager.SaveRecord(EditorData.c_ConfigRecord, "apkPath", apkPath);
                EditorData.SaveAPKPath(gameName,apkPath);
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
            //RecordManager.SaveRecord(EditorData.c_ConfigRecord, "exportPath", exportPath);
            EditorData.SaveExportAPKPath(gameName, exportPath);
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
            EditorData.SdkLibPath = m_Dir;
        }

        private void Button_ClickAndroidSDKPath(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.Text_AndroidAPKPath.Text = m_Dir;
            EditorData.AndroidSdkPath = m_Dir;
        }

        private void Button_ClickJetifierPath(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            this.Text_jetifierPath.Text = m_Dir;
            EditorData.JetifierPath = m_Dir;
        }

        /// <summary>
        /// 点击重打包
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ClicRepackageAPK(object sender, RoutedEventArgs e)
        {
            if(!isBuilding)
            {
                //获取ChannelInfo
                List<ChannelInfo> list = new List<ChannelInfo>();
                for (int i = 0; i < EditorData.CurrentGameChannelList.Count; i++)
                {
                    if (EditorData.CurrentGameChannelList[i].isChecked)
                    {
                        list.Add( EditorData.CurrentGameChannelList[i]);
                    }
                }

                if (list.Count == 0)
                {
                    System.Windows.MessageBox.Show("没有渠道被选中");
                    return;
                }

                RepackageInfo ri = new RepackageInfo();
                ri.apkPath = apkPath;
                ri.exportPath = exportPath;

                repackageManager.Repackage(ri, list, RepackCallBack, RepackCallBackError);
                isBuilding = true;
                UpdateRepackButton();

                currentTotalStep = list.Count * c_totalStep;
            }
            else
            {
                repackageManager.CancelRepack();
                isBuilding = false;
                UpdateRepackButton();

                progress = 0;
                Text_progressName.Content = "已取消";
            }
        }

        private void Button_ClickClean(object sender, RoutedEventArgs e)
        {
            line = 0;
            output = "";
            Text_output.Text = "";
        }

        private void Button_ClickEditorGame(object sender, RoutedEventArgs e)
        {
            GameSetting gs = new GameSetting();
            gs.ShowDialog();
        }

        private void Button_ClickDeleteChannel(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("确定要移除这个渠道吗？\n所有填写的配置都会丢失", "警告", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;

                for (int i = 0; i < EditorData.CurrentGameChannelList.Count; i++)
                {
                    ChannelInfo info = EditorData.CurrentGameChannelList[i];

                    if (info.Name == (string)btn.Tag)
                    {
                        EditorData.CurrentGameChannelList.RemoveAt(i);
                        EditorData.CurrentGameChannelList = EditorData.CurrentGameChannelList;
                        return;
                    }
                }
            }

        }

        private void Button_ClickAddChannel(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Text_AddChannel.Text != "")
                {
                    ChannelInfo ci3 = new ChannelInfo();
                    ci3.Name = Text_AddChannel.Text;

                    EditorData.CurrentGameChannelList.Add(ci3);
                    EditorData.CurrentGameChannelList = EditorData.CurrentGameChannelList;

                    Text_AddChannel.Text = "";
                }
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        private void CheckBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.CheckBox cb = sender as System.Windows.Controls.CheckBox;

            EditorData.SetCurrentChannel((string)cb.Tag);

            ChannelEditorWindow cew = new ChannelEditorWindow();
            cew.ShowDialog();
        }

        private void Button_ClickEditor(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button cb = sender as System.Windows.Controls.Button;

            EditorData.SetCurrentChannel((string)cb.Tag);

            ChannelEditorWindow cew = new ChannelEditorWindow();
            cew.ShowDialog();
        }

        private void Button_ClickSave(object sender, RoutedEventArgs e)
        {
            EditorData.SaveAPKPath(gameName, apkPath);
            EditorData.SaveExportAPKPath(gameName, exportPath);
            RecordManager.SaveRecord(EditorData.c_ConfigRecord, "index", ComboBox_gameList.SelectedIndex);

            System.Windows.Forms.MessageBox.Show("保存成功");
        }



        #endregion

        #region 界面改变事件

        private void ComboBox_gameList_Selected(object sender, RoutedEventArgs e)
        {
            RecordManager.SaveRecord(EditorData.c_ConfigRecord, "index", ComboBox_gameList.SelectedIndex);
            UpdateChannel();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            EditorData.CurrentGameChannelList = EditorData.CurrentGameChannelList;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            EditorData.CurrentGameChannelList = EditorData.CurrentGameChannelList;
        }

        void UpdateRepackButton()
        {
            if(isBuilding)
            {
                Button_Repack.Content = "取消";
            }
            else
            {
                Button_Repack.Content = "重打包";
            }
        }

        private void Text_BuildToolVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorData.BuildToolVersion = Text_BuildToolVersion.Text;
        }

        private void Text_APILevel_TextChanged(object sender, TextChangedEventArgs e)
        {
            EditorData.APILevel = int.Parse(Text_APILevel.Text);
        }

        #endregion

        #region 事件接收

        int line = 0;
        string output = "";

        float progress = 0;
        string content;

        void RepackCallBack(float progress, string content, string output)
        {
            this.progress = progress;
            this.content = content;

            line++;
            this.output += "["+line+"]"+ output+"\n";

            Action ac = new Action(UpdateContent);
            Dispatcher.BeginInvoke(ac);
        }

        void RepackCallBackError(float progress, string content, string output)
        {
            this.progress = progress;
            this.content = "打包异常";

            line++;
            this.output += "<Error>[" + line + "]" + output + "\n";
            isBuilding = false;
            progress = 0;

            Action ac = new Action(UpdateContent);
            Dispatcher.BeginInvoke(ac);

            repackageManager.CancelRepack();
        }

        void OutPut(string output)
        {
            this.content = "日志：";

            line++;
            this.output += "<Log>[" + line + "]" + output + "\n";
            isBuilding = false;
            progress = 0;

            Action ac = new Action(UpdateContent);
            Dispatcher.BeginInvoke(ac);

            repackageManager.CancelRepack();
        }

        #endregion

        #region Update

        void UpdateContent()
        {
            Progress_repackage.Maximum = currentTotalStep;
            Progress_repackage.Value = progress;

            Text_output.Text = output;
            Text_output.ScrollToEnd();

            Text_progressName.Content = content;

            exportPath = EditorData.GetExportAPKPath(gameName);
            apkPath = EditorData.GetAPKPath(gameName);

            Text_APKExportPath.Text = exportPath;
            Text_APKPath.Text = apkPath;
            Text_SDKLibPath.Text = EditorData.SdkLibPath;
            Text_AndroidAPKPath.Text = EditorData.AndroidSdkPath;
            Text_jetifierPath.Text = EditorData.JetifierPath;
            Text_BuildToolVersion.Text = EditorData.BuildToolVersion;
            Text_APILevel.Text = EditorData.APILevel.ToString();

            CheckBox_IsPutCMD.IsChecked = EditorData.IsOutPutCMD;
            CheckBox_IsAutoInstall.IsChecked = EditorData.IsAutoInstall;

            if (progress >= currentTotalStep)
            {
                isBuilding = false;
            }

            UpdateRepackButton();
        }

        void UpdateChannel()
        {
            if(ComboBox_gameList.SelectedIndex != -1
                && ComboBox_gameList.SelectedIndex < EditorData.GameList.Count
                )
            {
                gameName = EditorData.GameList[ComboBox_gameList.SelectedIndex].GameName;
                exportPath = EditorData.GetExportAPKPath(gameName);
                apkPath = EditorData.GetAPKPath(gameName);

                EditorData.SetChannelList(gameName);
                ListBox_channel.ItemsSource = EditorData.CurrentGameChannelList;

                //将apk路径保存在game设置中
                Text_APKExportPath.Text = exportPath;
                Text_APKPath.Text = apkPath;
            }
            else
            {
                ListBox_channel.ItemsSource = null;
            }
        }

        #endregion

        private void ChexkBox_IsPutCMD_Checked(object sender, RoutedEventArgs e)
        {
            EditorData.IsOutPutCMD = CheckBox_IsPutCMD.IsChecked ?? true;
        }

        private void ChexkBox_IsAutoInstall_Checked(object sender, RoutedEventArgs e)
        {
            EditorData.IsAutoInstall = CheckBox_IsAutoInstall.IsChecked ?? true;
        }

        private void Button_ClickTest(object sender, RoutedEventArgs e)
        {
            string aimPath = PathTool.GetCurrentPath() + "\\20200226_1";

            OutPutWindow opw = new OutPutWindow();
            opw.Show();

            ChannelTool ct = new ChannelTool(opw.ReceviceOutPut, opw.ReceviceErrorOutPut);

            ct.Rebuild_R_Table(aimPath);
        }

        private void Text_APILevel_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");
            e.Handled = re.IsMatch(e.Text);
        }

        //private void ComboBox_gameList_Selected(object sender, SelectionChangedEventArgs e)
        //{
        //    UpdateChannel();
        //}
    }
}
