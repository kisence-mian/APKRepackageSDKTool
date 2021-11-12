using APKRepackageSDKTool;
using APKRepackageSDKTool.src;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

public class AndroidTool
{
    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;

    CompileTool cot;

    public AndroidTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;

        cot = new CompileTool(callBack, errorCallBack);
    }

    #region 生成R表

    public void BuildRTable(string aimPath, string name, string sdkPath)
    {
        hash = new Dictionary<string, Dictionary<string, RData>>();

        string R_Path = aimPath + "/R_path/";
        string manifest = aimPath + "/AndroidManifest.xml";
        string resPath = aimPath + "/res";

        if (!Directory.Exists(resPath))
        {
            //不存在res文件夹则不传入
            OutPut( "I: 无需生成R文件 " + name);
            return ;
        }

        //清理掉旧的R_Path
        if(Directory.Exists(R_Path))
        {
            FileTool.DeleteDirectoryComplete(R_Path);
        }
        FileTool.CreatePath(R_Path);

        //用直接生成的办法创建R文件
        string packageName = GetPackageName(manifest);
        string txtPath = aimPath + "\\R.txt";
        string javaPath = R_Path + "" + packageName.Replace(".","\\") +"\\R.java";

        RTxt2RJava(packageName, txtPath, javaPath);
        cot.GenerateRJar(R_Path, aimPath,  name,  sdkPath);

        FileTool.DeleteDirectoryComplete(R_Path);
    }

    public string GetPackageName(string manifest)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(manifest);

        XmlElement root = doc.DocumentElement;
        XmlElement ele = (XmlElement)root;

        return ele.GetAttribute("package");
    }



    Dictionary<string, Dictionary<string, RData>> hash = new Dictionary<string, Dictionary<string, RData>>();
    Dictionary<string, int> indexHash = new Dictionary<string, int>();

    public void RTxt2RJava(string packageName,string txtPath,string javaPath)
    {
        string content = FileTool.ReadStringByFile(txtPath);
        FileTool.CreateFilePath(javaPath);

        string[] lines = content.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            if(string.IsNullOrEmpty( lines[i]))
            {
                continue;
            }

            string[] line = lines[i].Split(' ');

            RData rData = new RData();
            rData.field = line[0];
            rData.type = line[1];
            rData.name = line[2];

            int index = 0;
            if(indexHash.ContainsKey(rData.type))
            {
                index = indexHash[rData.type]++;
            }
            else
            {
                indexHash.Add(rData.type, index);
            }

            rData.index = index;

            if (rData.field == "int")
            {
                rData.value = line[3];
                //rData.value = GenerateValue(rData);
            }
            else
            {
                int start = lines[i].IndexOf("{");
                rData.value = lines[i].Substring(start);
            }

            Dictionary<string, RData> map;
            if(!hash.ContainsKey(rData.type))
            {
                map = new Dictionary<string, RData>();
                hash.Add(rData.type, map);
            }
            else
            {
                map = hash[rData.type];
            }

            map.Add(rData.name, rData);

            //OutPut("type " + type + " name " + name + " value " + value);
        }

        string java = "package " + packageName + ";\n\n";
        java += "public class R\n";
        java += "{\n";

        foreach (var item in hash)
        {
            java += "\tpublic static class " + item.Key + "\n";
            java += "\t{\n";

            foreach (var map in item.Value)
            {
                if(map.Value.field == "int")
                {
                    java += "\t\tpublic static int " + map.Key + " = " + map.Value.value + ";\n";
                }
                else
                {
                    java += "\t\tpublic static " + map.Value.field+ " " + map.Key + " = " + map.Value.value + ";\n";
                }
            }
            java += "\t}\n\n";
        }

        java += "}";

        FileTool.WriteStringByFile(javaPath, java);

        OutPut("I: R.java 构造完成");
    }

    //string GenerateValue(RData rData)
    //{
    //    if(Convert.ToInt32(rData.value, 16) > 1000)
    //    {
    //        return rData.value;
    //    }

    //    string value = "";
    //    switch(rData.type)
    //    {
    //        case "drawable": value = "Ox7F02";break;
    //        case "layout": value = "Ox7F03"; break;
    //        case "values": value = "Ox7F04"; break;
    //        case "xml": value = "Ox7F05"; break;
    //        case "raw": value = "Ox7F06"; break;
    //        case "color": value = "Ox7F06"; break;
    //        case "meny": value = "Ox7F06"; break;

    //        default: value = "Ox7F04";
    //            OutPut("W: 找不到匹配的类型 " + rData.type);
    //            break;
    //    }

    //    value += rData.index.ToString("x4");

    //    return value;
    //}

    #endregion

    #region 提取资源到SDK目录

    //在 SdkEditorWindow  -> void ExtractAAR(string path) 1667 行

    #endregion

    #region 提取资源到APK打包目录

    /// <summary>
    /// 从AAR向APK提取资源（注意区分与从AAR向SDK库提取资源）
    /// </summary>
    /// <param name="rarPath"></param>
    /// <param name="aimPath"></param>
    /// <param name="info"></param>
    public void ExtractAAR2APK(string rarPath,string aimPath,ChannelInfo info)
    {
        cot.assignMinAPILevel = info.GetAssignMinAPILevel();
        cot.minAPILevel = info.GetMinAPILevel();

        string aarName = FileTool.GetFileNameByPath(rarPath);

        //提取jar
        ExtractJar2APK(rarPath, aimPath);

        if (File.Exists(rarPath + "/AndroidManifest.xml"))
        {
            //提取Manifest
            ExtractManifest2APK(rarPath, aimPath, info);
        }

        //复制jni
        if (Directory.Exists(rarPath + "/jni"))
        {
            FileTool.CopyDirectory(rarPath + "/jni", aimPath + "/lib", (pathA, pathB) => {

                OutPut("W: 重复的资源 " + pathA);
            });
        }

        //复制assets
        if (Directory.Exists(rarPath + "/assets"))
        {
            FileTool.CopyDirectory(rarPath + "/assets", aimPath + "/assets", (pathA, pathB) => {

                OutPut("W: 重复的资源 " + pathA);
            });
        }

        //合并res
        if (Directory.Exists(rarPath + "/res"))
        {
            MergeResTool mergeRes = new MergeResTool(callBack, errorCallBack);

            //合并Res
            mergeRes.Merge(rarPath + "/res", aimPath + "/res");
        }

        //生成R表
        BuildRTable2APKByCache(rarPath, aarName, aimPath);
    }

    void ExtractJar2APK(string sourcePath, string aimPath)
    {
        string[] paths = Directory.GetFiles(sourcePath);
        foreach (var item in paths)
        {
            if (item.EndsWith(".jar"))
            {
                cot.Jar2SmaliByCache(item, aimPath);
            }
        }

        string[] dires = Directory.GetDirectories(sourcePath);
        for (int i = 0; i < dires.Length; i++)
        {
            ExtractJar2APK(dires[i], aimPath);
        }
    }

    public void BuildRTable2APKByCache(string rarPath, string name, string aimPath)
    {
        string R_Path = rarPath + "/R_path/";

        string cachePath = rarPath + "\\.smaliCache\\" + name + "_R.jar";
        string smaliPath = aimPath + "\\smali";

        if(Directory.Exists(cachePath))
        {
            OutPut("读取R文件缓存 " + cachePath);
            FileTool.CopyDirectory(cachePath, smaliPath, (fileA, fileB) => { });
        }
        else
        {
            BuildRTable2APK(rarPath, name, aimPath);
        }
    }

    public void BuildRTable2APK(string rarPath, string name, string aimPath)
    {
        hash = new Dictionary<string, Dictionary<string, RData>>();

        string R_Path = rarPath + "/R_path/";
        string manifest = rarPath + "/AndroidManifest.xml";
        string resPath = rarPath + "/res";
        string txtPath = rarPath + "\\R.txt";

        if (string.IsNullOrEmpty(FileTool.ReadStringByFile(txtPath)))
        {
            //不存在res文件夹则不传入
            OutPut("I: 无需生成R文件 " + name);
            return;
        }

        //清理掉旧的R_Path
        if (Directory.Exists(R_Path))
        {
            FileTool.DeleteDirectoryComplete(R_Path);
        }
        FileTool.CreatePath(R_Path);

        //用直接生成的办法创建R文件
        string packageName = GetPackageName(manifest);
        
        string javaPath = R_Path + "" + packageName.Replace(".", "\\") + "\\R.java";

        RTxt2RJava(packageName, txtPath, javaPath);
        cot.GenerateRJar2APK(R_Path, rarPath, name, aimPath);

        FileTool.DeleteDirectoryComplete(R_Path);
    }


    void ExtractManifest2APK(string sourcePath, string aimPath,ChannelInfo info)
    {
        string sourceManifestPath = sourcePath + "/AndroidManifest.xml";

        XmlDocument doca = new XmlDocument();
        doca.Load(sourceManifestPath);

        XmlNode manifest = doca.SelectSingleNode("manifest");
        XmlNode application = manifest.SelectSingleNode("application");

        for (int i = 0; i < manifest.ChildNodes.Count; i++)
        {
            XmlNode node = manifest.ChildNodes[i];

            //跳过注释
            if (node.NodeType == XmlNodeType.Comment)
            {
                continue;
            }

            if(node.NodeType != XmlNodeType.Element)
            {
                OutPut("I: node.NodeType" + node.NodeType);
                return;
            }

            XmlElement ele = (XmlElement)node;

            //权限
            if (ele.Name == "uses-permission")
            {
                string permissionString = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android").Replace("android.permission.", "");
                OutPut("I: 添加 permission " + permissionString);

                permissionString = ReplaceKeyWordByChannelInfo(permissionString, info);
                permissionString = ReplaceKeyWordbySDKInfo(permissionString, null);

                AddPermission(aimPath, permissionString);
            }
            //meta以及其他
            else if (ele.Name == "meta" || ele.Name == "meta-data")
            {
                //Meta
                KeyValue metaKV = new KeyValue();
                metaKV.key = ele.OuterXml;
                metaKV.value = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "");

                AddMeta_Manifest(aimPath, metaKV, info,null);

                OutPut("I: 添加 meta " + metaKV.value);
            }

            //uses-feature
            else if (ele.Name == "uses-feature")
            {
                KeyValue metaKV = new KeyValue();
                metaKV.key = ele.OuterXml;
                metaKV.value = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "");

                AddUses(aimPath, metaKV, info, null);

                OutPut("I: 添加 meta " + metaKV.value);
            }

            //queries
            if (ele.Name == "queries")
            {
                KeyValue queriesKV = new KeyValue();
                queriesKV.key = ele.OuterXml;
                queriesKV.value = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "");

                AddQueries(aimPath, queriesKV, info, null);

                OutPut("I: 添加 queries " + queriesKV.value);
            }
        }

        if (application != null)
            for (int i = 0; i < application.ChildNodes.Count; i++)
            {
                XmlNode node = application.ChildNodes[i];

                //跳过注释
                if (node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                XmlElement ele = (XmlElement)node;

                //Activity
                if (ele.Name == "activity")
                {
                    string name = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                    if (!HasApplicationNode(aimPath, "activity", name))
                    {
                        ActivityInfo activityInfo = new ActivityInfo();

                        activityInfo.mainActivity = false;
                        activityInfo.name = name;
                        activityInfo.content = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "");

                        AddActivity(aimPath, activityInfo, null, info);

                        OutPut("I: 添加 activity " + name);
                    }
                    else
                    {
                        OutPut("I: 重复的 activity " + name);
                    }
                }

                //Service
                if (ele.Name == "service")
                {
                    string name = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                    if (!HasApplicationNode(aimPath, "service", name))
                    {
                        OutPut("I: 添加 service " + name);
                        ServiceInfo serviceInfo = new ServiceInfo();

                        serviceInfo.name = name;
                        serviceInfo.content = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "");

                        AddService(aimPath, serviceInfo, info, null);
                    }
                    else
                    {
                        OutPut("I: 重复的 service " + name);
                    }
                }

                if (ele.Name == "provider")
                {
                    string name = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                    if(!HasApplicationNode(aimPath, "provider", name))
                    {
                        OutPut("I: 添加 provider " + name);

                        ProviderInfo providerInfo = new ProviderInfo();
                        providerInfo.name = name;
                        providerInfo.content = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "").Replace("${ApplicationID}", "{PackageName}");

                        AddProvider(aimPath, providerInfo, info, null);
                    }
                    else
                    {
                        OutPut("I: 重复的 provider " + name);
                    }
                }

                //recevicer和provider的处理是一样的
                if (ele.Name == "receiver")
                {
                    string name = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                    if (!HasApplicationNode(aimPath, "recevicer", name))
                    {
                        OutPut("I: 添加 recevicer " + name);
                        ProviderInfo recevicerInfo = new ProviderInfo();

                        recevicerInfo.name = name;
                        recevicerInfo.content = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "").Replace("${ApplicationID}", "{PackageName}");

                        AddProvider(aimPath, recevicerInfo, info, null);
                    }
                    else
                    {
                        OutPut("I: 重复的 recevicer " + name);
                    }
                }

                if (ele.Name == "meta-data" || ele.Name == "meta")
                {
                    string name = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                    if (!HasApplicationNode(aimPath, "meta-data", name))
                    {
                        OutPut("I: 添加 meta-data " + name);

                        //Meta
                        KeyValue metaKV = new KeyValue();
                        metaKV.key = ele.OuterXml;
                        metaKV.value = ele.OuterXml.Replace("xmlns:android=\"http://schemas.android.com/apk/res/android\"", "");

                        AddMeta_Application(aimPath, metaKV, info, null);
                    }
                    else
                    {
                        OutPut("I: 重复的 recevicer " + name);
                    }
                }


            }
    }

    #endregion

    #region Manifest处理

    #region Manifest 判断

    /// <summary>
    /// 判断清单文件application节点下是否存在某个 Node
    /// </summary>
    /// <param name="aimPath"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    bool HasApplicationNode(string aimPath,string nodeName,string name)
    {
        bool has = false;

        string manifestPath = aimPath + "/AndroidManifest.xml";

        XmlDocument doca = new XmlDocument();
        doca.Load(manifestPath);

        //找到application 节点
        XmlNode manifest = doca.SelectSingleNode("manifest");
        XmlNode application = manifest.SelectSingleNode("application");

        //然后遍历
        if (application != null)
            for (int i = 0; i < application.ChildNodes.Count; i++)
            {
                XmlNode node = application.ChildNodes[i];

                //跳过注释
                if (node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                XmlElement ele = (XmlElement)node;

                if (ele.Name == nodeName)
                {
                    string tmp = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");

                    if(tmp == name)
                    {
                        return true;
                    }
                }
            }

        return has;
    }


    #endregion

    #region so文件

    public void ChangeExtractNativeLibs(string filePath)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";

        string xml = FileTool.ReadStringByFile(xmlPath);

        xml = xml.Replace("android:extractNativeLibs=\"false\"", "android:extractNativeLibs=\"true\"");

        //直接保存
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        xmlDoc.Save(xmlPath);
    }

    #endregion

    public void AddActivity(string filePath, ActivityInfo info, SDKInfo sdkInfo, ChannelInfo channelInfo)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";

        //移除旧MainActivity
        if (info.MainActivity)
        {
            RemoveOldMainActivity(filePath);
        }

        //替换关键字
        string newContent = ReplaceKeyWordByChannelInfo(info.content, channelInfo);
        newContent = ReplaceKeyWordbySDKInfo(newContent, sdkInfo);

        string xml = FileTool.ReadStringByFile(xmlPath);
        int index = xml.IndexOf("</application>");
        xml = xml.Insert(index, newContent);

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

    public void AddMainActivity(string filePath, ActivityInfo info)
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


    public void AddService(string filePath, ServiceInfo info, ChannelInfo channelInfo, SDKInfo SDKinfo)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        string content = ReplaceKeyWordByChannelInfo(info.content, channelInfo);
        content = ReplaceKeyWordbySDKInfo(content, SDKinfo);

        int index = xml.IndexOf("</application>");
        xml = xml.Insert(index, content);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        xmlDoc.Save(xmlPath);
    }

    public void AddProvider(string filePath, ProviderInfo info, ChannelInfo channelInfo, SDKInfo SDKinfo)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        int index = xml.IndexOf("</application>");

        string content = ReplaceKeyWordByChannelInfo(info.content, channelInfo);
        content = ReplaceKeyWordbySDKInfo(content, SDKinfo);

        xml = xml.Insert(index, content);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        xmlDoc.Save(xmlPath);
    }

    public void AddMeta_Application(string filePath, KeyValue kv, ChannelInfo channelInfo, SDKInfo info)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        int index = xml.IndexOf("</application>");

        //替换关键字和配置
        string content = ReplaceKeyWordByChannelInfo(kv.value, channelInfo);
        content = ReplaceKeyWordbySDKInfo(content, info);

        xml = xml.Insert(index, content);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        xmlDoc.Save(xmlPath);
    }

    public void AddMeta_Manifest(string filePath, KeyValue kv, ChannelInfo channelInfo, SDKInfo info)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        int index = xml.IndexOf("</manifest>");

        //替换关键字和配置
        string content = ReplaceKeyWordByChannelInfo(kv.value, channelInfo);
        content = ReplaceKeyWordbySDKInfo(content, info);

        xml = xml.Insert(index, content);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        xmlDoc.Save(xmlPath);
    }

    public void AddUses(string filePath, KeyValue kv, ChannelInfo channelInfo, SDKInfo info)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        int index = xml.IndexOf("</manifest>");

        //替换关键字和配置
        string content = ReplaceKeyWordByChannelInfo(kv.value, channelInfo);
        content = ReplaceKeyWordbySDKInfo(content, info);

        xml = xml.Insert(index, content);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        xmlDoc.Save(xmlPath);
    }

    public void AddQueries(string filePath, KeyValue kv, ChannelInfo channelInfo, SDKInfo info)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        int index = xml.IndexOf("</manifest>");

        //替换关键字和配置
        string content = ReplaceKeyWordByChannelInfo(kv.value, channelInfo);
        content = ReplaceKeyWordbySDKInfo(content, info);

        xml = xml.Insert(index, content);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        xmlDoc.Save(xmlPath);
    }

    public void RemoveOldMainActivity(string filePath)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        //直接保存
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);

        XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
        XmlNode app = GetNode(manifest, "application");

        //获取主Activity
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

                    if (ele2 != null
                        && ele2.GetAttribute("name", "http://schemas.android.com/apk/res/android") == "android.intent.category.LAUNCHER")
                    {
                        ele.RemoveChild(node2);
                        break;
                    }
                }
            }
        }

        xmlDoc.Save(xmlPath);
    }

    public void ChangeApplicationName(string filePath, string applicationName)
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

    public void AddMainActivityProperty(string filePath, KeyValue kv, SDKInfo sdkInfo, ChannelInfo channelInfo)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";

        string xml = FileTool.ReadStringByFile(xmlPath);
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);

        //替换关键字
        string newValue = ReplaceKeyWordByChannelInfo(kv.value, channelInfo);
        newValue = ReplaceKeyWordbySDKInfo(newValue, sdkInfo);

        XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
        XmlNode app = GetNode(manifest, "application");

        //获取主Activity
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

                    if (ele2 != null && ele2.GetAttribute("name", "http://schemas.android.com/apk/res/android") == "android.intent.category.LAUNCHER")
                    {
                        //增加属性
                        ele.SetAttribute(kv.key, "http://schemas.android.com/apk/res/android", newValue);

                        break;
                    }
                }
            }
        }

        //保存
        xmlDoc.Save(xmlPath);
    }

    public string GetMainActicityName(string filePath)
    {
        string mainActivityName = null;

        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        //直接保存
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);

        XmlNode manifest = xmlDoc.SelectSingleNode("manifest");
        XmlNode app = GetNode(manifest, "application");

        //获取主Activity
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

                    if (ele2 != null
                        && ele2.GetAttribute("name", "http://schemas.android.com/apk/res/android") == "android.intent.category.LAUNCHER")
                    {
                        mainActivityName = ele.GetAttribute("name", "http://schemas.android.com/apk/res/android");
                        break;
                    }
                }
            }
        }

        return mainActivityName;
    }

    public void RemoveErrordManifest(string filePath)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";

        string xml = FileTool.ReadStringByFile(xmlPath);

        xml = xml.Replace("android:compileSdkVersion=\"28\"", "");
        xml = xml.Replace("android:compileSdkVersionCodename=\"9\"", "");

        //直接保存
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        xmlDoc.Save(xmlPath);
    }

    public void AddXMLHead(string filePath, KeyValue info, SDKInfo sdkInfo, ChannelInfo channelInfo)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";

        //替换关键字
        string newContent = ReplaceKeyWordByChannelInfo(info.value, channelInfo);
        newContent = ReplaceKeyWordbySDKInfo(newContent, sdkInfo);

        string xml = FileTool.ReadStringByFile(xmlPath);
        int index = xml.IndexOf("<manifest") + 10;
        xml = xml.Insert(index, newContent + " "); //最后加一个空格

        //直接保存
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        xmlDoc.Save(xmlPath);
    }

    public void AddApplicationHead(string filePath, KeyValue info, SDKInfo sdkInfo, ChannelInfo channelInfo)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";

        //替换关键字
        string newContent = ReplaceKeyWordByChannelInfo(info.value, channelInfo);
        newContent = ReplaceKeyWordbySDKInfo(newContent, sdkInfo);

        string xml = FileTool.ReadStringByFile(xmlPath);

        if (!xml.Contains(newContent.Replace(" ", "")))
        {
            int index = xml.IndexOf("<application") + 13;
            xml = xml.Insert(index, newContent + " "); //最后加一个空格
        }

        //直接保存
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);
        xmlDoc.Save(xmlPath);
    }


    public void ChangeMainActity(string filePath)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        string mainName = GetMainActicityName(filePath);

        if (mainName != null)
        {
            xml = xml.Replace(mainName, "sdkInterface.activity.MainActivity");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            xmlDoc.Save(xmlPath);
        }
        else
        {
            ActivityInfo activityInfo = new ActivityInfo();
            activityInfo.mainActivity = true;
            activityInfo.content = "<activity android:name=\"sdkInterface.activity.MainActivity\"><intent-filter><action android:name=\"android.intent.action.MAIN\" /><category android:name=\"android.intent.category.LAUNCHER\" /></intent-filter><meta-data android:name=\"unityplayer.UnityActivity\" android:value=\"true\" /></activity>";

            AddActivity(filePath, activityInfo, null, null);
        }
    }

    public void ChangePackageName(string filePath, string packageName)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        packageName = RemoveSpecialCode(packageName);

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

        XmlNode resources = xmlDoc.SelectSingleNode("resources");

        //遍历String表，替换App_name
        for (int i = 0; i < resources.ChildNodes.Count; i++)
        {
            XmlNode node = resources.ChildNodes[i];
            XmlElement nodeEle = (XmlElement)node;
            if (node.Name == "string"
                && nodeEle.GetAttribute("name") == "app_name")
            {
                nodeEle.InnerText = appName;
                break;
            }
        }

        xmlDoc.Save(xmlPath);
    }

    public void ChangeAppNameByLanguage(string filePath, string language, string appName)
    {
        //判断文件是否存在
        string xmlPath = filePath + "\\res\\values-" + language + "\\strings-" + language + ".xml";
        //存在就直接改
        if (File.Exists(xmlPath))
        {
            string xml = FileTool.ReadStringByFile(xmlPath);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            XmlNode resources = xmlDoc.SelectSingleNode("resources");

            //遍历String表，替换App_name
            for (int i = 0; i < resources.ChildNodes.Count; i++)
            {
                XmlNode node = resources.ChildNodes[i];
                XmlElement nodeEle = (XmlElement)node;
                if (node.Name == "string"
                    && nodeEle.GetAttribute("name") == "app_name")
                {
                    nodeEle.InnerText = appName;
                    break;
                }
            }

            xmlDoc.Save(xmlPath);
        }
        //不存在就创建一份
        else
        {
            //创建路径
            FileTool.CreateFilePath(xmlPath);
            //写入文件
            string content = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<resources>\n    <string name=\"app_name\">appName</string>\n</resources>";
            content = content.Replace("appName", appName);
            FileTool.WriteStringByFile(xmlPath, content);
        }
    }

    /// <summary>
    /// 添加权限
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="Permission"></param>
    public void AddPermission(string filePath, string permission)
    {
        string xmlPath = filePath + "\\AndroidManifest.xml";
        string xml = FileTool.ReadStringByFile(xmlPath);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);

        XmlNode manifest = xmlDoc.SelectSingleNode("manifest");

        string permissionHead = "";
        string permissionContent = "";

        //支持自定义权限头
        string[] info = permission.Split('|');
        if (info.Length == 1)
        {
            permissionHead = "uses-permission";
            permissionContent = GeneratePermissionName(permission);
        }
        else
        {
            permissionHead = "uses-permission-sdk-" + info[1];
            permissionContent = GeneratePermissionName(info[0]);
        }

        //权限判重
        for (int i = 0; i < manifest.ChildNodes.Count; i++)
        {
            XmlNode node = manifest.ChildNodes[i];

            if (node.Name == permissionHead)
            {
                XmlElement ele = (XmlElement)node;
                if (ele.GetAttribute("name", "http://schemas.android.com/apk/res/android") == permissionContent)
                {
                    return;
                }
            }
        }

        XmlElement nd = xmlDoc.CreateElement(permissionHead);
        nd.SetAttribute("name", "http://schemas.android.com/apk/res/android", permissionContent);

        manifest.AppendChild(nd);
        xmlDoc.Save(xmlPath);
    }

    string GeneratePermissionName(string permission)
    {
        //如果有“.”则认为是自定义权限，全文输入，否则补全权限
        if (permission.Contains("."))
        {
            return permission;
        }
        else
        {
            return "android.permission." + permission;
        }
    }

    public void ChangeSDKVersion(string filePath, int minSDKVersion, int targetSDKVersion)
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

    public XmlNode GetNode(XmlNode parent, string name)
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

    #endregion

    #region 文本处理

    public static void ReplaceKeyWordByDiretory(string aimPath, ChannelInfo channelInfo, SDKInfo sdkInfo)
    {
        string[] allFileName = Directory.GetFiles(aimPath);
        foreach (var item in allFileName)
        {
            //只处理xml文件
            if (item.EndsWith(".xml"))
            {
                string content = FileTool.ReadStringByFile(item);

                content = ReplaceKeyWordByChannelInfo(content, channelInfo);
                content = ReplaceKeyWordbySDKInfo(content, sdkInfo);

                FileTool.WriteStringByFile(item, content);
            }
        }
    }

    public static string ReplaceKeyWordByChannelInfo(string oldContent, ChannelInfo channelInfo)
    {
        string result = oldContent;

        if (channelInfo != null)
        {
            result = result.Replace("{PackageName}", channelInfo.PackageName);
            result = result.Replace("${applicationId}", channelInfo.PackageName);
            //result = result.Replace("{applicationId}", channelInfo.PackageName);
        }

        return result;
    }



    public static string ReplaceKeyWordbySDKInfo(string oldContent, SDKInfo SDKinfo)
    {
        string result = oldContent;

        if (SDKinfo != null)
        {
            for (int i = 0; i < SDKinfo.sdkConfig.Count; i++)
            {
                result = result.Replace("{" + SDKinfo.sdkConfig[i].key + "}", SDKinfo.sdkConfig[i].value);
            }
        }

        return result;
    }

    public string RemoveSpecialCode(string oldContent)
    {
        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(oldContent)).Replace("?", "");

        //return HttpUtility.UrlEncode(oldContent, Encoding.UTF8);

        //Encoding utf8 = Encoding.UTF8;
        //String code = HttpUtility.UrlEncode(oldContent, utf8);
        //return HttpUtility.UrlDecode(code); 
    }

    #endregion

    #region 公共方法

    public void OutPut(string content)
    {
        callBack?.Invoke(content);
    }

    public void ErrorOutPut(string content)
    {
        errorCallBack?.Invoke(content);
    }
    #endregion

    #region 声明

    struct RData
    {
        public int index;
        public string field;
        public string type;
        public string name;
        public string value;
        //public string[] values;
    }
    #endregion
}
