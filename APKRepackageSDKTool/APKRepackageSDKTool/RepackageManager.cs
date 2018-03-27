using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKRepackageSDKTool
{
    public class RepackageManager
    {
        public void Repackage(RepackageInfo info, RepageProgress callBack)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            p.StandardInput.WriteLine("java -jar apktool_2.3.1.jar");

            p.StandardInput.WriteLine("exit");

            p.StandardInput.AutoFlush = true;

            string output = p.StandardOutput.ReadToEnd();


            callBack?.Invoke(output);

            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
        }
    }

    /// <summary>
    /// 重打包进度回调
    /// </summary>
    /// <param name="content"></param>
    public delegate void RepageProgress(string content);

    public class RepackageInfo
    {
        public string apkPath;      //APK的路径
        public string keyStorePath; //KeyStore的路径

        public string channelID; //渠道ID

        public string targetPackageName; //目标包名
    }
}
