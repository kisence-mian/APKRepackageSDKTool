using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
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

                for (int i = 0; i < info.sdkList.Count; i++)
                {
                    OutPut("放入SDK " + info.sdkList[i].sdkName);
                    PutSDK(filePath, info.sdkList[i]);
                }

                OutPut("整合AndroidManifest.xml 清单文件");
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

        #region 替换图片

        void ChangeAppIcon(string filePath, string appIcon)
        {
            string exportPath = filePath + "\\res\\drawable-hdpi-v4\\app_icon.png";
            ExportImage(exportPath, appIcon, 72, 72);

            exportPath = filePath + "\\res\\drawable-ldpi-v4\\app_icon.png";
            ExportImage(exportPath, appIcon, 36, 36);

            exportPath = filePath + "\\res\\drawable-mdpi-v4\\app_icon.png";
            ExportImage(exportPath, appIcon, 48, 48);

            exportPath = filePath + "\\res\\drawable-xhdpi-v4\\app_icon.png";
            ExportImage(exportPath, appIcon, 96, 96);

            exportPath = filePath + "\\res\\drawable-xxhdpi-v4\\app_icon.png";
            ExportImage(exportPath, appIcon, 144, 144);

            exportPath = filePath + "\\res\\drawable-xxxhdpi-v4\\app_icon.png";
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

        void PutSDK(string filePath,SDKInfo info)
        {
            //添加String字段
            for (int i = 0; i < info.sdkConfig.Count; i++)
            {
                KeyValue kv = info.sdkConfig[i];
                AddStringConfig(filePath, info.sdkName + "_"+ kv.key, kv.value);
            }

            //添加SDKLib

            //调整权限

        }

        void AddStringConfig(string filePath,string key,string value)
        {
            string xmlPath = filePath + "\\res\\values\\strings.xml";
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode resources = xmlDoc.SelectSingleNode("resources");

            XmlElement nd = xmlDoc.CreateElement("name");
            nd.SetAttribute("string", key);
            nd.InnerText = value;

            resources.AppendChild(nd);

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
            XmlElement nodeEle = (XmlElement)manifest;

            XmlElement nd = xmlDoc.CreateElement("uses-permission");
            nd.SetAttribute("android:name", "android.permission." + permission);

            //nodeEle.SetAttribute("package", packageName);
            xmlDoc.Save(xmlPath);
        }

        #endregion
    }

}
