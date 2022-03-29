using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AABTool
{
    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;

    CmdService cmd;

    public AABTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;

        cmd = new CmdService(callBack, errorCallBack);
    }

    /// <summary>
    /// 将apk 转换成 aab包
    /// </summary>
    /// <param name="aimPath"></param>
    public void ExportAABPackage(string aimPath , ChannelInfo channelInfo)
    {
        //参考链接 https://www.jianshu.com/p/117ebe7c6fd5


        //将apk拆开
        //string apkToolCmd = "java -jar " + EditorData.ApktoolVersion + ".jar d -f " + repackageInfo.apkPath + " -o " + aimPath;

        //编译资源使用aapt2编译生成 *.flat文件集合
        //生成compiled_resources.zip文件

        string resourcesPath = aimPath + "\res";
        string compiled_resourcesPath = "compiled_resources.zi";
        string aabPath = "";

        cmd.Execute("java -jar " + EditorData.GetAAPT2Path() + " compile --dir " + resourcesPath + " -o " + compiled_resourcesPath);

        //关联资源
        //生成base.apk
        cmd.Execute("java -jar " + EditorData.GetAAPT2Path() + " " + aimPath + " -o compiled_resources.zip");


        //解压base.apk
        //解压到base文件夹，目录结构
        cmd.Execute("java -jar " + EditorData.GetAAPT2Path() + " " + aimPath + " -o compiled_resources.zip");

        //拷贝资源
        //创建base文件夹，以base文件夹为根目录
        //将解压得到的resources.pb拷贝到 ./temp/base/resources.pb
        //将解压得到的res拷贝到 ./temp/base/res
        //创建manifest文件夹， 将解压得到的AndroidManifest.xml拷贝到 ./temp/base/manifest
        //拷贝assets , 将 ./temp/decode_apk_dir/assets 拷贝到 ./temp/base/assets
        //拷贝lib， 将 ./temp/decode_apk_dir/lib 拷贝到 ./temp/base/lib
        //创建root文件夹，拷贝unknown， 将 ./temp/decode_apk_dir/unknown 拷贝到 ./temp/base/root/unknown
        //拷贝kotlin， 将 ./temp/decode_apk_dir/kotlin拷贝到 ./temp/base/root/kotlin
        //root目录下创建META-INF文件夹，得到 ./temp/base/root/META-INF
        //创建dex 文件夹，将 ./temp/decode_apk_dir/*.dex拷贝到 ./temp/base/dex（多个dex都要一起拷贝过来）


        //压缩资源
        cmd.Execute("java -jar " + EditorData.GetAAPT2Path() + " " + aimPath + " -o compiled_resources.zip");


        //编译abb
        cmd.Execute("java -jar " + EditorData.GetBundletoolPath() + " build-bundle --modules=" + aimPath + "--output=" + aabPath);


        //abb签名
        cmd.Execute("jarsigner" // -verbose
                + " -digestalg SHA1 -sigalg MD5withRSA"
                + " -storepass " + channelInfo.KeyStorePassWord
                + " -keystore " + channelInfo.KeyStorePath
                + " -sigFile CERT"  //强制将RSA文件更名为CERT
                + " " + aabPath
                + " " + channelInfo.KeyStoreAlias
        );

    }
}
