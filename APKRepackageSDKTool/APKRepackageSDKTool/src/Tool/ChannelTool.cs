using APKRepackageSDKTool.src;
using APKRepackageSDKTool.src.YML;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using YamlDotNet.Serialization.NamingConventions;

namespace APKRepackageSDKTool
{
    public class ChannelTool
    {
        OutPutCallBack callBack;
        OutPutCallBack errorCallBack;

        CompileTool compileTool;
        AndroidTool androidTool;
        RarTool rarTool;

        public ChannelTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
        {
            this.callBack = callBack;
            this.errorCallBack = errorCallBack;

            compileTool = new CompileTool(callBack, errorCallBack);
            androidTool = new AndroidTool(callBack, errorCallBack);
            rarTool = new RarTool(callBack, errorCallBack);
        }

        public void ChannelLogic(string filePath, ChannelInfo info)
        {
            //指定编译版本
            compileTool.assignMinAPILevel = info.GetAssignMinAPILevel();
            compileTool.minAPILevel = info.GetMinAPILevel();

            if (info.IsExecuteInvalidFile)
            {
                OutPut("处理无效文件");
                ExecuteInvalidFile(filePath);
            }

            if (!string.IsNullOrEmpty(info.PackageName))
            {
                OutPut("替换包名");
                androidTool.ChangePackageName(filePath, info.PackageName);
            }

            if (!string.IsNullOrEmpty(info.AppName))
            {
                OutPut("替换appName");
                androidTool.ChangeAppName(filePath, info.AppName);
            }

            if (info.appNameLanguages.Count > 0)
            {
                for (int i = 0; i < info.appNameLanguages.Count; i++)
                {
                    KeyValue kv = info.appNameLanguages[i];
                    OutPut("替换appName " + kv.key + " " + kv.value);
                    androidTool.ChangeAppNameByLanguage(filePath, kv.key, kv.value);
                }
            }

            if (!string.IsNullOrEmpty(info.AppIcon))
            {
                OutPut("替换appIcon");
                ChangeAppIcon(filePath, info.AppIcon);
            }

            if (!string.IsNullOrEmpty(info.AppBanner))
            {
                OutPut("替换AppBanner");
                ChangeAppBanner(filePath, info.AppBanner);
            }

            if (info.IsChangeMainApplication)
            {
                OutPut("替换MainApplication");
                androidTool.ChangeApplicationName(filePath, "sdkInterface.application.MainApplication");
            }

            if (info.isChangeMainActivity)
            {
                OutPut("替换MainActity");
                androidTool.ChangeMainActity(filePath);
            }

            if(info.IsExtractNativeLibs)
            {
                androidTool.ChangeExtractNativeLibs(filePath);
            }
            
            if (info.sdkList.Count > 0)
            {
                OutPut("放入SDK接口 ");
                PutSDKInterface(filePath);

                for (int i = 0; i < info.sdkList.Count; i++)
                {
                    OutPut("放入SDK " + info.sdkList[i].sdkName);
                    PutSDK(filePath, info.sdkList[i], info);
                }

                //使用Maven
                MavanLogic(filePath, info);


                //指令集设置
                ArmeabiConfig(filePath, info);
            }

            OutPut("移除编不过的资源");
            RemoveUnCompileResource(filePath);

            OutPut("写配置清单");
            SaveSDKManifest(filePath, info);

            if(info.propertiesList.Count > 0)
            {
                OutPut("写入配置");
                SaveProperties(filePath, info);
            }

            OutPut("整合权限");
            PermissionLogic(filePath, info);

            OutPut("整合SDKVsersion");
            SDKVersion(filePath, info);
        }

        public void OutPut(string content)
        {
            callBack?.Invoke(content);
        }

        public void ErrorOutPut(string content)
        {
            errorCallBack?.Invoke(content);
        }

