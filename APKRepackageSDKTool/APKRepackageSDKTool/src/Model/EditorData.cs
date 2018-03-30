using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        static TotalSDKConfig totalSDKInfo;

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
                if(totalSDKInfo == null)
                {
                    GetTotalSDKInfo();
                }
                return totalSDKInfo;
            }

            set
            {
                totalSDKInfo = value;
                totalSDKInfo.Change();
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

                SdkLibPath = RecordManager.GetRecord(c_ConfigRecord, "SDKLibPath", "null");

                GetTotalSDKInfo();
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

        static void GetTotalSDKInfo()
        {
            //遍历SDKPath下的全部文件夹，排除Config文件夹

            if (!string.IsNullOrEmpty(SdkLibPath))
            {
                totalSDKInfo = new TotalSDKConfig();

                string[] dires = Directory.GetDirectories(SdkLibPath);
                for (int i = 0; i < dires.Length; i++)
                {
                    if (!dires[i].Contains("Config")
                        && !dires[i].Contains("git")
                        )
                    {
                        //读取每个文件夹中的config.json，以获取对应的设置要求
                        string configPath = dires[i] + "\\config.json";

                        SDKConfig info = LoadSDKConfig(configPath);

                        totalSDKInfo.Add(info);
                    }
                }
            }
            else
            {
                totalSDKInfo = null;
            }
        }

        static SDKConfig LoadSDKConfig(string path)
        {
            string content = FileTool.ReadStringByFile(path);
            SDKConfig config = des.Deserialize<SDKConfig>(content);

            return config;
        }

        static void SaveSDKConfig(string path, SDKConfig config)
        {
            string content = Serializer.Serialize(config);
            FileTool.WriteStringByFile(path, content);
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

        public bool isChecked; //是否被选中

        public string keyStorePath; //KeyStore的路径
        public string keyStorePassWord; //KeyStore 密码

        public string keyStoreAlias;         //别名
        public string keyStoreAliasPassWord; //别名密码

        public string packageName; //包名
        public string appName;
        public string appIcon;
        public string appBanner;

        public List<SDKInfo> sdkList;

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

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Change()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged?.Invoke(this, e);
        }
    }

    public class TotalSDKConfig : List<SDKConfig>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Change()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged(this, e);
        }
    }

    /// <summary>
    /// SDKConfig 是SDK的设置
    /// </summary>
    public class SDKConfig
    {
        public string sdkName;
        public Dictionary<string, string> sdkConfig;

        public string SdkName { get => sdkName; set => sdkName = value; }
    }

    /// <summary>
    /// SDKInfo 是用户的设置
    /// </summary>
    public class SDKInfo
    {
        public string sdkName;
        private bool isChecked;
        public Dictionary<string, string> sdkConfig;

        public string SdkName { get => sdkName; set => sdkName = value; }
        public bool IsChecked { get => isChecked; set => isChecked = value; }
    }

    #endregion

}
