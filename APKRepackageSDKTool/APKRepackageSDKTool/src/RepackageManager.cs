using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

                        if (string.IsNullOrEmpty(EditorData.AndroidSdkPath))
                        {
                            throw new Exception("AndroidSdkPath 不能为空！");
                        }

                        if (string.IsNullOrEmpty(EditorData.SdkLibPath))
                        {
                            throw new Exception("SdkLibPath 不能为空！");
                        }

                        if (string.IsNullOrEmpty(EditorData.BuildToolVersion))
                        {
                            throw new Exception("BuildToolVersion 不能为空！");
                        }

                        if (string.IsNullOrEmpty(channelInfo.keyStorePath))
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

                        DateTime now = DateTime.Now;

                        if (!string.IsNullOrEmpty(channelInfo.suffix))
                        {
                            if(EditorData.IsTimeStamp)
                            {
                                //移除旧的时间戳
                                fileName = Regex.Replace(fileName, @"\d+_\d+", "");

                                //加时间戳
                                finalPath = repackageInfo.exportPath + "\\" + FileTool.RemoveExpandName(fileName) + now.ToString("yyyyMMdd_HHmm")+ "_" + channelInfo.suffix + ".apk";
                            }
                            else
                            {
                                finalPath = repackageInfo.exportPath + "\\" + FileTool.RemoveExpandName(fileName) + "_" + channelInfo.suffix + ".apk";
                            }
                        }

                        CmdService cmd = new CmdService(OutPutCallBack, ErrorCallBack);
                        ChannelTool channelTool = new ChannelTool(OutPutCallBack, ErrorCallBack);

                        string apktool_version = "apktool_2.4.1";

                        //反编译APK
                        MakeProgress("反编译APK ",i, channelList.Count,channelInfo.Name);

                        string apkToolCmd = "java -jar " + apktool_version + ".jar d -f " + repackageInfo.apkPath + " -o " + aimPath;

                        if (channelInfo.isForceManifest)
                        {
                            apkToolCmd += " --force-manifest";
                        }

                        if (channelInfo.IsOnlyMainClasses)
                        {
                            apkToolCmd += " --only-main-classes";
                        }

                        cmd.Execute(apkToolCmd);

                        //执行对应的文件操作
                        MakeProgress("执行对应的文件操作", i, channelList.Count, channelInfo.Name);
                        channelTool.ChannelLogic(aimPath, channelInfo);

                        //移除过长的YML
                        MakeProgress("移除过长的YML", i, channelList.Count, channelInfo.Name);
                        channelTool.YMLLogic(aimPath);

                        //分包
                        MakeProgress("分包", i, channelList.Count, channelInfo.Name);
                        if (channelInfo.IsSplitDex)
                        {
                            channelTool.SplitDex(aimPath, channelInfo);
                        }

                        ////混淆DLL
                        //MakeProgress("混淆DLL", i, channelList.Count, channelInfo.Name);
                        //channelTool.ConfusionDLL(aimPath);

                        //重新生成R表
                        MakeProgress("重新生成R表", i, channelList.Count, channelInfo.Name);
                        if (!channelInfo.isForceManifest && channelInfo.isRebuildRTable)
                        {
                            channelTool.Rebuild_R_Table(aimPath, channelInfo);
                        }

                        //重打包
                        MakeProgress("重打包", i, channelList.Count, channelInfo.Name);
                        cmd.Execute("java -jar "+ apktool_version + ".jar b " + aimPath,true,true);

                        //进行签名
                        MakeProgress("进行签名", i, channelList.Count, channelInfo.Name);
                        cmd.Execute("jarsigner" // -verbose
                            //+ " -tsa https://timestamp.geotrust.com/tsa"
                            + " -digestalg SHA1 -sigalg MD5withRSA"
                            + " -storepass " + channelInfo.KeyStorePassWord
                            + " -keystore " + channelInfo.KeyStorePath
                            + " -sigFile CERT"  //强制将RSA文件更名为CERT
                            + " " + apkPath
                            + " " + channelInfo.KeyStoreAlias
                            );

                        //进行字节对齐并导出到最终目录
                        MakeProgress("进行字节对齐并导出到最终目录", i, channelList.Count, channelInfo.Name);
                        cmd.Execute(EditorData.GetZipalignPath() + " -f  4 " + apkPath + " " + finalPath);

                        if(EditorData.IsAutoInstall)
                        {
                            //自动安装
                            MakeProgress("自动安装", i, channelList.Count, channelInfo.Name);
                            cmd.Execute("adb install -r " + finalPath,true,true);
                        }

                        if(channelInfo.IsDeleteTempPath)
                        {
                            //删除临时目录
                            MakeProgress("删除临时目录", i, channelList.Count, channelInfo.Name);
                            FileTool.SafeDeleteDirectory(aimPath);
                        }
                        else
                        {
                            //删除临时目录
                            MakeProgress("跳过删除临时目录", i, channelList.Count, channelInfo.Name);
                        }

                        System.Diagnostics.Process.Start("Explorer", "/select," + finalPath);
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