        #region 处理无效文件

        
        void ExecuteInvalidFile(string filePath)
        {
            //递归替换关键字
            FileTool.RecursionFileExecute(filePath + "\\res" , "xml", (file) =>
            {
                String content = FileTool.ReadStringByFile(file);

                //去掉不合法的字符
                content = content.Replace("$avd_show_password", "avd_show_password");
                content = content.Replace("$avd_hide_password", "avd_hide_password");

                //去掉不合法的文件名 
                //res\drawable\$avd_hide_password__0.xml: Invalid file name: must contain only [a-z0-9_.]
                if (file.Contains("$"))
                {
                    string newFile = file.Replace("$", "");
                    FileTool.WriteStringByFile(newFile, content);
                    File.Delete(file);
                }
                else
                {
                    FileTool.WriteStringByFile(file, content);
                }
            });
        }

        #endregion

        #region 重新生成R表

        public void Rebuild_R_Table(string aimPath,ChannelInfo channel)
        {
            OutPut("创建临时目录");
            String R_Path = PathTool.GetCurrentPath() + "\\R_path\\";

            FileTool.CreatePath(R_Path);

            string manifest = aimPath + "\\AndroidManifest.xml";
            string resPath = aimPath + "\\res";
            string smaliPath = aimPath + "\\smali";

            CmdService cmd = new CmdService(OutPut, errorCallBack);
            OutPut("生成R.java文件");

            //生成R文件
            if (!channel.IsUseAAPT2)
            {
                cmd.Execute(EditorData.GetAAPTPath() + " package -f"
                    + " -I " + EditorData.GetAndroidJarPath(EditorData.APILevel) 
                    + " -m -J " + R_Path 
                    + " -S " + resPath
                    + " -M " + manifest);
            }
            else
            {
                //aapt2
                string tempRes = PathTool.GetCurrentPath() + "\\res.zip";
                string tempAPK = PathTool.GetCurrentPath() + "\\apk.apk";

                //编译
                cmd.Execute(EditorData.GetAAPT2Path() + " compile --dir " + resPath + " -o " + tempRes,true,true);
                //链接
                cmd.Execute(EditorData.GetAAPT2Path() + " link " 
                    + tempRes
                    + " -I " + EditorData.GetAndroidJarPath(EditorData.APILevel) 
                    + " --java " + R_Path 
                    + " --manifest " + manifest 
                    + " -o " + tempAPK, true, true);

                //删除临时文件
                File.Delete(tempAPK);
                File.Delete(tempRes);
            }

            string javaPath = FindRPath(R_Path);

            if(!string.IsNullOrEmpty(javaPath))
            {
                string metaPackageName = androidTool.GetPackageName(manifest);
                RTableUtil tr = new RTableUtil(callBack,errorCallBack);

                //tr.GenerateRKV(javaPath);
                //tr.ReplaceRTable(smaliPath);

                compileTool.RJava2Smali(R_Path, aimPath);

                //将模板文件覆盖到所有Styleable文件上
                string templatePath = smaliPath + "\\" + metaPackageName.Replace(".","\\");
                //string temp = FileTool.ReadStringByFile(templatePath);

                //OutPut("metaPackageName " + metaPackageName);
                //OutPut("templatePath " + templatePath);

                if(Directory.Exists(smaliPath))
                {
                    tr.ReplaceRsmali(smaliPath, metaPackageName, GenerateTemplate(templatePath));
                    FileTool.DeleteDirectoryComplete(R_Path);
                }
                else
                {
                    ErrorOutPut("找不到模板文件 " + smaliPath);
                }
            }
            else
            {
                ErrorOutPut("找不到R文件，请检查清单文件");
            }

            //compileTool.RJava2Smali(R_Path, aimPath);

            //string metaPackageName = androidTool.GetPackageName(manifest);
            //string metaRfile = FindRPath(R_Path);
            //string content = FileTool.ReadStringByFile(metaRfile);

            ////清空R_Path路径
            //FileTool.DeleteDirectory(R_Path);

            //List<string> Rlist = FindRList(aimPath);

            //for (int i = 0; i < Rlist.Count; i++)
            //{
            //    string sub_packageName = GetPackageNameByPath(Rlist[i]);
            //    string sub_javaPath = R_Path + GetJavaPath(Rlist[i]) + "\\R.java";
                
            //    OutPut("sub_packageName " + sub_packageName);
            //    OutPut("sub_javaPath " + sub_javaPath);

            //    FileTool.CreateFilePath(sub_javaPath);
            //    FileTool.WriteStringByFile(sub_javaPath, content.Replace(metaPackageName, sub_packageName));

            //    compileTool.RJava2Smali(R_Path, aimPath);

            //    //File.Delete(sub_javaPath);
            //    FileTool.DeleteDirectory(R_Path);
            //}
        }

