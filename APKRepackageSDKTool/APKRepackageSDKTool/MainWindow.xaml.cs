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
        string apkPath;
        string keyStorePath;
        string exportPath;

        public MainWindow()
        {
            InitializeComponent();
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
        }

        private void Button_ClickSelectKeyStore(object sender, RoutedEventArgs e)
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

        void RepackCallBack(string content)
        {
            Text_output.Text = content;
        }

        #endregion
    }
}
