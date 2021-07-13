using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

public class MavenTool
{
    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;

    string mavenCachePath;

    string aarPath;
    string rarPath;
    string jarPath;
    string pomPath;

    List<string> failDownloadList = new List<string>();
    List<string> successDownloadList = new List<string>();

    RarTool rt ;
    HttpTool ht;
    CompileTool ct;
    AndroidTool at;

    public MavenTool(string mavenCachePath, OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;

        this.mavenCachePath = mavenCachePath;

        //判断缓存路径正确性
        if (!PathTool.JudgeIsLegalPath(mavenCachePath))
        {
            throw new Exception("E: maven 缓存路径 不合法 " + mavenCachePath);
        }

        //创建子目录
        aarPath = mavenCachePath + @"\aar";
        rarPath = mavenCachePath + @"\rar";
        jarPath = mavenCachePath + @"\jar";
        pomPath = mavenCachePath + @"\pom";

        FileTool.CreatePath(aarPath);
        FileTool.CreatePath(rarPath);
        FileTool.CreatePath(jarPath);
        FileTool.CreatePath(pomPath);

        rt = new RarTool(callBack, errorCallBack);
        ht = new HttpTool(callBack, errorCallBack);
        ct = new CompileTool(callBack, errorCallBack);
        at = new AndroidTool(callBack, errorCallBack);
    }

    public void TestConnectMaven(List<string> mavenPathList)
    {
        HttpTool ht = new HttpTool(callBack, errorCallBack);

        for (int i = 0; i < mavenPathList.Count; i++)
        {
            OutPut("I: 测试 " + mavenPathList[i]);
            //构造http链接测试连通性
            ht.DoGetRequestSendData(mavenPathList[i]);
        }
    }

    public void DowmLoadMaven(List<string> mavenPathList, List<string> mavenLibList)
    {
        failDownloadList.Clear();

        for (int i = 0; i < mavenLibList.Count; i++)
        {
            DownLoadSingleMavenLib(mavenPathList, mavenLibList[i]);
        }

        OutPut("I: 下载完成");
        if (successDownloadList.Count > 0)
        {
            OutPut("I: 本次下载成功的列表");
            for (int i = 0; i < successDownloadList.Count; i++)
            {
                OutPut(successDownloadList[i]);
            }
        }
        if (failDownloadList.Count > 0)
        {
            OutPut("I: 下载失败的列表 ");
            for (int i = 0; i < failDownloadList.Count; i++)
            {
                OutPut(failDownloadList[i]);
            }
        }
    }

    /// <summary>
    /// 根据Maven的设置
    /// </summary>
    /// <param name="mavenLibList"></param>
    /// <param name="aimPath"></param>
    public void ExtractMavenFile(List<string> mavenPathList, List<string> mavenLibList,string aimPath, ChannelInfo info)
    {
        //指定编译版本
        ct.assignMinAPILevel = info.GetAssignMinAPILevel();
        ct.minAPILevel = info.GetMinAPILevel();

        //先根据依赖收集一份maven清单
        //主要是为了版本号唯一
        List<string> mavenFinalList = CollectMavenList(mavenLibList);

        Dictionary<string, bool> repeatHash = new Dictionary<string, bool>();
        //进行提取
        //这里考虑进行多线程优化，提高速度
        for (int i = 0; i < mavenFinalList.Count; i++)
        {
            ExtractSinglePackage(mavenFinalList[i], aimPath, repeatHash,info);
        }
    }

    List<string> CollectMavenList(List<string> mavenLibList)
    {
        List<string> fList = new List<string>();
        List<MavenData> mavenDataList = new List<MavenData>();

        List<MavenData> repeatList = new List<MavenData>();
        List<MavenData> finalList = new List<MavenData>();

        //构造MavenData
        for (int i = 0; i < mavenLibList.Count; i++)
        {
            mavenDataList.Add(MavenData.GetMavenData(mavenLibList[i]));
        }

        //进行依赖收集
        for (int i = 0; i < mavenDataList.Count; i++)
        {
            CollectSingleMaven(mavenDataList[i], finalList, repeatList);
        }

        OutPut("最终放入的maven清单：");
        for (int i = 0; i < finalList.Count; i++)
        {
            OutPut(finalList[i].GetFullName());
            //输出结果
            fList.Add(finalList[i].GetFullName());
        }
        OutPut("抛弃的maven清单：");
        for (int i = 0; i < repeatList.Count; i++)
        {
            OutPut(repeatList[i].GetFullName());
        }

        return fList;
    }

