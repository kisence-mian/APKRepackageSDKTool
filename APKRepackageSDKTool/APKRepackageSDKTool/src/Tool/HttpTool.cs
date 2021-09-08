using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class HttpTool
{
    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;

    public HttpTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;
    }


    /// <summary>
    /// GET方式发送得结果
    /// </summary>
    /// <param name="url">请求的url</param>
    public string DoGetRequestSendData(string url)
    {
        HttpWebRequest hwRequest = null;
        HttpWebResponse hwResponse = null;

        string strResult = string.Empty;
        try
        {
            hwRequest = (System.Net.HttpWebRequest)WebRequest.Create(url);
            //hwRequest.Timeout = 30000;
            hwRequest.Method = "GET";
            hwRequest.ContentType = "application/x-www-form-urlencoded";
        }
        catch (System.Exception err)
        {
            ErrorOutPut(err.ToString());
        }

        try
        {
            hwResponse = (HttpWebResponse)hwRequest.GetResponse();
            StreamReader srReader = new StreamReader(hwResponse.GetResponseStream(), Encoding.ASCII);
            strResult = srReader.ReadToEnd();
            srReader.Close();
            hwResponse.Close();
        }
        catch (System.Exception err)
        {
            ErrorOutPut(err.ToString());
        }

        OutPut(strResult.ToString());

        return strResult;
    }

    /// http下载文件
    /// </summary>
    /// <param name="url">下载文件地址</param>
    /// <param name="path">文件存放地址，包含文件名</param>
    /// <returns></returns>
    public bool HttpDownload(string url, string path)
    {
        url = url.Replace("\\", "/");

        OutPut("开始下载 " + url);

        if(File.Exists(path))
        {
            OutPut("目标文件已存在 " + path);
            return true;
        }

        Random rd = new Random();
        int random = rd.Next() % 1000000;
        string tempPath = System.IO.Path.GetDirectoryName(path) + @"\temp" + random;
        System.IO.Directory.CreateDirectory(tempPath);  //创建临时文件目录
        string tempFile = tempPath + @"\" + System.IO.Path.GetFileName(path) + ".temp"; //临时文件
        
        if (System.IO.File.Exists(tempFile))
        {
            System.IO.File.Delete(tempFile);    //存在则删除
        }

        FileStream fs = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

        try
        {
            // 设置参数
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //发送请求并获取相应回应数据
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            //直到request.GetResponse()程序才开始向目标网页发送Post请求
            Stream responseStream = response.GetResponseStream();
            //创建本地文件写入流
            //Stream stream = new FileStream(tempFile, FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, (int)bArr.Length);
            while (size > 0)
            {
                //stream.Write(bArr, 0, size);
                fs.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, (int)bArr.Length);
            }
            //stream.Close();
            fs.Close();
            responseStream.Close();
            System.IO.File.Move(tempFile, path);
            FileTool.DeleteDirectoryComplete(tempPath);

            OutPut("下载完成 " + url);

            return true;
        }
        catch (Exception e)
        {
            fs.Close();
            ErrorOutPut("下载失败 " + url + " \n" + e);

            FileTool.DeleteDirectoryComplete(tempPath);
            return false;
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