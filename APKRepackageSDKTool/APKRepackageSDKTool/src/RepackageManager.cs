using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace APKRepackageSDKTool
{
    public class RepackageManager
    {
        const string c_channelRecordName = "Channel";

        RepackageThread repackageThread;
        Thread thread;

        public void Repackage(RepackageInfo info,List< ChannelInfo> channelList, RepageProgress callBack, RepageProgress errorCallBack)
        {
            //APK路径正确性校验
            if(string.IsNullOrEmpty(info.apkPath))
            {
                MessageBox.Show("APK不能为空！");
            }

            repackageThread = new RepackageThread();
            repackageThread.repackageInfo = info;
            repackageThread.callBack = callBack;
            repackageThread.errorCallBack = errorCallBack;
            repackageThread.channelList = channelList;

            thread = new Thread(repackageThread.Repackage);
            thread.Start();
        }

        public void CancelRepack()
        {
            if(thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        class RepackageThread
        {
            string outPath = PathTool.GetCurrentPath();

            public RepackageInfo repackageInfo;
            public List<ChannelInfo> channelList;
            public RepageProgress callBack;
            public RepageProgress errorCallBack;

            int step = 0;
            float progress = 0;
            //const float totalStep = 5f;

            string content = "";

            public void Repackage()
            {
                try
                {
                    for (int i = 0; i < channelList.Count; i++)
                    {
                        ChannelInfo channelInfo = channelList[i];

                        if(string.IsNullOrEmpty(channelInfo.keyStorePath))
                        {
                            throw new Exception("keyStore 不能为空！");
                        }

                        if (string.IsNullOrEmpty(channelInfo.KeyStorePassWord))
                        {
                            throw new Exception("KeyStore PassWord 不能为空！");
                        }

                        if (string.IsNullOrEmpty(channelInfo.KeyStoreAlias))
                        {
                            throw new Exception("Alias 不能为空！");
                        }

                        if (string.IsNullOrEmpty(channelInfo.KeyStoreAliasPassWord))
                        {
                            throw new Exception("Alias PassWord 不能为空！");
                        }

                        string fileName = FileTool.GetFileNameByPath(repackageInfo.apkPath);
                        string aimPath = outPath + "\\" + FileTool.RemoveExpandName(fileName);
                        string apkPath = aimPath + "\\dist\\" + fileName;
                        string finalPath = repackageInfo.exportPath + "\\" + FileTool.RemoveExpandName(fileName) + ".apk";

                        if (!string.IsNullOrEmpty(channelInfo.suffix))
                        {
                            finalPath = repackageInfo.exportPath + "\\" + FileTool.RemoveExpandName(fileName) + "_" + channelInfo.suffix + ".apk";
                        }

                        CmdService cmd = new CmdService(OutPutCallBack, ErrorCallBack);
                        ChannelTool channelTool = new ChannelTool(OutPutCallBack, ErrorCallBack);

                        //反编译APK
                        MakeProgress("反编译APK ",i, channelList.Count,channelInfo.Name);
                        cmd.Execute("java -jar apktool.jar d -f " + repackageInfo.apkPath + " -o " + aimPath);

                        //执行对应的文件操作
                        MakeProgress("执行对应的文件操作", i, channelList.Count, channelInfo.Name);
                        channelTool.ChannelLogic(aimPath, channelInfo);

                        //重打包
                        MakeProgress("重打包", i, channelList.Count, channelInfo.Name);
                        cmd.Execute("java -jar apktool.jar b " + aimPath);

                        //进行签名
                        MakeProgress("进行签名", i, channelList.Count, channelInfo.Name);
                        cmd.Execute("jarsigner" // -verbose
                            //+ " -tsa https://timestamp.geotrust.com/tsa"
                            + " -digestalg SHA1 -sigalg MD5withRSA"
                            + " -storepass " + channelInfo.KeyStorePassWord
                            + " -keystore " + channelInfo.KeyStorePath
                            + " " + apkPath
                            + " " + channelInfo.KeyStoreAlias
                            );

                        //进行字节对齐并导出到最终目录
                        MakeProgress("进行字节对齐并导出到最终目录", i, channelList.Count, channelInfo.Name);
                        cmd.Execute("zipalign -f  4 " + apkPath + " " + finalPath);

                        //删除临时目录
                        MakeProgress("删除临时目录", i, channelList.Count, channelInfo.Name);

                        //FileTool.DeleteDirectory(aimPath);
                        //Directory.Delete(aimPath);

                        MakeProgress("完成", i, channelList.Count, channelInfo.Name);
                    }
                }
                catch (Exception e)
                {
                    ErrorCallBack(e.ToString());
                }
            }

            public void OutPutCallBack(string output)
            {
                callBack?.Invoke(progress,content, output);
            }

            public void ErrorCallBack(string output)
            {
                errorCallBack?.Invoke(progress, content, output);
            }

            void MakeProgress(string content ,int currentChannel ,int totalChannel,string channelName)
            {
                this.content = content + " " + channelName + " ( " + (currentChannel + 1)+ " / "+ totalChannel + " )";
                progress = step;
                step++;

                callBack?.Invoke(progress, this.content, this.content);
            }
        }
    }

    #region 声明
    /// <summary>
    /// 重打包进度回调
    /// </summary>
    /// <param name="content"></param>
    public delegate void RepageProgress(float progress,string content, string outPut);
    public delegate void OutPutCallBack(string output);

    public class RepackageInfo
    {
        public string apkPath;      //APK的路径
        public string exportPath; //导出路径

        public string channelID; //渠道ID

        public string targetPackageName; //目标包名
    }

    #endregion
}