    void CollectSingleMaven(MavenData maven, List<MavenData> finalList, List<MavenData> repeatList)
    {
        //对自身进行排重
        MavenRemoveRepeat(maven , finalList, repeatList);

        //处理依赖
        string libPomPath = pomPath + "\\" + maven.GetFileName() + ".pom";
        var list = AnalysisPomDependencies(libPomPath);
        for (int i = 0; i < list.Count; i++)
        {
            CollectSingleMaven(MavenData.GetMavenData(list[i]), finalList,repeatList);
        }
    }

    bool MavenRemoveRepeat(MavenData data, List<MavenData> finalList, List<MavenData> repeatList)
    {
        bool isRepeat = false;
        for (int i = 0; i < finalList.Count; i++)
        {
            //只取版本最大的
            if(finalList[i].GetGroupAddArtifact() == data.GetGroupAddArtifact())
            {
                isRepeat = true;

                if(finalList[i].VersionCompare(data) < 0)
                {
                    finalList[i] = data;

                    //收集重复结果
                    if(!GetHasMaven(repeatList,finalList[i]))
                    {
                        repeatList.Add(finalList[i]);
                    }
                }
                else if(finalList[i].VersionCompare(data) > 0)
                {
                    //收集重复结果
                    if (!GetHasMaven(repeatList,data))
                    {
                        repeatList.Add(data);
                    }
                }
            }
        }

        if(!isRepeat)
        {
            finalList.Add(data);
        }

        return isRepeat;
    }