        //构造模板
        Dictionary<string,string> GenerateTemplate(string smaliPath)
        {
            Dictionary<string, string> template = new Dictionary<string, string>();

            List<string> list = FileTool.GetAllFileNamesByPath(smaliPath, new string[] { "smali" });

            for (int i = 0; i < list.Count; i++)
            {
                string fileName = FileTool.GetFileNameByPath(list[i]);
                if (fileName.StartsWith("R$"))
                {
                    template.Add(fileName, FileTool.ReadStringByFile(list[i]));

                    //OutPut("构造模板 " + fileName + " " + list[i]);
                }
            }

            return template;
        }

        //string GetPackageNameByPath(string path)
        //{
        //    string packageName = "";

        //    packageName = path.Replace("/", "\\").Replace("\\R.smali","");
        //    int index = packageName.LastIndexOf("smali\\");

        //    packageName = packageName.Substring(index).Replace("\\",".").Replace("smali.","");

        //    return packageName;
        //}

        //string GetJavaPath(string path)
        //{
        //    string javaPath = "";

        //    javaPath = path.Replace("/", "\\").Replace("\\R.smali", "");
        //    int index = javaPath.LastIndexOf("smali\\");

        //    javaPath = javaPath.Substring(index).Replace("smali\\", "");

        //    return javaPath;
        //}

        //List<string> FindRList(string path)
        //{
        //    List<string> list = new List<string>();

        //    RecursionFind(list,path);

        //    return list;
        //}

        //void RecursionFind(List<string> list, string path)
        //{
        //    string[] allfileName = Directory.GetFiles(path);
        //    foreach (var item in allfileName)
        //    {
        //        if (item.Contains("\\R.smali"))
        //        {
        //            list.Add(item);
        //            OutPut("找到R路径 " + item);
        //        }
        //    }

        //    string[] dires = Directory.GetDirectories(path);
        //    for (int i = 0; i < dires.Length; i++)
        //    {
        //        RecursionFind(list, dires[i]);
        //    }
        //}

        String FindRPath(string path)
        {
            try
            {
                //递归寻找目标文件路径并输出
                return FileTool.GetAllFileNamesByPath(path, new string[] { "java" })[0];
            }
            catch(Exception)
            {
                return null;
            }
        }

        #endregion

        #region 替换图片

        void ChangeAppIcon(string filePath, string appIcon)
        {
            string exportPath = filePath + "\\res\\drawable-hdpi-v4\\app_icon.png";
            if (File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 72, 72);

            exportPath = filePath + "\\res\\drawable-ldpi-v4\\app_icon.png";
            if (File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 36, 36);

            exportPath = filePath + "\\res\\drawable-mdpi-v4\\app_icon.png";
            if (File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 48, 48);

            exportPath = filePath + "\\res\\drawable-xhdpi-v4\\app_icon.png";
            if (File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 96, 96);

            exportPath = filePath + "\\res\\drawable-xxhdpi-v4\\app_icon.png";
            if(File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 144, 144);

            exportPath = filePath + "\\res\\drawable-xxxhdpi-v4\\app_icon.png";
            if (File.Exists(exportPath))
                ExportImage(exportPath, appIcon, 192, 192);
        }

