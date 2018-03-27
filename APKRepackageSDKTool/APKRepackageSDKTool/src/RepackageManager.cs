using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKRepackageSDKTool
{
    public class RepackageManager
    {
        string outPath = System.IO.Directory.GetCurrentDirectory();

        public void Repackage(RepackageInfo info, RepageProgress callBack)
        {
            //APK路径正确性校验

            //keyStore路径正确性校验

            string fileName = FileTool.GetFileNameByPath(info.apkPath);

            CmdService cmd = new CmdService();

            //反编译APK
            cmd.Execute("java -jar apktool.jar d " + info.apkPath + " " + outPath); 



            //执行对应的文件操作

            //重打包
            cmd.Execute("java -jar apktool.jar b " + outPath + "/" + fileName + " " + info.exportPath);


            //进行签名

            cmd.EndExecute();
            string output = cmd.GetOutPut();
            callBack?.Invoke(output);

            cmd.Close();
        }
    }

    /// <summary>
    /// 重打包进度回调
    /// </summary>
    /// <param name="content"></param>
    public delegate void RepageProgress(string content);

    public class RepackageInfo
    {
        public string apkPath;      //APK的路径
        public string keyStorePath; //KeyStore的路径

        public string exportPath;

        public string channelID; //渠道ID

        public string targetPackageName; //目标包名
    }
}
