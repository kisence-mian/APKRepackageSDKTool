using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static APKRepackageSDKTool.SDKConfig;

namespace APKRepackageSDKTool
{
    public static class EditorData
    {
        static Deserializer des = new Deserializer();

        public const string c_ConfigRecord = "Config";
        public const string c_GameRecord = "Games";

        private static TotalGameData gameList;

        static ChannelList currentGameChannelList;
        static ChannelInfo currentChannel;

        static TotalSDKConfig totalSDKConfig;
        static SDKConfig currentSDKConfig;

        static string sdkLibPath;
        static string androidSdkPath;

        static string mavenCachePath = "";

        static string jetifierPath;
        static string apktoolVersion = "apktool_2.6.0";
        static string baksmaliVersion = "baksmali-2.4.0.jar";

        static string buildToolVersion;
        static int apiLevel;

        static string RARdocompressCmd = "360zip.exe -x {RarPath} {AimPath}";
        static string RARcompressCmd = "360zip.exe -ar {FilePath} {ZipPath}";

        public static bool isTimeStamp = false;      //时间戳
        public static bool isOutPutCMD = false;      //输出原始命令
        public static bool isAutoInstall = false;      //自动安装

        #region 属性

        public static TotalGameData GameList
        {
            get
            {
                JudgeInit();
                return gameList;
            }
            set
            {
                gameList = value;
                gameList.Change();


                //不能直接保存TotalGameData 这里曲线救国了
                List<GameInfo> list = new List<GameInfo>();
                foreach (var item in gameList)
                {
                    list.Add(item);
                }

                RecordManager.SaveRecord(c_GameRecord, "gameList", list);
            }
        }

        public static ChannelList CurrentGameChannelList
        {
            get => currentGameChannelList;
            set
            {
                currentGameChannelList = value;
                currentGameChannelList.Change();
                SaveChannelInfo(currentGameChannelList.gameName, currentGameChannelList);
            }
        }

        public static ChannelInfo CurrentChannel
        {
            get => currentChannel;
            set
            {
                currentChannel = value;
                currentChannel.Change();
            }
        }

        public static TotalSDKConfig TotalSDKInfo
        {
            get
            {
                if(totalSDKConfig == null)
                {
                    UpdateTotalSDKInfo();
                }
                return totalSDKConfig;
            }

            set
            {
                totalSDKConfig = value;
                if(totalSDKConfig != null)
                {
                    totalSDKConfig.Change();
                }
            }
        }

        public static string SdkLibPath
        {
            get {
                JudgeInit();
                return sdkLibPath;
            }

            set
            {
                sdkLibPath = value;
                RecordManager.SaveRecord(c_ConfigRecord, "SDKLibPath", sdkLibPath);
            }
        }

        public static SDKConfig CurrentSDKConfig
        {
            get => currentSDKConfig;

            set
            {
                currentSDKConfig = value;
                if(currentSDKConfig != null)
                {
                    currentSDKConfig.Change();
                }
            }
        }

        public static string AndroidSdkPath {
            get
            {
                JudgeInit();
                return androidSdkPath;
            }

            set
            {
                androidSdkPath = value;
                RecordManager.SaveRecord(c_ConfigRecord, "AndroidSDKPath", androidSdkPath);
            }
        }

        public static string JetifierPath {

            get
            {
                JudgeInit();
                return jetifierPath;
            }

            set
            {
                jetifierPath = value;
                RecordManager.SaveRecord(c_ConfigRecord, "JetifierPath", jetifierPath);
            }
        }

        public static string BuildToolVersion 
        { 
            get
            {
                JudgeInit();
                return buildToolVersion;
            }

            set
            {
                buildToolVersion = value;
                RecordManager.SaveRecord(c_ConfigRecord, "BuildToolVersion", buildToolVersion);
            }
        }

