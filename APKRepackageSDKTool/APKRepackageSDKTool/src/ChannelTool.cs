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

namespace APKRepackageSDKTool
{
    public class ChannelTool
    {
        OutPutCallBack callBack;
        OutPutCallBack errorCallBack;
        public ChannelTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
        {
            this.callBack = callBack;
            this.errorCallBack = errorCallBack;
        }

        public void ChannelLogic(string filePath, ChannelInfo info)
        {
            if (!string.IsNullOrEmpty(info.PackageName))
            {
                OutPut("替换包名");
                ChangePackageName(filePath, info.PackageName);
            }

            if (!string.IsNullOrEmpty(info.AppName))
            {
                OutPut("替换appName");
                ChangeAppName(filePath, info.AppName);
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

            if (info.sdkList.Count > 0)
            {
                OutPut("放入SDK接口 ");
                PutSDKInterface(filePath);

                for (int i = 0; i < info.sdkList.Count; i++)
                {
                    OutPut("放入SDK " + info.sdkList[i].sdkName);
                    PutSDK(filePath, info.sdkList[i]);
                }
            }

            OutPut("写配置清单");
            SaveSDKManifest(filePath, info);

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
        #region AndroidManifest 修改

        public void ChangePackageName(string filePath, string packageName)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlElement nodeEle = (XmlElement)manifest;

            nodeEle.SetAttribute("package", packageName);
            xmlDoc.Save(xmlPath);
        }

        public void ChangeAppName(string filePath, string appName)
        {
            string xmlPath = filePath + "\\res\\values\\strings.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode resources =  xmlDoc.SelectSingleNode("resources");

            //遍历String表，替换App_name
            for (int i = 0; i < resources.ChildNodes.Count; i++)
            {
                XmlNode node = resources.ChildNodes[i];
                XmlElement nodeEle = (XmlElement)node;
                if (node.Name == "string"
                    && nodeEle.GetAttribute("name") == "app_name" )
                {
                    nodeEle.InnerText = appName;
                    break;
                }
            }

            xmlDoc.Save(xmlPath);
        }

        /// <summary>
        /// 添加权限
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="Permission"></param>
        void AddPermission(string filePath, string permission)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");

            //权限判重
            for (int i = 0; i < manifest.ChildNodes.Count; i++)
            {
                XmlNode node = manifest.ChildNodes[i];
                XmlElement ele = (XmlElement)node;
                if (ele.GetAttribute("name", "http://schemas.android.com/apk/res/android") == "android.permission." + permission)
                {
                    return;
                }
            }

            XmlElement nd = xmlDoc.CreateElement("uses-permission");
            nd.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.permission." + permission);

            manifest.AppendChild(nd);
            xmlDoc.Save(xmlPath);
        }

        void ChangeSDKVersion(string filePath, int minSDKVersion,int targetSDKVersion)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlNode useSdk = GetNode(manifest, "uses-sdk");

            if (useSdk == null)
            {
                useSdk = xmlDoc.CreateElement("uses-sdk");
                manifest.AppendChild(useSdk);
            }

            XmlElement sdkEle = (XmlElement)useSdk;

            if (minSDKVersion > 0)
            {
                sdkEle.SetAttribute("minSdkVersion", "http://schemas.android.com/apk/res/android", minSDKVersion.ToString());
            }

            if (targetSDKVersion > 0)
            {
                sdkEle.SetAttribute("targetSdkVersion", "http://schemas.android.com/apk/res/android", targetSDKVersion.ToString());
            }

            xmlDoc.Save(xmlPath);
        }

        #region 添加Activity与Service

        void AddActivity(string filePath, ActivityInfo info)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";

            //移除旧MainActivity
            if (info.MainActivity)
            {
                RemoveOldMainActivity(filePath);
            }

            string xml = FileTool.ReadStringByFile(xmlPath);
            int index = xml.IndexOf("</application>");
            xml = xml.Insert(index, info.content);

            //直接保存
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            xmlDoc.Save(xmlPath);

