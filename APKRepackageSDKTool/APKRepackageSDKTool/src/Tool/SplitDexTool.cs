using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SplitDexTool
{
    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;

    const string c_NoneName = "None";
    const int c_maxNum = 65535;

    Dictionary<SmaliType, int> currentNum = new Dictionary<SmaliType, int>();
    Dictionary<SmaliType, int> currentIndex = new Dictionary<SmaliType, int>();

    Dictionary<SmaliType, Dictionary<string,string >> allName = new Dictionary<SmaliType, Dictionary<string, string>>();

    public SplitDexTool(OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;
    }

    public void SplitDex(string aimPath, ChannelInfo info)
    {
        //分别计算  field  method string type proto methodHandle callSite 的数量，并分开分包
        //为提高效率，只遍历一次

        string smaliPath = aimPath.Replace("\\", "/") + "/smali";
        List<string> list = FileTool.GetAllFileNamesByPath(smaliPath, new string[] { "smali" });

        if (info.IsResplitDex)
        {
            //将其他class的合并到主class中再进行分包
            ReSplitDex(aimPath, smaliPath);
        }

        //初始化
        foreach (SmaliType item in Enum.GetValues(typeof(SmaliType)))
        {
            currentNum.Add(item,0);
            currentIndex.Add(item, 1);
            allName.Add(item, new Dictionary<string, string>());
        }

        //计算主dex 的数据
        for (int i = 0; i < list.Count; i++)
        {
            list[i] = list[i].Replace("\\", "/");
            if (JudgeMainDex(list[i], info))
            {
                string content = FileTool.ReadStringByFile(list[i]);

                foreach (SmaliType item in Enum.GetValues(typeof(SmaliType)))
                {
                    currentNum[item] += CalcNumberBySmaliType(item, content);
                }
            }
        }

        OutPut("共 " + list.Count + " 个 Smali 文件");
        foreach (SmaliType item in Enum.GetValues(typeof(SmaliType)))
        {
            OutPut(item + " " + currentNum[item]);
        }

        int moveIndex = 1;

        for (int i = 0; i < list.Count; i++)
        {
            list[i] = list[i].Replace("\\", "/");
            //剔除所有不需要分包的类
            if (JudgeMainDex(list[i], info))
            {
                continue;
            }

            string content = FileTool.ReadStringByFile(list[i]);

            if(string.IsNullOrEmpty(content))
            {
                OutPut("E: 找不到文件 " + list[i]);
                continue;
            }

            //计算当前有多少字段
            foreach (SmaliType item in Enum.GetValues(typeof(SmaliType)))
            {
                currentNum[item] += CalcNumberBySmaliType(item, content);

                if (currentNum[item] > c_maxNum)
                {
                    foreach (SmaliType item2 in Enum.GetValues(typeof(SmaliType)))
                    {
                        //全部重新计算
                        currentNum[item2] = 0;
                        currentIndex[item2]++;
                    }

                    moveIndex = currentIndex[item];

                    string newPath = smaliPath + "_classes" + currentIndex[item];
                    FileTool.CreatePath(newPath);

                    OutPut("I: "+ item + "超过上限： currentIndex " + currentIndex[item] + " name " + list[i]);
                    break;
                }
            }

            if (moveIndex > 1 )
            {
                string newPath = smaliPath + "_classes" + moveIndex;
                string targetPath = newPath + "" + list[i].Replace(smaliPath, "");
                FileTool.CreateFilePath(targetPath);

                if (File.Exists(list[i]))
                {
                    FileTool.MoveFile(list[i], targetPath);
                }
                else
                {
                    OutPut("W: 分包时找不到路径 " + list[i]);
                }
            }
        }
    }

    void ReSplitDex(string aimPath, string smaliPath)
    {
        for (int i = 2; i <= 20; i++)
        {
            string pathTmp = aimPath.Replace("\\", "/") + "/smali_classes" + i;
            if (Directory.Exists(pathTmp))
            {
                FileTool.CopyDirectory(pathTmp, smaliPath, Repeat);
                FileTool.SafeDeleteDirectory(pathTmp);
            }
        }
    }

    int CalcNumberBySmaliType(SmaliType type,string content)
    {
        switch(type)
        {

            case SmaliType.String: return CalcStringNumber(type, content);
            case SmaliType.Field:return CalcFieldNumber(type,content);
            case SmaliType.Method:return CalcMethodNumber(type, content);
            case SmaliType.Type: return CalcTypeNumber(type, content);

            //为提高效率，不分析对分包影响不大的文件
            //case SmaliType.CallSite:return CalcCallSiteHandleNumber(type, content);
            //case SmaliType.MethodHandle: return CalcMethodHandleNumber(type, content);
        }

        return 0;
    }

    private int CalcTypeNumber(SmaliType type, string content)
    {
        int count = 0;
        string[] lines = content.Split('\n');
        string className = ParseClassName(lines[0].Trim());

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            string name = c_NoneName;

            if (line.StartsWith("const-classe"))
            {
                name = ParseName(className, line);
            }

            if (line.StartsWith("check-cast"))
            {
                name = ParseName(className, line);
            }

            if (line.StartsWith("instance-of"))
            {
                name = ParseName(className, line);
            }

            if (line.StartsWith("new-instance"))
            {
                name = ParseName(className, line);
            }

            if (line.StartsWith("new-array"))
            {
                name = ParseName(className, line);
            }

            if (line.StartsWith("filled-new-array"))
            {
                name = ParseName(className, line);
            }

            if (name == c_NoneName)
            {
                continue;
            }

            if (!allName[type].ContainsKey(name))
            {
                //allName[type].Add(name, name);
                count += 1;
            }
        }

        return count;
    }

    private int CalcCallSiteHandleNumber(SmaliType type, string content)
    {
        int count = 0;
        string[] lines = content.Split('\n');
        string className = ParseClassName(lines[0].Trim());

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            string name = c_NoneName;

            if (line.StartsWith("invoke-custom"))
            {
                name = ParseName(className, line);
            }

            if (name == c_NoneName)
            {
                continue;
            }

            if (!allName[type].ContainsKey(name))
            {
                //allName[type].Add(name, name);
                count += 1;
            }
        }

        return count;
    }

    private int CalcMethodHandleNumber(SmaliType type, string content)
    {
        int count = 0;
        string[] lines = content.Split('\n');
        string className = ParseClassName(lines[0].Trim());

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            string name = c_NoneName;

            if (line.StartsWith("const-method-handle"))
            {
                name = ParseName(className, line);
            }

            if (name == c_NoneName)
            {
                continue;
            }

            if (!allName[type].ContainsKey(name))
            {
                count += 1;
            }
        }

        return count;
    }

    private int CalcStringNumber(SmaliType type, string content)
    {
        Dictionary<string, int> types = new Dictionary<string, int>();


        int count = 0;
        string[] lines = content.Split('\n');
        string className = ParseClassName(lines[0].Trim());

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            string name = c_NoneName;

            if (line.StartsWith("const-string"))
            {
                name = ParseString(className, line);
            }

            if (name == c_NoneName)
            {
                continue;
            }

            //不排重
            count += 1;
        }

        return count;
    }

    int CalcMethodNumber(SmaliType type, string content)
    {
        Dictionary<string, int> types = new Dictionary<string, int>();

        int count = 0;
        string[] lines = content.Split('\n');
        string className = ParseClassName(lines[0].Trim());

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            string name = c_NoneName;

            if (line.StartsWith("invoke-virtual"))
            {
                name = "invoke-virtual" + ParseName(className, line);
            }

            if (line.StartsWith("invoke-super"))
            {
                name = "invoke-super" + ParseName(className, line);
            }

            if (line.StartsWith("invoke-direct"))
            {
                name = "invoke-direct" + ParseName(className, line);
            }

            if (line.StartsWith("invoke-interface"))
            {
                name = "invoke-interface" + ParseName(className, line);
            }

            if (line.StartsWith("invoke-direct-empty"))
            {
                name = "invoke-direct-empty" + ParseName(className, line);
            }

            if (line.StartsWith("invoke-object-init"))
            {
                name = "invoke-object-init" + ParseName(className, line);
            }

            if (line.StartsWith("invoke-polymorphic"))
            {
                name = "invoke-polymorphic" + ParseName(className, line);
            }

            if (name == c_NoneName)
            {
                continue;
            }

            //尝试单文件排重
            if (!types.ContainsKey(name))
            {
                //types.Add(name, 1);

                //allName[type].Add(name, name);
                count += 1;
            }
        }

        return count;
    }

    int CalcFieldNumber(SmaliType type, string content)
    {
        int count = 0;
        string[] lines = content.Split('\n');
        string className = ParseClassName(lines[0].Trim());

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            string name = c_NoneName;

            if (line.StartsWith("iget"))
            {
                name = ParseFieldName(className, line);
            }

            if (line.StartsWith("iput"))
            {
                name = ParseFieldName(className, line);
            }

            if (line.StartsWith("sget"))
            {
                name = ParseFieldName(className, line);
            }

            if (line.StartsWith("sput"))
            {
                name = ParseFieldName(className, line);
            }

            if (line.StartsWith("invoke-"))
            {
                name = ParseFieldName(className, line);
            }

            if (name == c_NoneName)
            {
                continue;
            }

            if (!allName[type].ContainsKey(name))
            {
                count += 1;
            }
        }

        return count;
    }

    #region 解析方法

    private string ParseName(string className, string line)
    {
        string[] blocks = line.Split(' ');
        return className + "->" + blocks[blocks.Length - 1];
    }


    private string ParseString(string className, string line)
    {
        string[] blocks = line.Split(' ');
        return className + "->" + blocks[blocks.Length - 1];
    }

    string ParseClassName(string line)
    {
        if (line.StartsWith(".class"))
        {
            return c_NoneName;
        }

        string[] blocks = line.Split(' ');
        return blocks[blocks.Length - 1];
    }
 

    string ParseFieldName(string className, string line)
    {
        string[] blocks = line.Split(' ');
        if (blocks.Length >= 7)
        {
            string[] kv = blocks[blocks.Length - 3].Split(':');
            return className + "->" + kv[0];
        }
        else if (blocks.Length >= 4)
        {
            string[] kv = blocks[blocks.Length - 1].Split(':');
            return className + "->" + kv[0];
        }
        else
        {
            return c_NoneName;
        }
    }

    #endregion



    bool JudgeMainDex(string path, ChannelInfo info)
    {
        List<string> mainDexList = new List<string>();

        //将重要的类都放到主包里
        //默认设置
        mainDexList.Add(@"/android/support/multidex");
        mainDexList.Add("sdkInterface");
        mainDexList.Add("Application");
        mainDexList.Add("Activity");

        mainDexList.Add("Service");
        mainDexList.Add("Receiver");
        mainDexList.Add("Provider");
        
        //mainDexList.Add("Instrumentation");
        //mainDexList.Add("BackupAgent");
        //mainDexList.Add("Annotation");

        //mainDexList.Add("R$");
        //mainDexList.Add(@"/R.smali");

        for (int i = 0; i < info.SdkList.Count; i++)
        {
            SDKConfig config = EditorData.TotalSDKInfo.GetSDKConfig(info.SdkList[i].sdkName);
            for (int j = 0; j < config.FirstDexList.Count; j++)
            {
                if (!mainDexList.Contains(config.FirstDexList[j]))
                {
                    mainDexList.Add(config.FirstDexList[j]);
                }
            }
        }

        //将重要的类都放到主包里
        for (int i = 0; i < mainDexList.Count; i++)
        {
            if (path.Contains(mainDexList[i]))
            {
                return true;
            }
        }

        return false;
    }

    void Repeat(string p1, string p2)
    {
        File.Copy(p1, p2, true);
    }

    public void OutPut(string content)
    {
        callBack?.Invoke(content);
    }

    public void ErrorOutPut(string content)
    {
        errorCallBack?.Invoke(content);
    }


    enum SmaliType
    {
        String = 0,
        Type = 1,
        Field = 2,
        Method = 3,
        Proto = 4,
        CallSite = 5,
        MethodHandle = 6,
    }
}
