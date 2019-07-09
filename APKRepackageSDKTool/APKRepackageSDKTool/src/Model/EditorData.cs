using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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

                SdkLibPath = RecordManager.GetRecord(c_ConfigRecord, "SDKLibPath", null);

                UpdateTotalSDKInfo();
            }
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
            //遍历SDKPath下的全部文件夹，排除Config文件夹

            if (!string.IsNullOrEmpty(SdkLibPath))
            {
                if(totalSDKConfig == null)
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

                        SDKConfig info = LoadSDKConfig(FileTool.GetDirectoryName(dires[i]),configPath);

                        totalSDKConfig.Add(info);
                    }
                }

                TotalSDKInfo = totalSDKConfig;
            }
            else
            {
                totalSDKConfig = null;
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

        public string keyStorePath; //KeyStore的路径
        public string keyStorePassWord; //KeyStore 密码

        public string keyStoreAlias;         //别名
        public string keyStoreAliasPassWord; //别名密码

        public string packageName; //包名
        public string appName;
        public string appIcon;
        public string appBanner;

        public bool isLog;  //输出日志

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

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Change()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged?.Invoke(this, e);
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
            throw new Exception("找不到SDK设置！ " + sdkName);
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
        public int minSDKversion;
        public int targetSDKVersion;
        public string applicationName;

        public List<ActivityInfo> activityInfoList = new List<ActivityInfo>();
        public List<KeyValue> mainActivityPropertyList = new List<KeyValue>();
        public List<ServiceInfo> serviceInfoList = new List<ServiceInfo>();
        public List<ProviderInfo> providerInfoList = new List<ProviderInfo>();
        public List<KeyValue> metaInfoList = new List<KeyValue>();

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
        public bool mainActivity = true;
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
    }

    #endregion

}