            //添加新MainActivity
            if (info.MainActivity)
            {
                AddMainActivity(filePath, info);
            }
        }

        void RemoveOldMainActivity(string filePath)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            //直接保存
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlNode app = GetNode(manifest, "application");

            //权限判重
            for (int i = 0; i < app.ChildNodes.Count; i++)
            {
                XmlNode node = app.ChildNodes[i];
                XmlElement ele = (XmlElement)node;
                for (int j = 0; j < ele.ChildNodes.Count; j++)
                {
                    XmlNode node2 = ele.ChildNodes[j];

                    if (node2.Name == "intent-filter")
                    {
                        XmlNode action = GetNode(node2, "category");
                        XmlElement ele2 = (XmlElement)action;

                        if (ele2.GetAttribute("name", "http://schemas.android.com/apk/res/android") == "android.intent.category.LAUNCHER")
                        {
                            ele.RemoveChild(node2);
                            break;
                        }
                    }
                }
            }

            xmlDoc.Save(xmlPath);
        }

        XmlNode GetNode(XmlNode parent, string name)
        {
            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                if (parent.ChildNodes[i].Name == name)
                {
                    return parent.ChildNodes[i];
                }
            }

            return null;
        }

        void AddMainActivity(string filePath, ActivityInfo info)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            //直接保存
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlNode app = GetNode(manifest, "application");

            //权限判重
            for (int i = 0; i < app.ChildNodes.Count; i++)
            {
                XmlNode node = app.ChildNodes[i];
                XmlElement ele = (XmlElement)node;

                string Attribute = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                if (Attribute.Contains(info.name))
                {
                    XmlElement nd = xmlDoc.CreateElement("intent-filter");
                    node.AppendChild(nd);

                    XmlElement nd1 = xmlDoc.CreateElement("action");
                    nd1.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.action.MAIN");

                    XmlElement nd2 = xmlDoc.CreateElement("category");
                    nd2.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.category.LAUNCHER");
                    XmlElement nd3 = xmlDoc.CreateElement("category");
                    nd3.SetAttribute("name", "http://schemas.android.com/apk/res/android", "android.intent.category.LEANBACK_LAUNCHER");

                    nd.AppendChild(nd1);
                    nd.AppendChild(nd2);
                    nd.AppendChild(nd3);
                    break;
                }
            }

            xmlDoc.Save(xmlPath);
        }

        void AddService(string filePath, ServiceInfo info)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            int index = xml.IndexOf("</application>");
            xml = xml.Insert(index, info.content);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            xmlDoc.Save(xmlPath);
        }

        void ChangeApplicationName(string filePath,string applicationName)
        {
            string xmlPath = filePath + "\\AndroidManifest.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
            XmlNode app = GetNode(manifest, "application");
            XmlElement ele = (XmlElement)app;

            ele.SetAttribute("name", "http://schemas.android.com/apk/res/android", applicationName);

            xmlDoc.Save(xmlPath);

        }

        #endregion

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
            string interfacePath = EditorData.SdkLibPath + "\\Interface\\SDKInterface.jar";
            Jar2Smali(interfacePath, filePath);
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

            FileTool.WriteStringByFile(path, content);
        }

        void PutSDK(string filePath,SDKInfo info)
        {
            SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.sdkName);

            //添加Jar
            OutPut("添加Jar " + info.sdkName);
            PutJar(filePath, info);

            //拷贝资源文件
            OutPut("拷贝资源文件 " + info.sdkName);
            CopyFile(filePath,info);

            //添加Activity
            for (int i = 0; i < config.ActivityInfoList.Count; i++)
            {
                OutPut("添加Activity " + info.sdkName + " " + config.ActivityInfoList[i].name);
                AddActivity(filePath, config.ActivityInfoList[i]);
            }

            //添加Service
            for (int i = 0; i < config.serviceInfoList.Count; i++)
            {
                OutPut("添加Service " + info.sdkName + " " + config.serviceInfoList[i].name);
                AddService(filePath, config.serviceInfoList[i]);
            }

            if(!string.IsNullOrEmpty(config.ApplicationName))
            {
                ChangeApplicationName(filePath, config.ApplicationName);
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
                Jar2Smali(jarList[i], filePath);
            }
        }

        void SaveSDKConfigFile(string filePath , SDKInfo info)
        {
            //TODO 加密此处以免破解
            string path = filePath + "\\assets\\"+ info.SdkName+ ".properties";

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
                    if (!permissionList.Contains(permission))
                    {
                        permissionList.Add(permission);
                    }
                }
            }

            for (int i = 0; i < permissionList.Count; i++)
            {
                OutPut("权限 " + permissionList[i]);

                AddPermission(filePath, permissionList[i]);
            }
        }

        void CopyFile(string filePath,SDKInfo info)
        {
            string SDKPath = EditorData.SdkLibPath + "\\" + info.sdkName;

            DirectoryInfo directoryInfo = new DirectoryInfo(SDKPath);
            DirectoryInfo[] directoryInfoArray = directoryInfo.GetDirectories();
            foreach (DirectoryInfo dir in directoryInfoArray)
            {
                string dirName = FileTool.GetDirectoryName(dir.FullName);

                //只拷贝这三个目录
                if(dirName.Contains("assets")
                    || dirName.Contains("lib")
                     || dirName.Contains("res")
                    )
                {
                    FileTool.CopyDirectory(dir.FullName, filePath + "\\" + dirName);
                }
            }
        }



        #endregion

        #region Java重编译

        public void Jar2Smali(string jarPath,string filePath)
        {
            string smaliPath = filePath + "\\smali";
            string JavaTempPath = PathTool.GetCurrentPath() + "\\JavaTempPath";
            string jarName = FileTool.GetFileNameByPath(jarPath);
            string tempPath = JavaTempPath + "\\" + jarName;

            FileTool.CreatPath(JavaTempPath);

            CmdService cmd = new CmdService(OutPut, errorCallBack);
            //Jar to dex
            cmd.Execute("java -jar dx.jar --dex --output=" + tempPath + " " + jarPath);

            //dex to smali
            cmd.Execute("java -jar baksmali-2.1.3.jar --o=" + smaliPath + " " + tempPath);

            //删除临时目录
            FileTool.DeleteDirectory(JavaTempPath);
            Directory.Delete(JavaTempPath);
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
                        //最小SDKVersion取所有SDK设置中最小的
                        if(config.minSDKversion < minSDKVersion)
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
            ChangeSDKVersion(filePath, minSDKVersion, targetSDKVersion);
        }

        #endregion

    }

}