        public static bool IsTimeStamp
        {
            get
            {
                JudgeInit();
                return isTimeStamp;
            }

            set
            {
                isTimeStamp = value;
                RecordManager.SaveRecord(c_ConfigRecord, "IsTimeStamp", isTimeStamp);
            }

        }

        public static bool IsOutPutCMD 
        { 
            get
            {
                JudgeInit();
                return isOutPutCMD;
            }

            set
            {
                isOutPutCMD = value;
                RecordManager.SaveRecord(c_ConfigRecord, "IsOutPutCMD", isOutPutCMD);
            }

        }

        public static bool IsAutoInstall 
        { 
            get
            {
                JudgeInit();
                return isAutoInstall;
            }

            set
            {
                isAutoInstall = value;
                RecordManager.SaveRecord(c_ConfigRecord, "IsAutoInstall", isAutoInstall);
            }
        }

        public static int APILevel {

            get
            {
                JudgeInit();
                return apiLevel;
            }

            set
            {
                apiLevel = value;
                RecordManager.SaveRecord(c_ConfigRecord, "APILevel", apiLevel);
            }
        }

        public static string ApktoolVersion {

            get
            {
                JudgeInit();
                return apktoolVersion;
            }

            set
            {
                apktoolVersion = value;
                RecordManager.SaveRecord(c_ConfigRecord, "ApktoolVersion", apktoolVersion);
            }

        }

        public static string BaksmaliVersion {
            get
            {
                JudgeInit();
                return baksmaliVersion;
            }

            set
            {
                baksmaliVersion = value;
                RecordManager.SaveRecord(c_ConfigRecord, "BaksmaliVersion", baksmaliVersion);
            }
        }
        public static string MavenCachePath
        {
            get
            {
                JudgeInit();
                return mavenCachePath;
            }

            set
            {
                mavenCachePath = value;
                RecordManager.SaveRecord(c_ConfigRecord, "MavenCachePath", mavenCachePath);
            }
        }

        public static string _RARdocompressCmd
        {
            get
            {
                JudgeInit();
                return RARdocompressCmd;
            }

            set
            {
                RARdocompressCmd = value;
                RecordManager.SaveRecord(c_ConfigRecord, "RARdocompressCmd", RARdocompressCmd);
            }
        }

        public static string _RARcompressCmd
        {
            get
            {
                JudgeInit();
                return RARcompressCmd;
            }

            set
            {
                RARcompressCmd = value;
                RecordManager.SaveRecord(c_ConfigRecord, "RARcompressCmd", RARcompressCmd);
            }
        }



        #endregion

        #region 初始化
        static bool isInit = false;
        static void JudgeInit()
        {
            if(!isInit)
            {
                isInit = true;

                //不能直接保存TotalGameData 这里曲线救国了
                var list = RecordManager.GetRecord(c_GameRecord, "gameList", new List<GameInfo>());

                gameList = new TotalGameData();
                foreach (var item in list)
                {
                    gameList.Add(item);
                }

                sdkLibPath = RecordManager.GetRecord(c_ConfigRecord, "SDKLibPath", null);
                androidSdkPath = RecordManager.GetRecord(c_ConfigRecord, "AndroidSDKPath", null);
                jetifierPath = RecordManager.GetRecord(c_ConfigRecord, "JetifierPath", null);
                buildToolVersion = RecordManager.GetRecord(c_ConfigRecord, "BuildToolVersion", null);
                apiLevel = RecordManager.GetRecord(c_ConfigRecord, "APILevel", 29);

                isOutPutCMD = RecordManager.GetRecord(c_ConfigRecord, "IsOutPutCMD", false);
                isAutoInstall = RecordManager.GetRecord(c_ConfigRecord, "IsAutoInstall", false);
                isTimeStamp = RecordManager.GetRecord(c_ConfigRecord, "IsTimeStamp", false);

                baksmaliVersion = RecordManager.GetRecord(c_ConfigRecord, "BaksmaliVersion", "baksmali-2.1.3.jar");
                apktoolVersion = RecordManager.GetRecord(c_ConfigRecord, "ApktoolVersion", "apktool_2.5.0");
                mavenCachePath = RecordManager.GetRecord(c_ConfigRecord, "MavenCachePath", null);
                RARdocompressCmd = RecordManager.GetRecord(c_ConfigRecord, "RARdocompressCmd", "360zip.exe -x {RarPath} {AimPath}");
                RARcompressCmd = RecordManager.GetRecord(c_ConfigRecord, "RARcompressCmd", "360zip.exe -ar {FilePath} {ZipPath}");

                UpdateTotalSDKInfo();
            }
        }

