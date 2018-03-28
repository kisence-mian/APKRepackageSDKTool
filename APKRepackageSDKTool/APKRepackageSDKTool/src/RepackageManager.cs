using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APKRepackageSDKTool
{
    public class RepackageManager
    {
        public void Repackage(RepackageInfo info, RepageProgress callBack)
        {
            //APK路径正确性校验

            //keyStore路径正确性校验

            RepackageThread rt = new RepackageThread();

            rt.info = info;
            rt.callBack = callBack;

            Thread th = new Thread(rt.Repackage);
            th.Start();
        }

        class RepackageThread
        {
            string outPath = PathTool.GetCurrentPath();

            public RepackageInfo info;
            public RepageProgress callBack;

            int step = 0;
            float progress = 0;
            //const float totalStep = 5f;

            int line = 0;
            StringBuilder output = new StringBuilder();
            string content = "";

            public void Repackage()
            {
                string fileName = FileTool.GetFileNameByPath(info.apkPath);
                string aimPath = outPath + "\\" + FileTool.RemoveExpandName(fileName);
                string apkPath = aimPath + "\\dist\\" + fileName;

                CmdService cmd = new CmdService(OutPutCallBack);

                //反编译APK
                MakeProgress("反编译APK");
                cmd.Execute("java -jar apktool.jar d -f " + info.apkPath + " -o " + aimPath);
                
                //执行对应的文件操作
                MakeProgress("执行对应的文件操作");

                //重打包
                MakeProgress("重打包");
                cmd.Execute("java -jar apktool.jar b " + aimPath);

                //进行签名
                MakeProgress("进行签名");
                cmd.Execute("jarsigner -verbose -keystore " + info.keyStorePath + " " + apkPath + " -storepass hello9123 huoyu" /*+ info.keyStorePath*/);

                //拷贝到导出目录
                MakeProgress("拷贝到导出目录");
                cmd.Execute("copy " + apkPath + " " + info.exportPath + "\\" + fileName +" /Y");

                MakeProgress("删除临时目录");
                //删除临时目录
                FileTool.DeleteDirectory(aimPath);
                Directory.Delete(aimPath);

                MakeProgress("完成");
            }

            public void OutPutCallBack(string output)
            {
                line++;
                this.output.Append("["+line+"]"+ output + "\n");
                callBack?.Invoke(progress,content, this.output.ToString());
            }

            void MakeProgress(string content)
            {
                line++;
                this.output.Append("[" + line + "]" + content + "\n");

                this.content = content;
                progress = step;
                step++;

                callBack?.Invoke(progress, content, output.ToString());
            }
        }
    }

    /// <summary>
    /// 重打包进度回调
    /// </summary>
    /// <param name="content"></param>
    public delegate void RepageProgress(float progress,string content, string outPut);

    public class RepackageInfo
    {
        public string apkPath;      //APK的路径
        public string keyStorePath; //KeyStore的路径
        public string keyStorePassWord; //KeyStore 密码
        public string exportPath; //导出路径

        public string channelID; //渠道ID

        public string targetPackageName; //目标包名
    }
}
