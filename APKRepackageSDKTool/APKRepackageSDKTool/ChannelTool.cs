using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            OutPut("替换包名");
            OutPut("替换appName");
            OutPut("替换appIcon");
            OutPut("放入SDK");

            OutPut("整合AndroidManifest.xml 清单文件");
        }

        public void OutPut(string content)
        {
            callBack?.Invoke(content);
        }
    }

    /// <summary>
    /// 储存渠道所需的数据
    /// </summary>
    public class ChannelInfo
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
    }

    public class SDKInfo
    {
        public string SDKName;
        public Dictionary<string, string> SDKConfig;
    }
}