        #endregion

        #region APK

        public static string GetAPKPath(string gameName)
        {
            return RecordManager.GetRecord(c_GameRecord, gameName+ "_APKPath", "");
        }

        public static void SaveAPKPath(string gameName,string apkPath)
        {
            RecordManager.SaveRecord(c_GameRecord, gameName + "_APKPath", apkPath);
        }

        public static string GetExportAPKPath(string gameName)
        {
            return RecordManager.GetRecord(c_GameRecord, gameName + "_ExportAPKPath", "");
        }

        public static void SaveExportAPKPath(string gameName, string apkPath)
        {
            RecordManager.SaveRecord(c_GameRecord, gameName + "_ExportAPKPath", apkPath);
        }

        #endregion

        #region ChannelList

        public static void SetChannelList(string gameName)
        {
            currentGameChannelList = GetChannelInfo(gameName);
        }

        public static ChannelList GetChannelInfo(string GameName)
        {
            List<ChannelInfo> list = RecordManager.GetRecord(c_GameRecord, GameName, new List<ChannelInfo>());

            //ChannelInfo ci3 = new ChannelInfo();
            //ci3.Name = "GooglePlay";

            //list.Add(ci3);

            ChannelList info = new ChannelList();
            info.gameName = GameName;
            foreach (var item in list)
            {
                info.Add(item);
            }

            return info;
        }

        public static void SaveChannelInfo(string GameName, ChannelList channel)
        {
            List<ChannelInfo> list = new List<ChannelInfo>();

            ChannelList info = new ChannelList();
            foreach (var item in channel)
            {
                list.Add(item);
            }

            RecordManager.SaveRecord(c_GameRecord, GameName, list);
        }

        #endregion

        #region CurrentChannel

        public static void SetCurrentChannel(string channelName)
        {
            for (int i = 0; i < CurrentGameChannelList.Count; i++)
            {
                ChannelInfo info = CurrentGameChannelList[i];

                if (info.Name == channelName)
                {
                    CurrentChannel = info;
                    return;
                }
            }
        }

        #endregion

        #region SDKConfig

        public static void UpdateTotalSDKInfo()
        {
            try
            {
                //遍历SDKPath下的全部文件夹，排除Config文件夹
                if (!string.IsNullOrEmpty(SdkLibPath))
                {
                    if (totalSDKConfig == null)
                    {
                        totalSDKConfig = new TotalSDKConfig();
                    }
                    else
                    {
                        totalSDKConfig.Clear();
                    }

                    string[] dires = Directory.GetDirectories(SdkLibPath);
                    for (int i = 0; i < dires.Length; i++)
                    {
                        if (!dires[i].Contains("Interface")
                            && !dires[i].Contains("git")
                             && !dires[i].Contains("svn")
                            && !dires[i].Contains("Source")
                            )
                        {
                            //读取每个文件夹中的config.json，以获取对应的设置要求
                            string configPath = dires[i] + "\\config.json";

                            SDKConfig info = LoadSDKConfig(FileTool.GetDirectoryName(dires[i]), configPath);

                            totalSDKConfig.Add(info);
                        }
                    }

                    TotalSDKInfo = totalSDKConfig;
                }
                else
                {
                    totalSDKConfig = null;
                }
            }catch(Exception)
            {
                MessageBox.Show("读取SDKLibrary出错，请确保配置了正确的SDKLibrary路径 ：" + SdkLibPath);
            }
        }

