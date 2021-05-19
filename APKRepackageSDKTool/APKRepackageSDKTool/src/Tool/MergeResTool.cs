using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MergeResTool
{
    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;

    public MergeResTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;
    }

    public void Merge(string sdkPath,string aimPath)
    {
        OutPut("I: MergeResTool 合并res文件夹 \n" + sdkPath + " \n" + aimPath);

        DirectoryInfo dir = new DirectoryInfo(sdkPath);
        DirectoryInfo[] res_directoryInfoArray = dir.GetDirectories();
        foreach (DirectoryInfo res_dir in res_directoryInfoArray)
        {
            string dirName = FileTool.GetDirectoryName(res_dir.FullName);

            //创建文件夹
            Directory.CreateDirectory(aimPath + "\\" + dirName);

            //OutPut("MergeResTool dirName " + dirName);

            //特殊处理 value 文件夹，优化合并效率
            if (dirName.Contains("values"))
            {
                MergeXMLTool mergeXML = new MergeXMLTool(callBack, errorCallBack);
                mergeXML.Merge(res_dir.FullName, aimPath + "\\" + dirName );
            }
            else
            {
                //MergeXMLTool mergeXML = new MergeXMLTool(callBack, errorCallBack);
                //mergeXML.Merge(res_dir.FullName, aimPath + "\\" + dirName);

                //跳过xml合并
                FileTool.CopyDirectory(res_dir.FullName, aimPath + "\\" + dirName, RepeatHandle);
            }
        }
    }

    void RepeatHandle(string fileA, string fileB)
    {
        OutPut("重复的资源 " + fileB);

        ////只支持合并xml
        //if (fileA.Contains("xml") && fileB.Contains("xml"))
        //{
        //    //跳过
        //    OutPut("跳过xml 合并" + fileA);
        //}
        //else if (fileA.Contains("png") && fileB.Contains("png"))
        //{
        //    //跳过PNG
        //    OutPut("跳过PNG 合并" + fileA);
        //}
        //else
        //{
        //    ErrorOutPut("不支持的合并类型" + fileA + " ->" + fileB);
        //}
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
