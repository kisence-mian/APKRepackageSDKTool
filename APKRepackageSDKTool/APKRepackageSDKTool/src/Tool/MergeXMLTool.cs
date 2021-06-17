using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

/// <summary>
/// 专门用来处理xml的批量合并，并提供性能优化
/// </summary>
public class MergeXMLTool
{
    OutPutCallBack callBack; 
    OutPutCallBack errorCallBack; 

    Dictionary<string, Dictionary<string, string>> hash = new Dictionary<string, Dictionary<string, string>>();
    Dictionary<string, XmlDocument> aimDict = new Dictionary<string, XmlDocument>();
    Dictionary<string, XmlDocument> sdkDict = new Dictionary<string, XmlDocument>();

    public MergeXMLTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;
    }

    public void Merge(string SDKPath, string aimPath)
    {
        //OutPut("开始合并 XML \n" + SDKPath + "\n " + aimPath);

        //加载目标路径的所有xml并存储
        LoadAimXML(aimPath);

        //依次加载sdk中的xml并和分类存储中的文档进行比对去重
        LoadSdkXML(SDKPath);

        //将判重之后的结果进行存储
        SaveXML(aimPath);
    }



    private void SaveXML(string aimPath)
    {
        foreach (var item in aimDict)
        {
            if(sdkDict.ContainsKey(item.Key))
            {
                MergeSingleXML(sdkDict[item.Key], item.Value);
            }
        }

        foreach (var item in aimDict)
        {
            item.Value.Save(aimPath + "\\" + item.Key + ".xml");

            //OutPut("aim 存储 " + aimPath + "\\" + item.Key + ".xml");
        }

        foreach (var item in sdkDict)
        {
            if(!aimDict.ContainsKey(item.Key))
            {
                item.Value.Save(aimPath + "\\" + item.Key + ".xml");

                //OutPut("sdk 存储 " + aimPath + "\\" + item.Key + ".xml");
            }
        }
    }

    string GenerateXMLKey(XmlElement ele)
    {
        string key = "";

        if(ele.Name == "item")
        {
            key = ele.GetAttribute("type") + "#" + ele.GetAttribute("name");
        }
        if(ele.Name == "attr")
        {
            if(ele.HasAttribute("format"))
            {
                key = ele.GetAttribute("format") + "#" + ele.GetAttribute("name");
            }
            else
            {
                key = ele.Name + "#" + ele.GetAttribute("name");
            }
        }
        else if (ele.HasAttribute("name"))
        {
            key = ele.Name + "#" + ele.GetAttribute("name");
        }
        else
        {
            key = ele.Name + "#" + ele.InnerXml;
        }

        if(string.IsNullOrEmpty(key) || key == "#" )
        {
            OutPut("I: xml Key 获取失败" + ele.OuterXml);
        }

        return key;
    }

    bool IsRepeat(string attributeName)
    {
        bool result = false;

        foreach (var item in hash)
        {
            result |= item.Value.ContainsKey(attributeName);
        }

        return result;
    }

    void LoadAimXML(string aimPath)
    {
        string[] allFileName = Directory.GetFiles(aimPath);
        foreach (var item in allFileName)
        {
            //只处理xml文件
            if(!item.EndsWith(".xml"))
            {
                continue;
            }

            string fileName = FileTool.RemoveExpandName(FileTool.GetFileNameByPath(item));
            //OutPut("LoadAimXML fileName " + fileName);

            XmlDocument doc = new XmlDocument();
            doc.Load(item);

            XmlElement root = doc.DocumentElement;
            Dictionary<string, string> map = new Dictionary<string, string>();

            hash.Add(fileName, map);
            aimDict.Add(fileName, doc);

            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode tmp = root.ChildNodes[i];

                if(tmp.NodeType == XmlNodeType.Element)
                {
                    XmlElement ele = (XmlElement)tmp;

                    string key = GenerateXMLKey(ele);
                    if (!map.ContainsKey(key)  && !string.IsNullOrEmpty(key))
                    {
                        map.Add(key, ele.Value);
                    }
                    else
                    {
                        //OutPut("重复的key  " + key);
                    }
                }

                //用于处理自定义组件 declare-styleable
                if (tmp.HasChildNodes && tmp.Name == "declare-styleable")
                {
                    JudgeAimChildNode(tmp,map);
                }
            }
        }
    }

    void JudgeAimChildNode(XmlNode node,Dictionary<string,string> map)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode tmp = node.ChildNodes[i];

            if (tmp.NodeType == XmlNodeType.Element)
            {
                XmlElement ele = (XmlElement)tmp;
                string key = GenerateXMLKey(ele);

                if (tmp.HasChildNodes)
                {
                    if (!map.ContainsKey(key) && !string.IsNullOrEmpty(key))
                    {
                        map.Add(key, ele.Value);
                    }
                    else
                    {
                        //OutPut("I: 重复的节点  " + key);
                    }
                }

                //对一种情况进行特殊处理
                // <attr format="dimension" name="android:translationX" />
                if (ele.HasAttribute("format") && ele.GetAttribute("name").Contains("android:"))
                {
                    node.RemoveChild(tmp);
                    i--;

                    OutPut("I: 特殊处理掉的节点 " + key);
                }
            }

            //else
            //{
            //    OutPut("跳过节点  " + tmp.Name);
            //}
        }
    }

    void LoadSdkXML(string sdkPath)
    {
        string[] allFileName = Directory.GetFiles(sdkPath);
        foreach (var item in allFileName)
        {
            //只处理xml文件
            if (!item.EndsWith(".xml"))
            {
                continue;
            }

            string fileName = FileTool.RemoveExpandName(FileTool.GetFileNameByPath(item));
            //OutPut("SDKPath fileName " + fileName);

            XmlDocument doc = new XmlDocument();
            doc.Load(item);

            sdkDict.Add(fileName, doc);

            XmlElement root = doc.DocumentElement;
            Dictionary<string, string> map;

            if(!hash.ContainsKey(fileName))
            {
                map = new Dictionary<string, string>();
                hash.Add(fileName, map);
            }
            else
            {
                map = hash[fileName];
            }

            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode tmp = root.ChildNodes[i];

                if(tmp.NodeType == XmlNodeType.Element)
                {
                    XmlElement ele = (XmlElement)tmp;

                    string key = GenerateXMLKey(ele);

                    //去重
                    if (IsRepeat(key) && !string.IsNullOrEmpty(key))
                    {
                        root.RemoveChild(tmp);
                        i--;
                        //OutPut("I: 重复的节点 " + key);
                    }
                    else
                    {
                        map.Add(key, ele.Value);
                    }
                }
                //else
                //{
                //    OutPut("跳过节点  " + tmp.Name);
                //}

                //用于处理自定义组件 declare-styleable
                if (tmp.HasChildNodes && tmp.Name == "declare-styleable")
                {
                    JudgeSdkChileNode(tmp, map);
                }
            }
        }
    }

    void JudgeSdkChileNode(XmlNode node, Dictionary<string, string> map)
    {
        for (int i = 0; i < node.ChildNodes.Count; i++)
        {
            XmlNode tmp = node.ChildNodes[i];

            if (tmp.NodeType == XmlNodeType.Element && tmp.HasChildNodes)
            {
                XmlElement ele = (XmlElement)tmp;

                string key = GenerateXMLKey(ele);

                //去重
                if (IsRepeat(key) && !string.IsNullOrEmpty(key))
                {
                    node.RemoveChild(tmp);
                    i--;
                    //OutPut("I: 重复的节点 " + key);
                }
                else
                {
                    map.Add(key, ele.Value);
                }

                //对一种情况进行特殊处理
                // <attr format="dimension" name="android:translationX" />
                if(ele.HasAttribute("format") && ele.GetAttribute("name").Contains("android:"))
                {
                    node.RemoveChild(tmp);
                    i--;

                    OutPut("I: 特殊处理掉的节点 " + key);
                }

            }
            //else
            //{
            //    OutPut("跳过节点  " + tmp.Name);
            //}
        }
    }

    public void MergeSingleXML(XmlDocument doca, XmlDocument docb)
    {
        // 分别获取两个文档的根元素，以便于合并
        XmlElement rootA = doca.DocumentElement;
        XmlElement rootB = docb.DocumentElement;

        foreach (XmlNode node in rootA.ChildNodes)
        {
            // 先导入节点
            XmlNode n = docb.ImportNode(node, true);

            // 然后，插入指定的位置
            rootB.AppendChild(n);
        }

        //docb.Save(fileB);
    }

    bool IsRepeatNode(XmlElement root, XmlNode node)
    {
        for (int i = 0; i < root.ChildNodes.Count; i++)
        {
            XmlNode tmp = root.ChildNodes[i];

            //跳过注释
            if (tmp.NodeType == XmlNodeType.Comment)
            {
                continue;
            }

            XmlElement ele = (XmlElement)tmp;

            if (ele.Name == node.Name
                && ele.OuterXml == node.OuterXml)
            {
                return true;
            }
        }

        return false;
    }

    bool IsRepeatNodeNameOnly(XmlElement root, XmlElement node)
    {
        for (int i = 0; i < root.ChildNodes.Count; i++)
        {
            XmlNode tmp = root.ChildNodes[i];

            //跳过注释
            if (tmp.NodeType == XmlNodeType.Comment
                //|| tmp.NodeType == XmlNodeType.Text
                //|| tmp.NodeType == XmlNodeType.Element
                )
            {
                continue;
            }
            XmlElement ele = (XmlElement)tmp;

            if ((ele.Name == node.Name || (ele.Name == "item" && ele.GetAttribute("type") == node.Name))
                 && ele.GetAttribute("name") == node.GetAttribute("name")
                 && ele.GetAttribute("name") != string.Empty
                 && ele.GetAttribute("name") != null)
            {
                return true;
            }
        }

        return false;
    }

    public void OutPut(string content)
    {
        callBack?.Invoke(content);
    }

    public void ErrorOutPut(string content)
    {
        errorCallBack?.Invoke(content);
    }
}