        static SDKConfig LoadSDKConfig(string configName,string path)
        {
            string content = FileTool.ReadStringByFile(path);

            if(content == null || content == "")
            {
                SDKConfig c = new SDKConfig();
                c.SdkName = configName;
                return c;
            }

            SDKConfig config = des.Deserialize<SDKConfig>(content);

            return config;
        }

        public static void SaveSDKConfig(SDKConfig config)
        {
            string path = SdkLibPath + "\\" + config.SdkName + "\\config.json";

            string content = Serializer.Serialize(config);
            FileTool.WriteStringByFile(path, content);
        }

        public static void SetCurrentSDKConfig(string sdkName)
        {
            for (int i = 0; i < totalSDKConfig.Count; i++)
            {
                if(totalSDKConfig[i].sdkName == sdkName)
                {
                    CurrentSDKConfig = totalSDKConfig[i];
                }
            }
        }
        #endregion

        #region AndroidSDK

        public static string GetAndroidJarPath(int apiLevel)
        {
            return androidSdkPath + "\\platforms\\android-" + apiLevel + "\\android.jar";
        }

        /// <summary>
        /// 获取android sdk dxjar 的路径
        /// </summary>
        /// <returns></returns>
        public static string GetDxPath()
        {
            return androidSdkPath + "\\build-tools\\"+ buildToolVersion + "\\lib\\dx.jar";
        }

        /// <summary>
        /// 获取android sdk d8jar 的路径
        /// </summary>
        /// <returns></returns>
        public static string GetD8Path()
        {
            return androidSdkPath + "\\build-tools\\" + buildToolVersion + "\\d8.bat";
        }

        /// <summary>
        /// 获取android sdk apksignerjar 的路径
        /// </summary>
        /// <returns></returns>
        public static string GetApksignerPath()
        {
            return androidSdkPath + "\\build-tools\\" + buildToolVersion + "\\lib\\apksigner.jar";
        }

        public static string GetAAPTPath()
        {
            return androidSdkPath + "\\build-tools\\" + buildToolVersion + "\\aapt.exe";
        }

        public static string GetAAPT2Path()
        {
            return androidSdkPath + "\\build-tools\\" + buildToolVersion + "\\aapt2.exe";
        }

        public static string GetZipalignPath()
        {
            return androidSdkPath + "\\build-tools\\" + buildToolVersion + "\\zipalign.exe";
        }

        public static string GetBaksmaliPath()
        {
            return BaksmaliVersion;
        }


        

        #endregion
    }

    #region 声明

