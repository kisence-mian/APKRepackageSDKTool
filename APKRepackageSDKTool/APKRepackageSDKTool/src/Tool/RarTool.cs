using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

public class RarTool
{
    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;

    CmdService cs;

    string decompressionCmd = @"360zip.exe -x {RarPath} {AimPath}";
    string compressionCmd = @"360zip.exe -ar {FilePath} {ZipPath}";
    //string addFileCmd = @"360zip.exe -ar {FilePath} {ZipPath}";

    public RarTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;

        cs = new CmdService(callBack, errorCallBack);

        this.decompressionCmd = EditorData._RARdocompressCmd;
        this.compressionCmd = EditorData._RARcompressCmd;
    }

    /// <summary>
    /// 解压到同名文件夹
    /// </summary>
    /// <param name="rarFile"></param>
    public void Decompression(string rarFile)
    {
        string aimPath = FileTool.RemoveExpandName(rarFile);

        if(!Directory.Exists(aimPath))
        {
            string cmd = decompressionCmd.Replace("{AimPath}", aimPath).Replace("{RarPath}", rarFile);
            
            cs.Execute(cmd, outPutCmd: false);
        }
        else
        {
            OutPut("!解压时路径已存在 " + aimPath);
        }
    }

    /// <summary>
    /// 解压到目标文件夹
    /// </summary>
    /// <param name="rarFile"></param>
    /// <param name="aimPath"></param>
    public void Decompression(string rarFile,string aimPath,bool delectAimPath = false)
    {
        if(delectAimPath)
        {
            if (Directory.Exists(aimPath))
            {
                FileTool.DeleteDirectoryComplete(aimPath);
                //Directory.Delete(aimPath);
            }
        }

        string cmd = decompressionCmd.Replace("{AimPath}", aimPath).Replace("{RarPath}", rarFile);
        cs.Execute(cmd, outPutCmd: false);
    }

    ///// <summary>
    ///// 压缩
    ///// </summary>
    ///// <param name="filePath"></param>
    //public void Compression(string filePath)
    //{
    //    //string aimPath = FileTool.RemoveExpandName(rarFile);

    //    //if (!Directory.Exists(aimPath))
    //    //{
    //    //    string cmd = rarCmd.Replace("{AimPath}", aimPath).Replace("{RarPath}", rarFile);

    //    //    cs.Execute(cmd, outPutCmd: false);
    //    //}
    //}

    /// <summary>
    /// 压缩到目标文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="aimZipFile"></param>
    public void Compression(string filePath, string aimZipFile)
    {
        string cmd = compressionCmd.Replace("{ZipPath}", aimZipFile).Replace("{FilePath}", filePath);

        cs.Execute(cmd, outPutCmd: true);
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