        void ChangeAppBanner(string filePath, string appBanner)
        {
            string exportPath = filePath + "\\res\\drawable-xhdpi-v4\\app_banner.png";

            ExportImage(exportPath, appBanner, 320, 180);
        }

        void ExportImage(string exportPath,string scorePath, int width,int height)
        {
            System.Drawing.Size size = new System.Drawing.Size(width, height);

            Image img = Image.FromFile(scorePath);

            img = ResizeImage(img, size);

            img.Save(exportPath);
        }

        private Image ResizeImage(Image imgToResize, System.Drawing.Size size)
        {
            Bitmap b = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //绘制图像
            g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
            g.Dispose();
            return b;
        }

        #endregion

        #region 放入SDK

        void PutSDKInterface(string filePath)
        {
            //将Interface文件夹中的所有jar放到apk中
            string libPath = EditorData.SdkLibPath + "\\Interface";

            List<string> jarList = FileTool.GetAllFileNamesByPath(libPath, new string[] { "jar" }, false);

            for (int i = 0; i < jarList.Count; i++)
            {
                compileTool.Jar2SmaliByCache(jarList[i], filePath);
            }
        }

        void SaveSDKManifest(string filePath, ChannelInfo info)
        {
            string path = filePath + "\\assets\\SdkManifest.properties";
            string content = "";

            foreach (SDKType item in Enum.GetValues(typeof(SDKType)))
            {
                string key = item.ToString();
                string value = "";

                for (int i = 0; i < info.sdkList.Count; i++)
                {
                    SDKInfo si = info.sdkList[i];
                    SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(si.sdkName);
                    if ((config.sdkType & item) != 0)
                    {
                        if(config.className == null)
                        {
                            config.className = config.SdkName + "<NullClassName>";
                        }

                        if(value =="")
                        {
                            value += config.className;
                        }
                        else
                        {
                            value += "|" + config.className;
                        }
                    }
                }
                content += key + "=" + value + "\n";
            }

            //content += "\n";

            for (int i = 0; i < info.sdkList.Count; i++)
            {
                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkList[i].sdkName);
                content += config.className + "=" + config.sdkName + "\n";
            }

            if(info.isLog)
            {
                content += "IsLog=true\n";
            }

            if (info.IsSplitDex)
            {
                content += "IsMultiDex=true\n";
            }

            FileTool.WriteStringByFile(path, content);
        }

        void SaveProperties(string filePath, ChannelInfo info)
        {
            string path = filePath + "\\assets\\Channel.properties";
            string content = "";

            for (int i = 0; i < info.propertiesList.Count; i++)
            {
                content += info.propertiesList[i].key + "=" + info.propertiesList[i].value + "\n";
            }

            FileTool.WriteStringByFile(path, content);
        }

        void PutSDK(string filePath,SDKInfo info, ChannelInfo channelInfo)
        {
            SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkName);

            //代码剔除
            if(config.excludeSmaliList.Count>0)
            {
                OutPut("准备剔除 Smali 文件，剔除设置数量为： " + config.excludeSmaliList.Count);
                ExcludeSmali(filePath, config);
            }

            //添加Jar
            OutPut("添加Jar " + info.sdkName);
            PutJar(filePath, info);

            //处理aar
            PutAAR( filePath,  info,  channelInfo);

            //手动移除无法编译通过的字段
            androidTool.RemoveErrordManifest(filePath);

            //自动编译类
            if (config.useCustomJavaClass)
            {
                OutPut("自动编译 " );
                compileTool.CompileCustomJavaClass(config, channelInfo, filePath);
            }

            //拷贝资源文件
            OutPut("拷贝资源文件 " + info.sdkName);
            CopyFile(filePath,info, channelInfo);

            //添加标签头
            for (int i = 0; i < config.XmlHeadList.Count; i++)
            {
                OutPut("添加XMLHead " + info.sdkName + " " + config.XmlHeadList[i].key);
                androidTool.AddXMLHead(filePath, config.XmlHeadList[i], info, channelInfo);
            }

