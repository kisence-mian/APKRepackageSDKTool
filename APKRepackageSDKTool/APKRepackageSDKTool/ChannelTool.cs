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
        public string channelID;

        public string keyStorePath; //KeyStore的路径
        public string keyStorePassWord; //KeyStore 密码

        public string keyStoreAlias;         //别名
        public string keyStoreAliasPassWord; //别名密码

        public string packageName; //包名
        public string appName;
        public string appIcon;

        public List<SDKInfo> sdkList;
    }

    public class SDKInfo
    {
        public string SDKName;
        public Dictionary<string, string> SDKConfig;
    }
}