    bool GetHasMaven(List<MavenData> repeatList,MavenData maven)
    {
        for (int i = 0; i < repeatList.Count; i++)
        {
            if(repeatList[i].GetFullName() == maven.GetFullName())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 下载一个库
    /// </summary>
    /// <returns>下载结果,下载成功返回true</returns>
    bool DownLoadSingleMavenLib(List<string> mavenPathList, string mavenLibName)
    {
        OutPut("I: 开始尝试下载 " + mavenLibName);

        bool isFindMaven = false;
        string finalMaven ="";
       
        string urlpath = GradleName2url(mavenLibName);
        string fileName = GradleName2fileName(mavenLibName);
        string libPomPath = pomPath + "\\" + fileName + ".pom";

        if(string.IsNullOrEmpty(urlpath))
        {
            ErrorOutPut("E: 格式异常 终止下载");
            return false;
        }

        if (File.Exists(libPomPath))
        {
            OutPut("I: " + mavenLibName + " 已经存在本地Maven库中");
            return true;
        }

        //寻找可用的Maven
        for (int i = 0; i < mavenPathList.Count; i++)
        {
            //构造url进行下载
            string url = mavenPathList[i] + "\\" + urlpath + ".pom";
           
            if (ht.HttpDownload(url, libPomPath))
            {
                isFindMaven = true;
                finalMaven = mavenPathList[i];

                break;
            }
        }

        if(isFindMaven)
        {
            //解析pom 下载依赖
            //OutPut("解析依赖 " + mavenLibName);
            var list =  AnalysisPomDependencies(libPomPath);
            string fileType = AnalysisPomByFileType(libPomPath);

            //将该maven置顶，提高查询效率
            mavenPathList.Remove(finalMaven);
            mavenPathList.Insert(0, finalMaven);

            bool isAll = true;
            //加载依赖
            for (int i = 0; i < list.Count; i++)
            {
                isAll &=  DownLoadSingleMavenLib(mavenPathList, list[i]);
            }

            bool isDownload = false;

            if (fileType == "aar")
            {
                isDownload = DownLoadAar(finalMaven, urlpath, fileName);

            }
            else if(fileType == "jar")
            {
                isDownload = DownLoadJar(finalMaven, urlpath, fileName);
            }
            else
            {
                ErrorOutPut("E: 意外的文件类型 " + fileType + " " + mavenLibName);
            }

            if (isDownload)
            {
                if (!isAll)
                {
                    OutPut("E: 没有完整下载全部依赖 " + mavenLibName);
                    //删除pom
                    File.Delete(libPomPath);
                    AddFailList(mavenLibName + " (没有完整下载全部依赖)");
                    return false;
                }
                else
                {
                    AddSuccessList(mavenLibName);
                    return true;
                }
            }
            else
            {
                OutPut("E: " + fileType + " 下载失败 " + mavenLibName);
                //删除pom
                File.Delete(libPomPath);
                AddFailList(mavenLibName + " (下载失败)");
                return false;
            }

        }
        else
        {
            ErrorOutPut("E: " + "没有含有 " + mavenLibName + " 的 Maven");
            AddFailList(mavenLibName + " (没有找到合适的Maven)");
            return false;
        }
    }

    void AddFailList(string mavenLibName)
    {
        if(!failDownloadList.Contains(mavenLibName))
        {
            failDownloadList.Add(mavenLibName);
        }
    }

    void AddSuccessList(string mavenLibName)
    {
        if (!successDownloadList.Contains(mavenLibName))
        {
            successDownloadList.Add(mavenLibName);
        }
    }

    bool DownLoadAar(string mavenPath,string urlPath,string fileName)
    {
        //下载aar
        string aarUrl = mavenPath + "\\" + urlPath + ".aar";
        string aarFileName = aarPath + "\\" + fileName + ".aar";

        if (ht.HttpDownload(aarUrl, aarFileName))
        {
            //OutPut("AAR 下载完成 " + mavenLibName + " maven " + finalMaven);

            string rarFileName = rarPath + "\\" + fileName + ".rar";

            //转储成rar并解压
            DumpRarAndDecompression(aarFileName, rarFileName);
            return true;
        }
        else
        {
            return false;
        }
    }

    bool DownLoadJar(string mavenPath, string urlPath, string fileName)
    {
        //下载aar
        string jarUrl = mavenPath + "\\" + urlPath + ".jar";
        string jarFileName = jarPath + "\\" + fileName + ".jar";

        if (ht.HttpDownload(jarUrl, jarFileName))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 解析依赖节点
    /// </summary>
    /// <param name="pomPath"></param>
    /// <returns></returns>
    List<string> AnalysisPomDependencies (string pomPath)
    {
        List<string> result = new List<string>();

        XmlDocument doc = new XmlDocument();
        doc.Load(pomPath);

        XmlElement root = doc.DocumentElement;

        string version = "noFind";

        for (int i = 0; i < root.ChildNodes.Count; i++)
        {
            XmlNode tmp = root.ChildNodes[i];
            //OutPut("tmp.Name " + tmp.Name);

            if (tmp.Name == "parent")
            {
                for (int j = 0; j < tmp.ChildNodes.Count; j++)
                {
                    if (tmp.ChildNodes[j].Name == "version")
                    {
                        version = tmp.ChildNodes[j].InnerXml;
                    }
                }
            }

            if (tmp.Name == "dependencies")
            {
                if(tmp.HasChildNodes)
                {
                    //解析单个 依赖节点
                    for (int j = 0; j < tmp.ChildNodes.Count; j++)
                    {
                        if(IsDependenciesNode(tmp.ChildNodes[j]))
                        {
                            result.Add(AnalysisSingleDependenciesXML(tmp.ChildNodes[j], version));
                        }
                    }
                }
            }
        }
        return result;
    }

    string AnalysisPomByFileType(string pomPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(pomPath);

        XmlElement root = doc.DocumentElement;

        for (int i = 0; i < root.ChildNodes.Count; i++)
        {
            XmlNode tmp = root.ChildNodes[i];
            if (tmp.Name == "packaging")
            {
                return tmp.InnerXml;
            }
        }

        //默认当做jar处理
        return "jar";
    }

    bool IsDependenciesNode(XmlNode node)
    {
        bool isHasScope = false;

        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode xml = node.ChildNodes[i];
            if (xml.Name == "scope")
            {
                isHasScope = true;

                if (xml.InnerXml == "test"
                    || xml.InnerXml == "provided"
                    || xml.InnerXml == "system")
                {
                    return false;
                }
            }

            if(xml.Name == "optional" && xml.InnerXml == "true")
            {
                return false;
            }
        }

        return isHasScope;
    }

    string AnalysisSingleDependenciesXML(XmlNode node,string parentVersion)
    {
        //OutPut("AnalysisSingleDependenciesXML node.Name " + node.Name);

        string groupId = "";
        string artifactId = "";
        string version = null;

        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode xml = node.ChildNodes[i];
            if (xml.Name == "groupId")
            {
                groupId = xml.InnerXml;
            }
            else if (xml.Name == "artifactId")
            {
                artifactId = xml.InnerXml;
            }
            else if (xml.Name == "version")
            {
                version = VersionLogic(xml.InnerXml);
            }
        }

        //未指定版本则使用父节点版本
        if(string.IsNullOrEmpty(version))
        {
            version = parentVersion;
        }

        return groupId + ":" + artifactId + ":" + version;
    }

    string VersionLogic(string version)
    {
        //直接去掉左右方括号
        //[19.7.0]  -> 19.7.0
        if (version.Contains("[") && version.Contains("]"))
        {
            version = version.Replace("[", "").Replace("]", "");
        }

        //[5,6)

        //5.+ 



        return version;
    }

    string GradleName2fileName(string gradleName)
    {
        try
        {
            //androidx.appcompat:appcompat:1.1.0 -> androidx.appcompat.appcompat-1.1.0
            string[] spilt = gradleName.Split(':');

            return spilt[0] + "." + spilt[1] + "-" + spilt[2];
        }
        catch (Exception)
        {
            ErrorOutPut("E: Gradle 格式异常 " + gradleName);
            return null;
        }
    }

    string GradleName2url(string gradleName)
    {
        try
        {
            // androidx.appcompat:appcompat:1.1.0 -> androidx/appcompat/appcompat/1.2.0/appcompat-1.2.0
            string[] spilt = gradleName.Split(':');

            return spilt[0].Replace(".", "/") + "/" + spilt[1] + "/" + spilt[2] + "/" + spilt[1] + "-" + spilt[2];
        }
        catch(Exception )
        {
            ErrorOutPut("E: Gradle 格式异常 " + gradleName);
            return null;
        }
    }

    void DumpRarAndDecompression(string aarFileName, string rarFileName)
    {
        if(!File.Exists(rarFileName))
        {
            //转储RAR
            File.Copy(aarFileName, rarFileName);
        }

        rt.Decompression(rarFileName);
    }

    void ExtractSinglePackage(string gradleName,string aimPath, Dictionary<string, bool> repeatHash, ChannelInfo info)
    {
        string aimPomPath = pomPath + "\\" + GradleName2fileName(gradleName) + ".pom";
        string fileType = AnalysisPomByFileType(aimPomPath);

        //避免重复提取
        if(repeatHash.ContainsKey(gradleName))
        {
            return;
        }
        else
        {
            repeatHash.Add(gradleName,true);
        }

        OutPut("I: 提取Maven  " + gradleName);

        ////读取依赖
        //var list = AnalysisPomDependencies(aimPomPath);

        //for (int i = 0; i < list.Count; i++)
        //{
        //    ExtractSinglePackage(list[i], aimPath, repeatHash, info);
        //}

        if (fileType == "aar")
        {
            string aimRarPath = rarPath + "\\" + GradleName2fileName(gradleName);
            string aimRarFile = aimRarPath + "\\.rar";

            //如果没有解压就现场解压
            if (!Directory.Exists(aimRarPath))
            {
                if(File.Exists(aimRarFile))
                {
                    rt.Decompression(aimRarFile);
                }
                else
                {
                    ErrorOutPut("E: 找不到资源 " + gradleName + " " + aimRarPath);
                    return;
                }
            }

            //提取本体
            at.ExtractAAR2APK(aimRarPath, aimPath,info);
        }
        else if(fileType == "jar")
        {
            string aimJarPath = jarPath + "\\" + GradleName2fileName(gradleName) +".jar";
            if(!File.Exists(aimJarPath))
            {
                ErrorOutPut("E: 找不到资源 " + gradleName + " " + aimJarPath);
                return;
            }

            ct.Jar2SmaliByCache(aimJarPath, aimPath);
        }
        else
        {
            ErrorOutPut("E: 意外的文件类型 " + fileType +" " + gradleName);
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

    struct MavenData
    {
        public string group;
        public string artifact;
        public string version;

        List<int> versions;

        public static MavenData GetMavenData(string gradleName)
        {
            //androidx.appcompat:appcompat:1.1.0 
            MavenData mavenData = new MavenData();

            string[] spilt = gradleName.Split(':');
            mavenData.group = spilt[0];
            mavenData.artifact = spilt[1];
            mavenData.version = spilt[2];

            mavenData.versions = new List<int>();
            string[] vs = mavenData.version.Split('.');

            for (int i = 0; i < vs.Length; i++)
            {
                mavenData.versions.Add(int.Parse(vs[i]));
            }

            return mavenData;
        }

        public string GetGroupAddArtifact()
        {
            return group + ":" + artifact;
        }

        public string GetFullName()
        {
            return group + ":" + artifact+":"+version;
        }

        public string GetFileName()
        {
            return group + "." + artifact + "-" + version;
        }

        public int VersionCompare(MavenData otherMaven)
        {
            for (int i = 0; i < versions.Count; i++)
            {
                if(otherMaven.versions.Count < i)
                {
                    return 1;
                }

                if(versions[i] > otherMaven.versions[i])
                {
                    return 1;
                }
                else if(versions[i] < otherMaven.versions[i])
                {
                    return -1;
                }

                //如果相等则判断下一位
            }

            if(otherMaven.versions.Count > versions.Count)
            {
                return -1;
            }

            return 0;
        }
    }
}
