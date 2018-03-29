using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKRepackageSDKTool
{

    public static class EditorData
    {
        const string c_GameRecord = "Games";

        private static TotalGameData gameList;

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

        static bool isInit = false;
        public static void JudgeInit()
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

                GameInfo gi = new GameInfo();
                gi.GameName = "星空遇奇";
                gi.channelInfo = new List<ChannelInfo>();

                ChannelInfo ci1 = new ChannelInfo();
                ci1.Name = "小米";
                gi.channelInfo.Add(ci1);

                ChannelInfo ci2 = new ChannelInfo();
                ci2.Name = "华为";
                gi.channelInfo.Add(ci2);

                ChannelInfo ci3 = new ChannelInfo();
                ci3.Name = "GooglePlay";


                GameInfo gi2 = new GameInfo();
                gi2.GameName = "元素大作战";
                gi2.channelInfo = new List<ChannelInfo>();
                gi2.channelInfo.Add(ci1);
                gi2.channelInfo.Add(ci3);

                gameList.Add(gi);
                gameList.Add(gi2);
            }
        }
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
        public List<ChannelInfo> channelInfo;


        public string GameName { get => gameName; set => gameName = value; }
    }

    #endregion

}
