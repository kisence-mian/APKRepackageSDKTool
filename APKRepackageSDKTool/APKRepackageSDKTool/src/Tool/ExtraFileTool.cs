using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

public class ExtraFileTool
{
    public const string c_FileName = "ExtraFile";

    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;
    RarTool rar;

    public ExtraFileTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;

        rar = new RarTool(callBack, errorCallBack);
    }

    public void PurExtraFile(string aimPath,string apkPath,ChannelInfo channelInfo)
    {
        OutPut("处理额外文件 " + apkPath);

        string rarTempPath = aimPath + "\\dist\\RarPath";
        string zipTempPath = apkPath.Replace(".apk",".zip");

        File.Move(apkPath, zipTempPath);

        //放入文件
        for (int i = 0; i < channelInfo.sdkList.Count; i++)
        {
            SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(channelInfo.sdkList[i].sdkName);
            if (config.UseExtraFile)
            {
                OutPut("放入额外文件 " + config.SdkName);
                string sourcesPath = EditorData.SdkLibPath + "/" + config.sdkName + "/" + c_FileName;

                PutFileToAPK(sourcesPath, zipTempPath);
            }
        }

        File.Move(zipTempPath, apkPath);

        ////解压RAR
        //rar.Decompression(apkPath, rarTempPath);

        ////放入文件
        //for (int i = 0; i < channelInfo.sdkList.Count; i++)
        //{
        //    SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(channelInfo.sdkList[i].sdkName);
        //    if (config.UseExtraFile)
        //    {
        //        OutPut("放入额外文件 " + config.SdkName);

        //        string sourcesPath = EditorData.SdkLibPath + "/" + config.sdkName + "/" + c_FileName;
        //        FileTool.CopyDirectory(sourcesPath, rarTempPath);
        //    }
        //}

        ////重新压缩
        //rar.Compression(rarTempPath, zipTempPath);
        //FileTool.DeleteFile(apkPath);
        //File.Move(zipTempPath, apkPath);
    }

    void PutFileToAPK(string sdkPath,string apkPath)
    {
        DirectoryInfo info = new DirectoryInfo(sdkPath);

        foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
        {
            rar.Compression(fsi.FullName, apkPath);
        }
    }

    public void OutPut(string content)
    {
        callBack?.Invoke(content);
    }

    public void ErrorOutPut(string content)
    {
        errorCallBack?.Invoke(content);
    }
}