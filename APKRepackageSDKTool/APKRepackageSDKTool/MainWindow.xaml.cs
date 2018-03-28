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
        const string c_PathRecord = "Paths";

        string apkPath;
        string keyStorePath;
        string exportPath;
        string SDKLibPath;

        float progress = 0;
        string content;
        string output;

        public MainWindow()
        {
            InitializeComponent();

            apkPath = RecordManager.GetRecord(c_PathRecord, "apkPath", "null");
            keyStorePath = RecordManager.GetRecord(c_PathRecord, "keyStorePath", "null");
            exportPath = RecordManager.GetRecord(c_PathRecord, "exportPath", "null");
            SDKLibPath = RecordManager.GetRecord(c_PathRecord, "SDKLibPath", "null");

            UpdateContent();
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
                RecordManager.SaveRecord(c_PathRecord, "apkPath", apkPath);
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
            RecordManager.SaveRecord(c_PathRecord, "exportPath", exportPath);
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
            RecordManager.SaveRecord(c_PathRecord, "SDKLibPath", SDKLibPath);
        }

        private void Button_ClickSelectKeyStore(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "keystore Files (*.keystore)|*.keystore"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                this.Text_KeyStorePath.Text = openFileDialog.FileName;
                keyStorePath = openFileDialog.FileName;
                RecordManager.SaveRecord(c_PathRecord, "keyStorePath", keyStorePath);
            }
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
            ri.keyStorePath = keyStorePath;
            ri.exportPath = exportPath;

            RepackageManager rm = new RepackageManager();
            rm.Repackage(ri, RepackCallBack);
        }

        #endregion

        #region 事件接收
        void RepackCallBack(float progress, string content, string output)
        {
            this.progress = progress;
            this.content = content;
            this.output = output;

            Action ac = new Action(UpdateContent);
            Dispatcher.BeginInvoke(ac);
        }
        #endregion

        #region Update

        void UpdateContent()
        {
            Progress_repackage.Maximum = 5;
            Progress_repackage.Value = progress;

            Text_output.Text = output;
            Text_progressName.Content = content;

            Text_APKExportPath.Text = exportPath;
            Text_APKPath.Text = apkPath;
            Text_KeyStorePath.Text = keyStorePath;
            Text_SDKLibPath.Text = SDKLibPath;
        }
        
        #endregion
    }
}
