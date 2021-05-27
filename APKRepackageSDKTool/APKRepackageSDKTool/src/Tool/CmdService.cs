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
        bool ignoreError = false;
        bool ignoreWarning = false;
        bool outPutCmd = true;

        Process process;

        OutPutCallBack callBack;
        OutPutCallBack errorCallBack;

        public CmdService(OutPutCallBack callBack,OutPutCallBack errorCallBack)
        {
            this.callBack = callBack;
            this.errorCallBack = errorCallBack;
        }

        public void Execute(string content, bool ignoreWarning = false, bool ignoreError = false,string path = null,bool outPutCmd = true)
        {
            this.ignoreError = ignoreError;
            this.ignoreWarning = ignoreWarning;
            this.outPutCmd = outPutCmd;

            try
            {
                if(EditorData.IsOutPutCMD && outPutCmd)
                {
                    Out(content);
                }

                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序

                p.StandardInput.AutoFlush = true;

                p.BeginErrorReadLine();
                p.BeginOutputReadLine();

                p.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);
                p.ErrorDataReceived += new DataReceivedEventHandler(ErrorReceived);

                if (path != null)
                {
                    string disk = path.Split(':')[0];
                    p.StandardInput.WriteLine(disk + ":");
                    p.StandardInput.WriteLine("cd " + path);
                }

                p.StandardInput.WriteLine(content);
                p.StandardInput.WriteLine("exit");

                p.WaitForExit();//等待程序执行完退出进程
                p.Close();
                p = null;
            }
            catch(Exception e)
            {
                if(!ignoreError)
                {
                    errorCallBack?.Invoke(e.ToString());
                }
                else
                {
                    callBack?.Invoke(e.ToString());
                }
            }
        }


        public void StartCmd(bool ignoreWarning = false, bool ignoreError = false, bool outPutCmd = true)
        {
            this.ignoreError = ignoreError;
            this.ignoreWarning = ignoreWarning;
            this.outPutCmd = outPutCmd;

            process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            process.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            process.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
            process.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            process.StartInfo.CreateNoWindow = true;//不显示程序窗口
            process.Start();//启动程序

            //process.StandardInput.AutoFlush = true;

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(ErrorReceived);
        }

        public void SendCmd(string content, string path = null)
        {
            if(process != null)
            {
                if (path != null)
                {
                    string disk = path.Split(':')[0];
                    process.StandardInput.WriteLine(disk + ":");
                    process.StandardInput.WriteLine("cd " + path);
                }

                process.StandardInput.WriteLine(content);
                process.StandardInput.WriteLine("exit");
            }
            else
            {
                ErrorOut("SendCmd Error process is null ");
            }
        }

        public void WaitToClose()
        {
            if (process != null)
            {
                process.StandardInput.AutoFlush = true;

                process.StandardInput.WriteLine("exit");

                process.WaitForExit();//等待程序执行完退出进程
                process.Close();
                process = null;
            }
            else
            {
                ErrorOut("WaitToClose Error process is  null ");
            }
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
            if(string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            if(IsWaring(e.Data))
            {
                Out(e.Data);
            }
            else
            {
                if (!ignoreError)
                {
                    if (Filter(e.Data))
                    {
                        errorCallBack?.Invoke(e.Data);
                    }
                }
                else
                {
                    Out(e.Data);
                }
            }
        }

        bool IsWaring(string data)
        {
            if(data.StartsWith("W"))
            {
                return true;
            }

            if (data.StartsWith("警告"))
            {
                return true;
            }

            if (data.StartsWith("warning"))
            {
                return true;
            }

            return false;
        }

        void Out(string content)
        {
            callBack?.Invoke(content);
        }

        void ErrorOut(string content)
        {
            errorCallBack?.Invoke(content);
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
