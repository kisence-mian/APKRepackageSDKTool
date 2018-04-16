using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace APKRepackageSDKTool
{
    public class CmdService
    {
        Process process;

        OutPutCallBack callBack;
        OutPutCallBack errorCallBack;

        public CmdService(OutPutCallBack callBack,OutPutCallBack errorCallBack)
        {
            this.callBack = callBack;
            this.errorCallBack = errorCallBack;
        }

        public void Execute(string content)
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

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(ErrorReceived);

            process.StandardInput.WriteLine(content);
            process.StandardInput.WriteLine("exit");

            process.WaitForExit();//等待程序执行完退出进程
            process.Close();
            process = null;
        }

        public string GetOutPut()
        {
            return process.StandardOutput.ReadToEnd();
        }
        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (Filter(e.Data))
            {
                callBack?.Invoke(e.Data);
            }
        }

        private void ErrorReceived(object sender, DataReceivedEventArgs e)
        {
            if (Filter(e.Data))
            {
                errorCallBack?.Invoke(e.Data);
            }
        }

        bool Filter(string content)
        {
            if (String.IsNullOrEmpty(content))
            {
                return false;
            }
            else
            {
                if (content.Contains("Microsoft")
                    || content.Contains(">")
                    )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }


}
