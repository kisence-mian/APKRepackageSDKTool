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
                        string aimPath = outPath + "\\.APKCache\\" + FileTool.RemoveExpandName(fileName);
                        string apkPath = aimPath + "\\dist\\" + fileName;
                        string finalPath = repackageInfo.exportPath + "\\" + FileTool.RemoveExpandName(fileName) + ".apk";

                        DateTime now = DateTime.Now;

                        if (!string.IsNullOrEmpty(channelInfo.suffix))
                        {
                            string version = "";

                            if(channelInfo.BundleVersionCode >0)
                            {
                                version += "_" + channelInfo.versionName +"_" + channelInfo.BundleVersionCode;
                            }

                            if(EditorData.IsTimeStamp)
                            {
                                //移除旧的时间戳和渠道
                                fileName = Regex.Replace(fileName, @"_[a-zA-Z]+_\d+_\d+", "");

                                //移除旧的时间戳
                                fileName = Regex.Replace(fileName, @"_\d+_\d+", "");

                                //加时间戳
                                finalPath = repackageInfo.exportPath + "\\" + FileTool.RemoveExpandName(fileName) + version + "_" + channelInfo.suffix+"_" + now.ToString("yyyyMMdd_HHmm") + ".apk";
                            }
                            else
                            {
                                finalPath = repackageInfo.exportPath + "\\" + FileTool.RemoveExpandName(fileName) + version + "_" + channelInfo.suffix + ".apk";
                            }
                        }

                        CmdService cmd = new CmdService(OutPutCallBack, ErrorCallBack);
                        ChannelTool channelTool = new ChannelTool(OutPutCallBack, ErrorCallBack);

                        //string apktool_version = "apktool_2.3.1";
                        //EditorData.AndroidSdkPath

                        //反编译APK
                        MakeProgress("反编译APK ",i, channelList.Count,channelInfo.Name);

                        string apkToolCmd = "java -jar " + EditorData.ApktoolVersion + ".jar d -f \"" + repackageInfo.apkPath + "\" -o \"" + aimPath + "\"";

                        if (channelInfo.isForceManifest)
                        {
                            apkToolCmd += " --force-manifest";
                        }

                        if (channelInfo.IsOnlyMainClasses)
                        {
                            apkToolCmd += " --only-main-classes";
                        }
                        
                        cmd.Execute(apkToolCmd);

                        //移除过长的YML
                        string extensionsFilePath = aimPath + "\\doNotCompress.txt";
                        if (channelInfo.IsSimplifyYml)
                        {
                            MakeProgress("移除过长的YML", i, channelList.Count, channelInfo.Name);

                            channelTool.YMLLogic(aimPath);
                        }
                        else
                        {
                            MakeProgress("跳过移除过长的YML", i, channelList.Count, channelInfo.Name);
                        }

                        //执行对应的文件操作
                        MakeProgress("执行对应的文件操作", i, channelList.Count, channelInfo.Name);
                        channelTool.ChannelLogic(aimPath, channelInfo);

                        ////混淆DLL
                        //MakeProgress("混淆DLL", i, channelList.Count, channelInfo.Name);
                        //channelTool.ConfusionDLL(aimPath);

                        //重新生成R表
                        MakeProgress("重新生成R表", i, channelList.Count, channelInfo.Name);
                        if (!channelInfo.isForceManifest && channelInfo.isRebuildRTable)
                        {
                            channelTool.Rebuild_R_Table(aimPath, channelInfo);
                        }

                        //分包
                        MakeProgress("分包", i, channelList.Count, channelInfo.Name);
                        if (channelInfo.IsSplitDex)
                        {
                            SplitDexTool sdt = new SplitDexTool(OutPutCallBack, ErrorCallBack);
                            sdt.SplitDex(aimPath, channelInfo);
                            //channelTool.SplitDex(aimPath, channelInfo);
                        }

                        //重打包
                        MakeProgress("重打包", i, channelList.Count, channelInfo.Name);
                        string options = "";

                        if (channelInfo.isUseAAPT2)
                        {
                            if(channelInfo.IsCustomAAPT)
                            {
                                options += "-a " + EditorData.GetAAPT2Path();
                            }

                            options += " --use-aapt2 ";
                        }

                        if (channelInfo.GetAssignMinAPILevel())
                        {
                            options += "--api " + channelInfo.GetMinAPILevel() + " ";
                        }

                        if (channelInfo.IsNoCompressResource)
                        {
                            options += "--no-crunch ";
                        }

                        cmd.Execute("java -jar "+ EditorData.ApktoolVersion + ".jar " + options + "b \"" + aimPath + "\"",true,true);

                        //判断apk是否存在
                        if(!File.Exists(apkPath))
                        {
                            throw new Exception("重打包失败！");
                        }

                        //放入额外文件
                        //在通常apk结构之外的情况,需要先把包打出来,然后再放入文件
                        if(channelInfo.GetUseExtraFile())
                        {
                            ExtraFileTool eft = new ExtraFileTool(OutPutCallBack, ErrorCallBack);
                            eft.PurExtraFile(aimPath,apkPath, channelInfo);
                        }

                        MakeProgress("进行签名", i, channelList.Count, channelInfo.Name);
                        if (!channelInfo.UseV2Sign)
                        {
                            //进行签名
                            cmd.Execute("jarsigner" // -verbose
                                                    //+ " -tsa https://timestamp.geotrust.com/tsa"
                            + " -digestalg SHA1 -sigalg MD5withRSA"
                            + " -storepass " + channelInfo.KeyStorePassWord
                            + " -keystore " + channelInfo.KeyStorePath
                            + " -sigFile CERT"  //强制将RSA文件更名为CERT
                            + " \"" + apkPath + "\""
                            + " " + channelInfo.KeyStoreAlias
                            );
                        }

                        if(channelInfo.IsExportAAB)//aab打包
                        {
                            AABTool aab = new AABTool(OutPutCallBack, ErrorCallBack);
                            aab.ExportAABPackage(apkPath, channelInfo,repackageInfo,finalPath);
                        }

                        if (channelInfo.IsZipalign)
                        {
                            //进行字节对齐并导出到最终目录
                            MakeProgress("进行字节对齐并导出到最终目录", i, channelList.Count, channelInfo.Name);
                            cmd.Execute(EditorData.GetZipalignPath() + " -f  4 \"" + apkPath + "\" \"" + finalPath + "\"");
                        }
                        else
                        {
                            MakeProgress("跳过字节对齐", i, channelList.Count, channelInfo.Name);
                            File.Move(apkPath, finalPath);
                        }

                        //字节对齐后再进行v2签名
                        //进行V2签名
                        if (channelInfo.UseV2Sign)
                        {
                            string v2SignCmd = "java -jar " + EditorData.GetApksignerPath() + " sign "
                                + "--ks " + channelInfo.JksPath + " "
                                + "--ks-key-alias " + channelInfo.KeyStoreAlias + " "
                                + "--ks-pass pass:" + channelInfo.KeyStorePassWord + " "
                                + "--key-pass pass:" + channelInfo.keyStoreAliasPassWord + " "
                                + "--out \"" + finalPath + "\""
                                + " \"" + finalPath + "\"";

                            cmd.Execute(v2SignCmd);
                            //删掉多余的idsig文件
                            File.Delete(finalPath + ".idsig");
                        }
                        //播放完成音频
                        AudioTool.PlayAudio(@"\res\SFX_Finish.mp3", UriKind.RelativeOrAbsolute);


                        if (EditorData.IsAutoInstall)
                        {
                            //自动安装
                            MakeProgress("自动安装", i, channelList.Count, channelInfo.Name);
                            cmd.Execute("adb install -r \"" + finalPath + "\"", true,true);
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