            //添加Application头
            for (int i = 0; i < config.ApplicationHeadList.Count; i++)
            {
                OutPut("添加ApplicationHead " + info.sdkName + " " + config.ApplicationHeadList[i].Key);
                androidTool.AddApplicationHead(filePath, config.ApplicationHeadList[i], info, channelInfo);
            }

            //添加Activity
            for (int i = 0; i < config.ActivityInfoList.Count; i++)
            {
                OutPut("添加Activity " + info.sdkName + " " + config.ActivityInfoList[i].name);
                androidTool.AddActivity(filePath, config.ActivityInfoList[i], info, channelInfo);
            }

            //添加MainActivityProperty
            for (int i = 0; i < config.mainActivityPropertyList.Count; i++)
            {
                OutPut("添加mainActivityProperty " + info.sdkName + " " + config.mainActivityPropertyList[i].key);
                androidTool.AddMainActivityProperty(filePath, config.mainActivityPropertyList[i], info, channelInfo);
            }

            //添加Service
            for (int i = 0; i < config.serviceInfoList.Count; i++)
            {
                OutPut("添加Service " + info.sdkName + " " + config.serviceInfoList[i].name);
                androidTool.AddService(filePath, config.serviceInfoList[i], channelInfo , info);
            }

            //添加Provider
            for (int i = 0; i < config.providerInfoList.Count; i++)
            {
                OutPut("添加Provider " + info.sdkName + " " + config.providerInfoList[i].name);
                androidTool.AddProvider(filePath, config.providerInfoList[i], channelInfo, info);
            }

            //添加Meta字段
            for (int i = 0; i < config.metaInfoList.Count; i++)
            {
                OutPut("添加Meta " + info.sdkName + " " + config.metaInfoList[i].key);
                androidTool.AddMeta_Application(filePath, config.metaInfoList[i], channelInfo, info);
            }

            //添加Uses字段
            for (int i = 0; i < config.usesList.Count; i++)
            {
                OutPut("添加Uses " + info.sdkName + " " + config.usesList[i].key);
                androidTool.AddUses(filePath, config.usesList[i], channelInfo, info);
            }

            //修改ApplicationName
            if (!string.IsNullOrEmpty(config.ApplicationName))
            {
                OutPut("修改ApplicationName " + config.ApplicationName);
                androidTool.ChangeApplicationName(filePath, config.ApplicationName);
            }

