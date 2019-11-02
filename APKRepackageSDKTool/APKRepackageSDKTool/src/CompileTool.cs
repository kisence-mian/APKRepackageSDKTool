using APKRepackageSDKTool;
using Mono.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

public class CompileTool
{
    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;


    public CompileTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;
    }

    public void OutPut(string content)
    {
        callBack?.Invoke(content);
    }

    public void Jar2Smali(string jarPath, string filePath)
    {
        string smaliPath = filePath + "\\smali";
        string JavaTempPath = PathTool.GetCurrentPath() + "\\JavaTempPath";
        string jarName = FileTool.GetFileNameByPath(jarPath);
        string tempPath = JavaTempPath + "\\" + jarName;

        FileTool.CreatPath(JavaTempPath);

        CmdService cmd = new CmdService(OutPut, errorCallBack);

        //Jar to dex
        cmd.Execute("java -jar dx.jar --dex --output=" + tempPath + " " + jarPath,true,true);

        //dex to smali
        cmd.Execute("java -jar baksmali-2.1.3.jar --o=" + smaliPath + " " + tempPath);

        //删除临时目录
        FileTool.DeleteDirectory(JavaTempPath);
        Directory.Delete(JavaTempPath);
    }

    public void Compile(SDKConfig sdkConfig,ChannelInfo channelInfo, string filePath)
    {
        string smaliPath = filePath + "\\smali";
        string JavaCompileTempPath = PathTool.GetCurrentPath() + "\\JavaCompileTempPath";
        string JavaCompileSrcPath = PathTool.GetCurrentPath() + "\\JavaCompileTempPath\\Src";
        string JavaCompileLibPath = PathTool.GetCurrentPath() + "\\JavaCompileTempPath\\Lib";
        string JavaFilePath = JavaCompileTempPath + "\\smali.java";
        string classFilePath = JavaCompileTempPath + "\\Class";
        string dexFilePath = JavaCompileTempPath + "\\smali.dex";

        CmdService cmd = new CmdService(OutPut, errorCallBack);

        //构造编译环境
        //创建Java类(替换关键字)
        for (int i = 0; i < sdkConfig.customJavaClass.Count; i++)
        {
            string javaName = JavaCompileSrcPath + "\\" + sdkConfig.customJavaClass[i].key + ".java";
            string s = ReplaceKeyWord(sdkConfig.customJavaClass[i].value, channelInfo);
            FileTool.WriteStringByFile(javaName, s);
        }

        //复制Java库
        string libs = "";
        for (int i = 0; i < sdkConfig.customJavaLibrary.Count; i++)
        {
            string p = JavaCompileLibPath + "\\" + FileTool.GetFileNameByPath(sdkConfig.customJavaLibrary[i]);
            libs += p + ";";
            FileTool.CreatFilePath(p);
            File.Copy(sdkConfig.customJavaLibrary[i], p, true);
        }

        //创建导出目录
        FileTool.CreatPath(classFilePath);

        //Java to class
        cmd.Execute("javac  -classpath " + libs + " " + JavaCompileSrcPath + "\\*.java -d " + classFilePath);

        //class to dex
        cmd.Execute("java -jar dx.jar --verbose --dex --output=" + dexFilePath + " " + classFilePath);

        //dex to smali
        cmd.Execute("java -jar baksmali-2.1.3.jar --o=" + smaliPath + " " + dexFilePath);

        //删除临时目录
        FileTool.DeleteDirectory(JavaCompileTempPath);
        Directory.Delete(JavaCompileTempPath);
    }

    public string ReplaceKeyWord(string oldContent, ChannelInfo channelInfo)
    {
        string result = oldContent;
        result = result.Replace("{PackageName}", channelInfo.PackageName);

        return result;
    }

    public string ReplaceKeyWordbySDKInfo(string oldContent, SDKInfo SDKinfo)
    {
        string result = oldContent;

        for (int i = 0; i < SDKinfo.sdkConfig.Count; i++)
        {
            result = result.Replace("{"+ SDKinfo.sdkConfig[i].key + "}", SDKinfo.sdkConfig[i].value);
        }

        return result;
    }

    public string RemoveSpecialCode(string oldContent)
    {
        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(oldContent)).Replace("?","");

        return HttpUtility.UrlEncode(oldContent, Encoding.UTF8);

        //Encoding utf8 = Encoding.UTF8;
        //String code = HttpUtility.UrlEncode(oldContent, utf8);
        //return HttpUtility.UrlDecode(code); 
    }
}
