using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKRepackageSDKTool
{
    public class CmdService
    {
        Process process;

        public CmdService()
        {
            process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            process.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            process.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;//不显示程序窗口
            process.Start();//启动程序

            process.StandardInput.AutoFlush = true;
        }

        public void Execute(string content)
        {
            process.StandardInput.WriteLine(content);
        }

        public void EndExecute()
        {
            process.StandardInput.WriteLine("exit");
        }

        public void Close()
        {
            process.WaitForExit();//等待程序执行完退出进程
            process.Close();
        }

        public string GetOutPut()
        {
            return process.StandardOutput.ReadToEnd();
        }
    }
}
