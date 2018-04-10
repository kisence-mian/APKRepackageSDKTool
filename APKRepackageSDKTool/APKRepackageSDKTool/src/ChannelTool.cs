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
        public ChannelTool(OutPutCallBack callBack)
        {
            this.callBack = callBack;
        }

        public void ChannelLogic(string filePath, ChannelInfo info)
        {
            try
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

                if(info.sdkList.Count > 0)
                {
                    OutPut("放入SDK接口 ");
                    PutSDKInterface(filePath);

                    for (int i = 0; i < info.sdkList.Count; i++)
                    {
                        OutPut("放入SDK " + info.sdkList[i].sdkName);
                        PutSDK(filePath,info.sdkList[i]);
                    }

                    OutPut("写配置清单");
                    SaveSDKManifest(filePath, info);

                    OutPut("整合权限");
                    PermissionLogic(filePath, info);
                }
            }
            catch(Exception e)
            {
                OutPut(e.ToString());
            }
        }

        public void OutPut(string content)
        {
            callBack?.Invoke(content);
        }

        #region 修改包名和appName

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
                    if (config.sdkType == item)
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
            //添加Jar
            PutJar(filePath, info);

            //添加资源文件


            //添加配置文件
            SaveSDKConfigFile(filePath, info);
        }

        void PutJar(string filePath, SDKInfo info)
        {
            string libPath = EditorData.SdkLibPath + "\\" + info.sdkName + "\\lib";

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
                if (ele.GetAttribute("name") == "android.permission." + permission)
                {
                    return;
                }
            }

            XmlElement nd = xmlDoc.CreateElement("uses-permission");
            nd.SetAttribute("name", "http://schemas.android.com/apk/res/android","android.permission." + permission);

            manifest.AppendChild(nd);
            xmlDoc.Save(xmlPath);
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

            CmdService cmd = new CmdService(OutPut);
            //Jar to dex
            cmd.Execute("java -jar dx.jar --dex --output=" + tempPath + " " + jarPath);

            //dex to smali
            cmd.Execute("java -jar baksmali-2.1.3.jar --o=" + smaliPath + " " + tempPath);

            //删除临时目录
            FileTool.DeleteDirectory(tempPath);
            Directory.Delete(tempPath);
        }

        #endregion
    }

}
