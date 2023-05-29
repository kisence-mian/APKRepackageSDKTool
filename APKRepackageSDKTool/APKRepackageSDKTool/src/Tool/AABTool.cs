using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pri.LongPath;

public class AABTool
{
    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;

    CmdService cmd;
    RarTool rar;

    public AABTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;

        cmd = new CmdService(callBack, errorCallBack);
        rar = new RarTool(callBack, errorCallBack);
    }

    /// <summary>
    /// 将apk 转换成 aab包
    /// </summary>
    /// <param name="aimPath"></param>
    public void ExportAABPackage(string apkPath , ChannelInfo channelInfo,RepackageInfo repackageInfo,string finalPath)
    {
        //参考链接 https://www.jianshu.com/p/117ebe7c6fd5

        //创建相应的文件夹路径
        string apkName = finalPath.Split('\\')[finalPath.Split('\\').Length - 1].Replace(".apk","");
        if (!Directory.Exists(repackageInfo.exportPath + "\\AAB"))
        {
            Directory.CreateDirectory(repackageInfo.exportPath + "\\AAB");
        }
        string aabDirPth=repackageInfo.exportPath+"\\AAB\\"+apkName;
        if(Directory.Exists(aabDirPth))
        {
            Directory.Delete(aabDirPth,true);
        }
        Directory.CreateDirectory(aabDirPth);
        string tempDirPath=aabDirPth+"\\TempDir";
        Directory.CreateDirectory(tempDirPath);
        
        //首先是用apktool解压apk
        string apkDirPath = tempDirPath + "\\decode_apk_dir";
        string apkDirCmd = "java -jar " + EditorData.ApktoolVersion + ".jar d -f \"" + apkPath + "\" -s -o \"" + apkDirPath + "\"";
        cmd.Execute(apkDirCmd);

        //然后是生成resources资源压缩文件
        string apkResPath = tempDirPath + "\\compiled_resources.zip";
        string apkResCmd = EditorData.GetAAPT2Path()+ " compile --no-crunch --dir \"" + apkDirPath + "\\res\" -o \"" + apkResPath+ "\"";
        cmd.Execute(apkResCmd);

        //再然后是生成base.apk
        string androidPath = EditorData.AndroidSdkPath + "\\platforms\\android-" + EditorData.APILevel + "\\android.jar ";
        string baseApkPath = tempDirPath + "\\base.apk";

        int minSdkVersion = channelInfo.GetMinAPILevel();
        int targetSdkVersion = channelInfo.GetTargetAPILevel();
        int bundleVersionCode = channelInfo.GetBundleVersionCode();
        string versionName = channelInfo.GetVersionName();
        
        string apkBaseCmd = EditorData.GetAAPT2Path()+" link --proto-format -o " +baseApkPath+" -I " + androidPath+
                            "--min-sdk-version "+minSdkVersion +" "+
                            "--target-sdk-version "+targetSdkVersion +" "+
                            "--version-code "+ bundleVersionCode + " " +
                            "--version-name "+ versionName + " "+
                            "--manifest \"" + apkDirPath+ "\\AndroidManifest.xml\" " +
                            "-R \"" + apkResPath+ "\" " +
                            "--auto-add-overlay";
        cmd.Execute(apkBaseCmd,true,true);

        //再然后解压base.apk
        string tempBasePath = tempDirPath + "\\base";
        rar.Decompression(baseApkPath, tempBasePath);

        //再然后是资源整合拷贝等
        string targetPath = tempDirPath + "\\TempBase";
        Directory.CreateDirectory(targetPath);
        File.Copy(tempBasePath + "\\resources.pb", targetPath + "\\resources.pb");
        CopyDirectory(tempBasePath + "\\res", targetPath + "\\res");
        Directory.CreateDirectory(targetPath + "\\manifest");
        File.Copy(tempBasePath + "\\AndroidManifest.xml", targetPath + "\\manifest\\AndroidManifest.xml");
        CopyDirectory(apkDirPath + "\\assets", targetPath + "\\assets");
        CopyDirectory(apkDirPath + "\\lib", targetPath + "\\lib");
        Directory.CreateDirectory(targetPath + "\\root");
        if(Directory.Exists(apkDirPath+"\\unknown"))
        {
            CopyDirectory(apkDirPath + "\\unknown", targetPath + "\\root");
        }
        if(Directory.Exists(apkDirPath+"\\kotlin"))
        {
            CopyDirectory(apkDirPath + "\\kotlin", targetPath + "\\root");
        }
        Directory.CreateDirectory(targetPath + "\\root\\META-INF");
        Directory.CreateDirectory(targetPath + "\\dex");
        string[] dirFiles= Directory.GetFiles(apkDirPath);
        foreach(var file in dirFiles)
        {
            if(file.EndsWith(".dex"))
            {
                File.Copy(file, targetPath + "\\dex\\"+Path.GetFileName(file));
            }
        }

        //再然后是压缩base文件夹
        DirectoryInfo dirInfo = new DirectoryInfo(targetPath);
        foreach(var item in dirInfo.GetFileSystemInfos())
        {
            rar.Compression(item.FullName, tempDirPath + "\\base.zip");
        }

        //再然后是编译aab
        string noCompressFileList_AAB = channelInfo.NoCompressFileList_AAB;
        if (!string.IsNullOrEmpty(noCompressFileList_AAB))//判断是否有需要不压缩的资源
        {
            string bundleConfigPath = tempDirPath + "\\BundleConfig.json";
            string bundleStr = "{\n\"compression\":{\n\"uncompressedGlob\":["+noCompressFileList_AAB+"]\n}\n}";
            FileTool.WriteStringByFile(bundleConfigPath, bundleStr);
            cmd.Execute("java -jar " + EditorData.GetBundletoolPath() + " build-bundle --config=\"" + bundleConfigPath + "\" --modules=\"" + tempDirPath + "\\base.zip\" --output=\"" + aabDirPth+"\\"+apkName+ ".aab\"");
        }
        else
        {
            cmd.Execute("java -jar " + EditorData.GetBundletoolPath() + " build-bundle --modules=\"" + tempDirPath + "\\base.zip\" --output=\"" + aabDirPth+"\\"+apkName+ ".aab\"");
        }

        //再然后是签名
        string fairGuardConfigPath = Directory.GetCurrentDirectory().Replace("\\bin\\Debug","") + "\\FairGuard\\config.ini";
        string fairGuardPath = Directory.GetCurrentDirectory().Replace("\\bin\\Debug", "") + "\\FairGuard\\FairGuard3.1.8.jar";
        string configStr = "[gamekey]\nkey=" + "Test" + "\n[signinfo]\nkeystore-path=" + channelInfo.JksPath + "\nalias=" + channelInfo.KeyStoreAlias + "\npassword=" + channelInfo.KeyStorePassWord + "\nalias-pwd=" + channelInfo.KeyStoreAliasPassWord;
        FileTool.WriteStringByFile(fairGuardConfigPath, configStr);
        cmd.Execute("java -jar " + fairGuardPath + " -optype_sign_jar -inputfile \"" + aabDirPth+"\\" + apkName + ".aab\" -outputfile \"" + aabDirPth + "\\" + apkName+ "_sign.aab\"");
        File.Delete(aabDirPth + "\\" + apkName + ".aab");
        File.Move(aabDirPth + "\\" + apkName + "_sign.aab", aabDirPth + "\\" + apkName + ".aab");

        #region 暂时注释的代码，之后可能会重用

        //测试aab
        //cmd.Execute("java -jar " + EditorData.GetBundletoolPath() + " build-apks " +
        //            "--mode=universal "+
        //            "--bundle="+ aabDirPth + "\\" + apkName + "_sign.aab " +
        //            "--output="+ aabDirPth + "\\" + apkName + "_sign.apks " +
        //            "--ks="+ channelInfo.JksPath+" "+
        //            "--ks-pass=pass:"+ channelInfo.KeyStorePassWord+" "+
        //            "--ks-key-alias="+ channelInfo.KeyStoreAlias+" "+
        //            "--key-pass=pass:"+ channelInfo.KeyStorePassWord);
        //rar.Decompression(aabDirPth + "\\" + apkName + "_sign.apks", aabDirPth + "\\APK");

        //自动安装
        //if (EditorData.IsAutoInstall)
        //{
        //    cmd.Execute("java -jar " + EditorData.GetBundletoolPath() + " install-apks --apks=" + repackageInfo.exportPath + "\\AAB\\"+apkName+"\\"+apkName+".apks", true, true);
        //}

        //最后将解压的apk反编译，方便测试
        //cmd.Execute("java -jar " + EditorData.ApktoolVersion + ".jar d -f " + aabDirPth + "\\APK\\universal.apk" + " -s -o " + aabDirPth + "\\APK\\universal");

        #endregion

        #region 测试代码，

        //测试代码
        //string ymlStr = File.ReadAllText(repackageInfo.exportPath + "\\AAB\\APK\\universal\\apktool.yml");
        //string[] strs = ymlStr.Split(new string[] {"doNotCompress:"},StringSplitOptions.None);
        //string newStr = strs[0] + "doNotCompress:\n- assets/video/logo/logo_video_chinesesimplified" + strs[1];
        //File.WriteAllText(repackageInfo.exportPath + "\\AAB\\APK\\universal\\apktool.yml", newStr);
        //cmd.Execute("java -jar " + EditorData.ApktoolVersion + ".jar b " + repackageInfo.exportPath + "\\AAB\\APK\\universal");
        //cmd.Execute("jarsigner" // -verbose
        //            //+ " -tsa https://timestamp.geotrust.com/tsa"
        //            + " -digestalg SHA1 -sigalg MD5withRSA"
        //            + " -storepass " + channelInfo.KeyStorePassWord
        //            + " -keystore " + channelInfo.KeyStorePath
        //            + " -sigFile CERT"  //强制将RSA文件更名为CERT
        //            + " " + repackageInfo.exportPath + "\\AAB\\APK\\universal\\dist\\universal.apk"
        //            + " " + channelInfo.KeyStoreAlias
        //);

        #endregion

    }

    public void CopyDirectory(string sourcePath,string targetPath)
    {
        if(!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }
        DirectoryInfo dir = new DirectoryInfo(sourcePath);
        foreach(var item in dir.GetFiles())
        {
            File.Copy(item.FullName, targetPath + "\\" + Path.GetFileName(item.FullName));
        }
        foreach(var item in dir.GetDirectories())
        {
            CopyDirectory(item.FullName, targetPath + "\\" + Path.GetFileName(item.FullName));
        }
    }
}