            //添加配置文件
            OutPut("添加配置文件 " + info.sdkName);
            SaveSDKConfigFile(filePath, info);
        }

        void PutJar(string filePath, SDKInfo info)
        {
            string libPath = EditorData.SdkLibPath + "\\" + info.sdkName;

            List<string> jarList = FileTool.GetAllFileNamesByPath(libPath, new string[] { "jar" }, false);

            for (int i = 0; i < jarList.Count; i++)
            {
                compileTool.Jar2SmaliByCache(jarList[i], filePath);
            }
        }

        void PutAAR(string filePath, SDKInfo info, ChannelInfo channelInfo)
        {
            string aarPath = EditorData.SdkLibPath + "\\" + info.sdkName + "\\aar";
            string rarPath = EditorData.SdkLibPath + "\\" + info.sdkName + "\\rar";

            //先判断有没有aar文件夹
            if (!Directory.Exists(aarPath))
            {
                return;
            }

            List<string> aarList = FileTool.GetAllFileNamesByPath(aarPath, new string[] { "aar" }, false);

            for (int i = 0; i < aarList.Count; i++)
            {
                string aarFilePath = aarList[i];
                string rarFilePath = rarPath + "\\" + FileTool.RemoveExpandName(FileTool.GetFileNameByPath(aarFilePath)) + ".rar" ;
                string rarDecompressionPath = rarPath + "\\" + FileTool.RemoveExpandName(FileTool.GetFileNameByPath(aarFilePath));

                if (!Directory.Exists(rarPath))
                {
                    Directory.CreateDirectory(rarPath);
                }

                //转储为rar并解压

                if (!File.Exists(rarFilePath))
                {
                    //转储RAR
                    File.Copy(aarFilePath, rarFilePath);
                }

                //如果没有解压就现场解压
                if (!Directory.Exists(rarDecompressionPath))
                {
                    if (File.Exists(rarFilePath))
                    {
                        rarTool.Decompression(rarFilePath);
                    }
                    else
                    {
                        ErrorOutPut("E: 找不到资源 >" + rarFilePath + "<");
                        return;
                    }
                }

                //提取本体
                androidTool.ExtractAAR2APK(rarDecompressionPath, filePath, channelInfo);
            }
        }

        void SaveSDKConfigFile(string filePath , SDKInfo info)
        {
            //TODO 加密此处以免破解
            SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkName);
            string path = filePath + "\\assets\\"+ config.sdkName+ ".properties";

            string content = "";
            for (int i = 0; i < info.sdkConfig.Count; i++)
            {
                KeyValue kv = info.sdkConfig[i];

                content += kv.key + "=" + kv.value + "\n";
            }

            FileTool.WriteStringByFile(path, content);
        }

        //整合权限
        void PermissionLogic(string filePath, ChannelInfo info)
        {
            List<string> permissionList = new List<string>();

            for (int i = 0; i < info.sdkList.Count; i++)
            {
                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkList[i].sdkName);

                for (int j = 0; j < config.permissionList.Count; j++)
                {
                    //权限去重
                    string permission = config.permissionList[j];

                    //替换关键字
                    permission = AndroidTool.ReplaceKeyWordByChannelInfo(permission, info);
                    permission = AndroidTool.ReplaceKeyWordbySDKInfo(permission, info.sdkList[i]);

                    if (!permissionList.Contains(permission))
                    {
                        permissionList.Add(permission);
                    }
                }
            }

            for (int i = 0; i < permissionList.Count; i++)
            {
                OutPut("权限 " + permissionList[i]);

                androidTool.AddPermission(filePath, permissionList[i]);
            }
        }

        void CopyFile(string filePath,SDKInfo info, ChannelInfo channelInfo)
        {
            //TODO 这里如果可以通过aapt取代当然最好


            string SDKPath = EditorData.SdkLibPath + "\\" + info.sdkName;

            DirectoryInfo directoryInfo = new DirectoryInfo(SDKPath);
            DirectoryInfo[] directoryInfoArray = directoryInfo.GetDirectories();
            foreach (DirectoryInfo dir in directoryInfoArray)
            {
                string dirName = FileTool.GetDirectoryName(dir.FullName);

                //只拷贝这四个目录
                if(dirName.Contains("lib"))
                {
                    OutPut("拷贝lib文件 " + info.sdkName + " " + dirName);
                    FileTool.CopyDirectory(dir.FullName, filePath + "\\" + dirName);
                }

                if(dirName.Contains("assets"))
                {
                    OutPut("拷贝assets文件 " + info.sdkName + " " + dirName);
                    FileTool.CopyDirectory(dir.FullName, filePath + "\\" + dirName);

                    //递归替换关键字
                    FileTool.RecursionFileExecute(filePath + "\\" + dirName, "ini", (file) =>
                    {
                        String content = FileTool.ReadStringByFile(file);
                        content = AndroidTool.ReplaceKeyWordByChannelInfo(content, channelInfo);
                        content = AndroidTool.ReplaceKeyWordbySDKInfo(content, info);

                        FileTool.WriteStringByFile(file, content);
                    });
                }

                //合并res文件
                if (dirName.Contains("res"))
                {
                    OutPut("合并res文件夹 " + info.sdkName + " " + dirName);
                    MergeResTool mergeRes = new MergeResTool(callBack, errorCallBack);
                    mergeRes.Merge(dir.FullName, filePath + "\\res");
                    //替换关键字
                    AndroidTool.ReplaceKeyWordByDiretory(filePath + "\\res\\values", channelInfo, info);
                }

                //合并smali文件
                if (dirName.Contains("\\smali"))
                {
                    OutPut("合并smali文件 " + info.sdkName + " " + dirName);
                    FileTool.SafeCopyDirectory(dir.FullName, filePath + "\\" + dirName);
                }
            }
        }
        #endregion

        #region 代码剔除

        void ExcludeSmali(string aimPath, SDKConfig config)
        {
            string smaliPath = aimPath.Replace("\\", "/") + "/smali";
            List<string> list = FileTool.GetAllFileNamesByPath(smaliPath, new string[] { "smali" });

            OutPut("Smali 总文件数目为 " + list.Count);

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].Replace("\\", "/");
                for (int j = 0; j < config.excludeSmaliList.Count; j++)
                {
                    if (list[i].Contains(config.excludeSmaliList[j]))
                    {
                        FileTool.DeleteFile(list[i]);
                        //OutPut("剔除 Smali 文件 " + list[i]);
                        break;
                    }
                }
            }
        }

        #endregion

        #region 整合SDK

        public void SDKVersion(string filePath,ChannelInfo info)
        {
            int minSDKVersion = 0;
            int targetSDKVersion = 0;

            for (int i = 0; i < info.sdkList.Count; i++)
            {
                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkList[i].sdkName);
                if(config.minSDKversion != 0)
                {
                    if(minSDKVersion == 0)
                    {
                        minSDKVersion = config.minSDKversion;
                    }
                    else
                    {
                        //最小SDKVersion取所有SDK设置中最大的
                        if(config.minSDKversion > minSDKVersion)
                        {
                            minSDKVersion = config.minSDKversion;
                        }
                    }
                }

                if(config.targetSDKVersion != 0)
                {
                    if (targetSDKVersion == 0)
                    {
                        targetSDKVersion = config.targetSDKVersion;
                    }
                    else
                    {
                        //目标SDKVersion取所有SDK设置中最大的
                        if (config.targetSDKVersion > targetSDKVersion)
                        {
                            targetSDKVersion = config.targetSDKVersion;
                        }
                    }
                }
            }
            androidTool.ChangeSDKVersion(filePath, minSDKVersion, targetSDKVersion);
        }

        #endregion

        #region Maven
        void MavanLogic(string filePath, ChannelInfo info)
        {
            List<string> mavenPath = new List<string>();
            List<string> mavenLibPath = new List<string>();
            for (int i = 0; i < info.sdkList.Count; i++)
            {
                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkList[i].sdkName);
                if (config.useMaven)
                {
                    mavenPath.AddRange(config.mavenPathList);
                    mavenLibPath.AddRange(config.mavenLibList);
                }
            }

            MavenTool mt = new MavenTool(EditorData.MavenCachePath,callBack, errorCallBack);

            mt.ExtractMavenFile(mavenPath, mavenLibPath, filePath, info);
        }

        #endregion 

        #region 指令集设置

        void ArmeabiConfig(string filePath, ChannelInfo info)
        {
            //强制32位模式
            bool delete_64v8a = false;
            bool delete_armeabi = false;

            bool delete_x86 = false;
            bool delete_x86_64 = false;

            bool delete_mips = false;
            bool delete_mip64 = false;

            for (int i = 0; i < info.sdkList.Count; i++)
            {
                //OutPut("强制32位模式 " + info.sdkList[i].sdkName + " " + config.force32bit);

                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkList[i].sdkName);

                delete_64v8a |= config.Delete_armeabi_v8a;
                delete_armeabi |= config.Delete_armeabi;

                delete_x86 |= config.Delete_x86;
                delete_x86_64 |= config.Delete_x86_64;

                delete_mips |= config.Delete_mips;
                delete_mip64 |= config.Delete_mip64;
            }

            OutPut("删除不必要的的so库");

            if (delete_armeabi)
            {
                OutPut("删除armeabi文件");

                Delete(filePath + "/lib/armeabi");
            }

            if (delete_64v8a)
            {
                OutPut("删除arm64-v8a文件");
                
                Delete(filePath + "/lib/arm64-v8a");
            }

            if(delete_x86)
            {
                OutPut("删除x86文件");
                Delete(filePath + "/lib/x86");
            }

            if (delete_x86_64)
            {
                OutPut("删除x86_64文件");
                Delete(filePath + "/lib/x86_64");
            }

            if (delete_mips)
            {
                OutPut("删除mips文件");
                Delete(filePath + "/lib/mips");
            }

            if (delete_mip64)
            {
                OutPut("删除mips64文件");
                Delete(filePath + "/lib/mips64");
            }
        }

        void Delete(String path)
        {
            if (Directory.Exists(path))
            {
                FileTool.DeleteDirectory(path);
                Directory.Delete(path);
            }
        }

        #endregion

        #region 移除编不过的资源

        void RemoveUnCompileResource(string filePath)
        {
            string resPath = filePath + "\\res";

            string[] directorys = Directory.GetDirectories(resPath);

            //删掉所有子目录
            for (int i = 0; i < directorys.Length; i++)
            {
                string pathTmp = directorys[i];
                string dictName = FileTool.GetFileNameBySring(pathTmp);

                if (dictName.Contains("-watch-"))
                {
                    FileTool.DeleteDirectoryComplete(pathTmp);
                    OutPut("移除编不过的路径 " + pathTmp);
                }
            }
        }

        #endregion

        #region YML

        public void YMLLogic(string filePath)
        {
            string path = filePath + @"\apktool.yml";
            var input = new StringReader(path);

            YML yml = new YML(path);

            //不压缩逻辑
            List<string> noCompressList = new List<string>();

            var list = yml.FindChilenList("doNotCompress");
            for (int i = 0; i < list.Count; i++)
            {
                string value = list[i].value;
                if (value.Contains(".video")
                    || value.Contains(".so")
                     //|| value.Contains(".resource")
                     //|| value.Contains(".png")
                     )
                {
                    noCompressList.Add(value);
                }
            }

            yml.DeleteAllChildNode("doNotCompress");
            yml.AddNodeByKey("doNotCompress", "resources.arsc");
            yml.AddNodeByKey("doNotCompress", "resource");
            yml.AddNodeByKey("doNotCompress", "png");

            for (int i = 0; i < noCompressList.Count; i++)
            {
                yml.AddNodeByKey("doNotCompress", noCompressList[i]);
            }
            yml.save();
        }

        ///// <summary>
        ///// 创建额外文件并清空yml doNotCompress 节点
        ///// </summary>
        //public void CreateExtensionsFileAndClearDoNotCompressNode(string filePath,string extensionsFilePath)
        //{
        //    string content = "";

        //    string path = filePath + @"\apktool.yml";
        //    var input = new StringReader(path);

        //    YML yml = new YML(path);

        //    var list = yml.FindChilenList("doNotCompress");
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        string value = list[i].value;

        //        content += value;

        //        if( i != list.Count -1)
        //        {
        //            content += "\n";
        //        }
        //    }

        //    yml.DeleteAllChildNode("doNotCompress");
        //    yml.save();

        //    //写入额外文件
        //    FileTool.WriteStringByFile(extensionsFilePath, content);
        //}

        #endregion

        #region 混淆Dll

        public void ConfusionDLL(string filePath)
        {
            string dllPath = filePath + @"\assets\bin\Data\Managed\Assembly-CSharp.dll";
            string outPath = filePath + @"\assets\bin\Data\Managed\Assembly-CSharp_Secure\Assembly-CSharp.dll";
            string outDir = filePath + @"\assets\bin\Data\Managed\Assembly-CSharp_Secure";

            CmdService cmd = new CmdService(OutPut, errorCallBack);
            //混淆
            cmd.Execute(@"DotNETReactor\dotNET_Reactor.exe -file " + dllPath);

            //覆盖
            File.Copy(outPath, dllPath,true);

            //删除旧文件
            File.Delete(outPath);
            Directory.Delete(outDir);
        }

        #endregion

    }
}