    public class TotalGameData : List<GameInfo>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Change()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged(this, e);
        }
    }

    public class GameInfo
    {
        public string gameName;

        public string GameName { get => gameName; set => gameName = value; }
    }

    public class ChannelList : List<ChannelInfo>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public string gameName;

        public void Change()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged(this, e);
        }
    }

    /// <summary>
    /// 储存渠道所需的数据
    /// </summary>
    public class ChannelInfo : INotifyCollectionChanged
    {
        public string channelName; //渠道名
        public string suffix; //渠道名

        public bool isChecked; //是否被选中

        public bool useV2Sign;      //使用v2版的签名
        public string keyStorePath; //KeyStore的路径
        public string keyStorePassWord; //KeyStore 密码

        public string jksPath; //jks文件的路径

        public string keyStoreAlias;         //别名
        public string keyStoreAliasPassWord; //别名密码

        public string packageName; //包名
        public string appName;
        public List<KeyValue> appNameLanguages = new List<KeyValue>();
        public string appIcon;
        public string appBanner;

        public bool isSplitDex = true; //分包
        public bool isResplitDex = true; //重新分包
        public int splitDexOffset = 0;   //分包补偿

        public bool isRebuildRTable = true; //重新生成R表
        public bool isForceManifest = false; //强制反编译清单文件(反编译Resource文件)
        public bool isOnlyMainClasses = false; //只反编译主Classes文件(解决混淆问题)
        public bool isNoCompressResource = false; //不压缩Resource文件
        public bool isUseAAPT2 = true; //使用AAPT2
        public bool isChangeMainApplication = false; //修改ManinApplication
        public bool isChangeMainActivity = true; //修改ManinActivity
        public bool isExecuteInvalidFile = true; //处理无效文件
        public bool isLog;  //输出日志
        public bool isZipalign = true; //字节对齐
        public bool isDeleteTempPath = true;  //删除临时目录
        public bool isSimplifyYml = true;     //精简YML

        public string apktoolVersion = "apktool";

        public List<SDKInfo> sdkList = new List<SDKInfo>();
        public List<KeyValue> propertiesList = new List<KeyValue>();

        public string Name { get => channelName; set => channelName = value; }
        public bool IsChecked { get => isChecked; set => isChecked = value; }
        public string KeyStorePath { get => keyStorePath; set => keyStorePath = value; }
        public string KeyStorePassWord { get => keyStorePassWord; set => keyStorePassWord = value; }
        public string KeyStoreAlias { get => keyStoreAlias; set => keyStoreAlias = value; }
        public string KeyStoreAliasPassWord { get => keyStoreAliasPassWord; set => keyStoreAliasPassWord = value; }
        public string PackageName { get => packageName; set => packageName = value; }
        public string AppName { get => appName; set => appName = value; }
        public string AppIcon { get => appIcon; set => appIcon = value; }
        public List<SDKInfo> SdkList { get => sdkList; set => sdkList = value; }
        public string AppBanner { get => appBanner; set => appBanner = value; }
        public string Suffix { get => suffix; set => suffix = value; }
        public List<KeyValue> PropertiesList { get => propertiesList; set => propertiesList = value; }
        public string ApktoolVersion { get => apktoolVersion; set => apktoolVersion = value; }
        public bool IsLog { get => isLog; set => isLog = value; }
        public bool IsDeleteTempPath { get => isDeleteTempPath; set => isDeleteTempPath = value; }
        public bool IsForceManifest { get => isForceManifest; set => isForceManifest = value; }
        public List<KeyValue> AppNameLanguages { get => appNameLanguages; set => appNameLanguages = value; }
        public bool IsSplitDex { get => isSplitDex; set => isSplitDex = value; }
        public bool IsResplitDex { get => isResplitDex; set => isResplitDex = value; }
        public bool IsRebuildRTable { get => isRebuildRTable; set => isRebuildRTable = value; }
        public bool IsUseAAPT2 { get => isUseAAPT2; set => isUseAAPT2 = value; }
        public bool IsChangeMainApplication { get => isChangeMainApplication; set => isChangeMainApplication = value; }
        public bool IsChangeMainActivity { get => isChangeMainActivity; set => isChangeMainActivity = value; }
        public bool IsExecuteInvalidFile { get => isExecuteInvalidFile; set => isExecuteInvalidFile = value; }

        public bool IsOnlyMainClasses { get => isOnlyMainClasses; set => isOnlyMainClasses = value; }
        public bool IsZipalign { get => isZipalign; set => isZipalign = value; }
        public bool UseV2Sign { get => useV2Sign; set => useV2Sign = value; }
        public string JksPath { get => jksPath; set => jksPath = value; }
        public bool IsNoCompressResource { get => isNoCompressResource; set => isNoCompressResource = value; }
        public bool IsSimplifyYml { get => isSimplifyYml; set => isSimplifyYml = value; }
        public int SplitDexOffset { get => splitDexOffset; set => splitDexOffset = value; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Change()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged?.Invoke(this, e);
        }

        public ChannelInfo Clone()
        {
            Deserializer des = new Deserializer();
            string content =  Serializer.Serialize(this);
            return des.Deserialize<ChannelInfo>(content);
        }

        public SDKInfo GetSDKInfo(string sdkName)
        {
            for (int i = 0; i < sdkList.Count; i++)
            {
                if(sdkList[i].sdkName == sdkName)
                {
                    return sdkList[i];
                }
            }

            return null;
        }

        public bool GetAssignMinAPILevel()
        {
            for (int i = 0; i < sdkList.Count; i++)
            {
                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(sdkList[i].sdkName);
                if (config.assignMinAPILevel)
                {
                    return true;
                }
            }

            return false;
        }

        public bool GetUseExtraFile()
        {
            for (int i = 0; i < sdkList.Count; i++)
            {
                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(sdkList[i].sdkName);
                if (config.UseExtraFile)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 最小APILevel 取所有sdk设置中最大的
        /// </summary>
        /// <returns></returns>
        public int GetMinAPILevel()
        {
            int minApiLevel = -1;
            for (int i = 0; i < sdkList.Count; i++)
            {
                SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(sdkList[i].sdkName);
                if (config.MinSDKversion > minApiLevel)
                {
                    minApiLevel = config.MinSDKversion;
                }
            }

            return minApiLevel;
        }
    }

    public class TotalSDKConfig : List<SDKConfig>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Change()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged?.Invoke(this, e);
        }

        public SDKConfig GetSDKConfig(string sdkName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if(this[i].sdkName == sdkName)
                {
                    return this[i];
                }
            }
            throw new Exception("找不到SDK设置！ SdkName ->" + sdkName + "<-");
        }
    }



    public class KeyValue
    {
        public string key;
        public string value;

        public string Key { get => key; set => key = value; }
        public string Value { get => value; set => this.value = value; }

        public KeyValue DeepCopy()
        {
            KeyValue cp = new KeyValue();
            cp.key = key;
            cp.value = value;

            return cp;
        }
    }

    /// <summary>
    /// SDKConfig 是SDK的设置
    /// </summary>
    public class SDKConfig 
    {
        public string sdkName;
        public string className;

        public bool useCustomJavaClass;
        public List<KeyValue> customJavaClass = new List<KeyValue>();
        public List<string> customJavaLibrary = new List<string>();

        public SDKType sdkType;
        public List<KeyValue> configList = new List<KeyValue>();
        public List<string> permissionList = new List<string>();

        public List<string> mavenPathList = new List<string>();
        public List<string> mavenLibList = new List<string>();

        public int minSDKversion = 16;
        public int targetSDKVersion;
        public string applicationName;

        public bool assignMinAPILevel = false;

        public bool force32bit; //强制32位执行
        public bool useMaven;   //使用Maven
        public bool useExtraFile; //放入额外文件

        public List<KeyValue> xmlHeadList = new List<KeyValue>();
        public List<ActivityInfo> activityInfoList = new List<ActivityInfo>();
        public List<KeyValue> mainActivityPropertyList = new List<KeyValue>();
        public List<ServiceInfo> serviceInfoList = new List<ServiceInfo>();
        public List<ProviderInfo> providerInfoList = new List<ProviderInfo>();
        public List<KeyValue> metaInfoList = new List<KeyValue>();
        public List<KeyValue> usesList = new List<KeyValue>();
        public List<KeyValue> applicationHeadList = new List<KeyValue>();
        public List<string> firstDexList = new List<string>();

        public string SdkName { get => sdkName; set => sdkName = value; }

        public bool IsChecked
        {
            get
            {
                return EditorData.CurrentChannel.GetSDKInfo(SdkName) != null;
            }

            set
            {
                if(value)
                {
                    if(!IsChecked)
                    {
                        SDKInfo info = new SDKInfo();
                        info.sdkName = sdkName;
                        info.sdkConfig = new List<KeyValue>();

                        for (int i = 0; i < configList.Count; i++)
                        {
                            info.sdkConfig.Add(configList[i].DeepCopy());
                        }

                        EditorData.CurrentChannel.sdkList.Add(info);
                    }
                }
                else
                {
                    if(IsChecked)
                    {
                        MessageBoxResult result = MessageBox.Show("确定要移除这个SDK吗？\n所有填写的配置都会丢失", "警告", MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            for (int i = 0; i < EditorData.CurrentChannel.sdkList.Count; i++)
                            {
                                if (EditorData.CurrentChannel.sdkList[i].sdkName == sdkName)
                                {
                                    EditorData.CurrentChannel.sdkList.RemoveAt(i);
                                    i--;
                                }
                            }
                        }
                    }
                }
            }
        }

        public SDKType SdkType { get => sdkType; set => sdkType = value; }
        public string ClassName { get => className; set => className = value; }
        public int MinSDKversion { get => minSDKversion; set => minSDKversion = value; }
        public int TargetSDKVersion { get => targetSDKVersion; set => targetSDKVersion = value; }
        public List<ActivityInfo> ActivityInfoList { get => activityInfoList; set => activityInfoList = value; }
        public string ApplicationName { get => applicationName; set => applicationName = value; }
        public bool UseCustomJavaClass { get => useCustomJavaClass; set => useCustomJavaClass = value; }
        public List<KeyValue> CustomJavaClass { get => customJavaClass; set => customJavaClass = value; }
        public List<string> CustomJavaLibrary { get => customJavaLibrary; set => customJavaLibrary = value; }
        public List<ProviderInfo> ProviderInfoList { get => providerInfoList; set => providerInfoList = value; }
        public List<KeyValue> MainActivityPropertyList { get => mainActivityPropertyList; set => mainActivityPropertyList = value; }
        public List<KeyValue> MetaInfoList { get => metaInfoList; set => metaInfoList = value; }
        public List<KeyValue> XmlHeadList { get => xmlHeadList; set => xmlHeadList = value; }
        public List<KeyValue> UsesList { get => usesList; set => usesList = value; }
        public bool Force32bit { get => force32bit; set => force32bit = value; }
        public List<KeyValue> ApplicationHeadList { get => applicationHeadList; set => applicationHeadList = value; }
        public List<string> FirstDexList { get => firstDexList; set => firstDexList = value; }
        public bool UseMaven { get => useMaven; set => useMaven = value; }
        public bool AssignMinAPILevel { get => assignMinAPILevel; set => assignMinAPILevel = value; }
        public bool UseExtraFile { get => useExtraFile; set => useExtraFile = value; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Change()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged?.Invoke(this, e);
        }
    }

    /// <summary>
    /// SDKInfo 是用户的设置
    /// </summary>
    public class SDKInfo
    {
        public string sdkName;
        public List<KeyValue> sdkConfig;

        public string SdkName { get => sdkName; set => sdkName = value; }
    }

    public class ActivityInfo 
    {
        public bool mainActivity = false;
        public string name;
        public string content;

        public string Content { get => content; set => content = value; }
        public string Name { get => name; set => name = value; }
        public bool MainActivity { get => mainActivity; set => mainActivity = value; }
    }

    public class ServiceInfo 
    {
        public string name;
        public string content;

        public string Content { get => content; set => content = value; }
        public string Name { get => name; set => name = value; }
    }

    public class ProviderInfo
    {
        public string name;
        public string content;

        public string Content { get => content; set => content = value; }
        public string Name { get => name; set => name = value; }

    }

    class StringList : List<string>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Change()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged?.Invoke(this, e);
        }
    }

    [Flags]
    public enum SDKType
    {
        Login = 1,
        AD = 2,
        Log = 4,
        Pay = 8,
        Other = 16,
        RealName = 32,
        Share = 64,
    }

    #endregion

}
