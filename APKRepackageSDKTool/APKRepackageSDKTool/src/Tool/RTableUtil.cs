using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APKRepackageSDKTool.src
{
    class RTableUtil
    {
        public OutPutCallBack callBack;
        public OutPutCallBack errorCallBack;

        Dictionary<string, string> RID = new Dictionary<string, string>();

        Dictionary<string, List<string>> RIDArray = new Dictionary<string, List<string>>();

        Dictionary<string, string> freeID = new Dictionary<string, string>(); //游离资源

        const string c_NoneField = "None";

        int IdIndex = 0;

        //int excessArrayNum = 0;
        //int 

        public RTableUtil(OutPutCallBack callBack, OutPutCallBack errorCallBack)
        {
            this.callBack = callBack;
            this.errorCallBack = errorCallBack;
        }


        public void GenerateRKV(string javaPath)
        {
            string content = FileTool.ReadStringByFile(javaPath);

            string[] lines = content.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                string field = c_NoneField;
                string value = c_NoneField;

                if (line.StartsWith("public static final int "))
                {
                    field = ParseJavaField(line);
                    value = ParseJavaFieldValue(line);

                    //查询真实数值
                    //if (!value.Contains("0x"))
                    //{
                    //    string arrayName = FindArrayName(field);

                    //    if (string.IsNullOrEmpty(field))
                    //    {
                    //        OutPut("E: 找不到数组 " + field);
                    //        continue;
                    //    }

                    //    List<string> values = RIDArray[arrayName];
                    //    int index = int.Parse(value);

                    //    if (values.Count > index || index >= 0)
                    //    {
                    //        OutPut("E: 数组越界 " + arrayName + " ->" + field + " -> " + value);
                    //        continue;
                    //    }

                    //    value = values[index];
                    //}

                    if (!RID.ContainsKey(field))
                    {
                        RID.Add(field, value);
                    }
                }

                //获取数组内容
                if (line.StartsWith("public static final int[] "))
                {
                    field = ParseJavaField(line);
                    List<string> array = new List<string>();

                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        string tmp = lines[j].Trim();

                        if (tmp.Contains("};"))
                        {
                            //OutPut("存储 array 数据 " + field + " -> " + array.Count);
                            for (int k = 0; k < array.Count; k++)
                            {
                                OutPut(array[k]);
                            }

                            RIDArray.Add(field, array);
                            break;
                        }
                        else
                        {
                            string[] arrs = lines[j].Replace(",", "").Trim().Split(' ');
                            array.AddRange(arrs);
                        }
                    }
                }
            }
        }

        public void ReplaceRTable(string smaliPath)
        {
            List<string> list = FileTool.GetAllFileNamesByPath(smaliPath, new string[] { "smali" });

            Dictionary<string, string> allMethod = new Dictionary<string, string>();

            for (int i = 0; i < list.Count; i++)
            {
                string path = FileTool.GetDirectoryName(list[i]);
                if(path.StartsWith("R$") && !path.StartsWith("R$styleable"))
                {
                    list[i] = list[i].Replace("\\", "/");

                    ReplaceSingleRTable(list[i]);
                    //Replace_Styleable(list[i]);
                }
            }

            OutPut("W: 找不到的资源数目： " + IdIndex);
        }

        public void ReplaceRsmali(string smaliPath,string packageName,Dictionary<string,string> template)
        {
            List<string> list = FileTool.GetAllFileNamesByPath(smaliPath, new string[] { "smali" });

            for (int i = 0; i < list.Count; i++)
            {
                string fileName = FileTool.GetFileNameByPath(list[i]);
                if (fileName.StartsWith("R$"))
                {
                    list[i] = list[i].Replace("\\", "/");
                    if(template.ContainsKey(fileName))
                    {
                        Replace_Single_Rsmali(list[i], packageName, template[fileName]);
                    }
                    else
                    {
                        OutPut("W: 找不到对应的模板： " + fileName + " " + list[i]);
                    }
                }
            }
        }

        void ReplaceSingleRTable(string path)
        {
            string content = FileTool.ReadStringByFile(path);

            Dictionary<string, string> RIDArrayNameOld_Tmp = new Dictionary<string, string>();
            Dictionary<string, List<string>> Rp = new Dictionary<string, List<string>>();

            //Dictionary<string, string> allMethod = new Dictionary<string, string>();

            string[] lines = content.Split('\n');

            //callBack("ReplaceSingleRTable " + path);
            OutPut("smaliPath " + path + " " + lines.Length);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                string field = null;

                if (line.StartsWith(".field") && !line.Contains(":[I"))
                {
                    field = ParseField( line);

                    //判断存在这个字段
                    if (field != null )
                    {
                        if(!RID.ContainsKey(field) )
                        {
                            string rid = GenerateRID();
                            RID.Add(field, rid);
                            OutPut("W: 找不到对应的R表ID " + field + " 构造ID->" + rid);
                        }

                        lines[i] = ReplaceFieldValue(line, RID[field]);
                        //OutPut("I: 替换常规数值 " + field + "->" + lines[i]);
                    }
                }

                //带有 :[I 的是数组,过滤掉
                if (line.StartsWith("sput") && !line.Contains(":[I"))
                {
                    string lastLine = lines[i - 2].Trim();
                    field = ParseFieldBySput(line);

                    if (!RID.ContainsKey(field))
                    {
                        string rid = GenerateRID();
                        RID.Add(field, rid);
                        OutPut("W: 找不到对应的R表ID " + field + " 构造ID->" + rid);
                    }

                    if (!lastLine.StartsWith("const"))
                    {
                        if(RID[field].Contains("0x"))
                        {
                            OutPut("E: 替换格式错误 " + field + " ->" + RID[field]);
                            RID[field] = "0";
                        }

                        string[] tmp = line.Split(' ');
                        string newLine = "    sput v" + RID[field] + ", " + tmp[2];

                        //插入一行常量赋值
                        //lines[i] = "    const v0, " + RID[field] + " \n\n" + newLine;

                        lines[i] = newLine;

                        OutPut("I: 替换从数组中读取值 " + field + " ->" + RID[field]);
                    }
                    else
                    {
                        lines[i - 2] = "    const v0, " + RID[field];
                        OutPut("I: 替换常量赋值 " + field + "->" + RID[field]);
                    }
                }

                //存储OldArray ID 映射，注意这里为了方便是以arrayName作为Key
                if (line.StartsWith("fill-array-data"))
                {
                    field = ParseFieldBySput(lines[i + 2].Trim());
                    string arrayName = ParseArrayNameByFill_Array_data(line);

                    if (!RIDArrayNameOld_Tmp.ContainsKey(arrayName))
                    {
                        RIDArrayNameOld_Tmp.Add(arrayName, field);

                        //OutPut("I: 存储 array Old 映射 " + field + " ->" + arrayName + "<");
                    }
                }

                //替换数组内容
                if (line.StartsWith(".array-data"))
                {

                    List<string> array = new List<string>();
                    string arrayName = lines[i - 1].Replace(":", "").Trim();

                    if (!RIDArrayNameOld_Tmp.ContainsKey(arrayName))
                    {
                        ErrorOutPut("E: 找不到Old array 数组映射 >" + arrayName + "<");
                        continue;
                    }

                    field = RIDArrayNameOld_Tmp[arrayName];
                    //OutPut("I: 替换数组 " + field  +"->"+ arrayName);

                    List<string> list = RIDArray[field];
                    
                    for (int j = 0; j <list.Count; j++)
                    {
                        string tmp = lines[i + 1 + j].Trim();
                        if(tmp.StartsWith(".end array-data"))
                        {
                            //OutPut("E: 替换前后两组数据长度不符 （原数据长于smlie）" + field  + "->" + arrayName + " length " + list.Count + "->" + (j+1));
                            break;
                        }

                        lines[i + 1 + j] = "        " + list[j];
                    }

                    //判断数组长度是否相等
                    for (int j = 0; j < lines.Length - 1; j++)
                    {
                        string endLine = lines[i + 1 + j].Trim();
                        if (endLine.StartsWith(".end array-data"))
                        {
                            if(j  == list.Count)
                            {
                                OutPut("I: 数组替换完成 " + field + "->" + arrayName + " length " + j);
                                break;
                            }
                            else
                            {
                                OutPut("E: 替换前后两组数据长度不符 " + field + "->" + arrayName + " length " + list.Count + "->" +  j);
                                break;
                            }

                        }
                    }
                }
            }

            //重新构造文本
            content = "";
            for (int i = 0; i < lines.Length; i++)
            {
                content += lines[i]+"\n";
            }

            //回写
            FileTool.WriteStringByFile(path, content);
        }

        void Replace_Single_Rsmali(string path,string packageName,string template)
        {
            packageName = packageName.Replace(".","/");
            string className = GetPackageNameByPath(path);
            string content = template.Replace(packageName, className);

            //OutPut("替换 R " + path + " " + className);

            //回写
            FileTool.WriteStringByFile(path, content);
        }

        string GetPackageNameByPath(string path)
        {
            string packageName = "";

            path = FileTool.GetFileDirectory(path);
            packageName = path.Replace("\\", "/");
            int index = packageName.LastIndexOf("smali/");

            packageName = packageName.Substring(index).Replace("smali/", "");

            return packageName;
        }

        string GenerateRID()
        {
            //构造一个ID
            string Rid = "0x7f08" + IdIndex.ToString("x4");
            IdIndex++;

            return Rid;
        }

        //一个个去试，直到试出来正确的arrayID为止
        string FindArrayName(string field)
        {
            string[] tmp =  field.Split('_');
            string arrayName = "";

            for (int i = 0; i < tmp.Length; i++)
            {
                arrayName = "";

                for (int j = 0; j <= i; j++)
                {
                    arrayName += "" + tmp[j];

                    if(j != i)
                    {
                        arrayName += "_";
                    }
                }

                if(RIDArray.ContainsKey(arrayName))
                {
                    return arrayName;
                }
            }

            return null;
        }

        private string ParseJavaField(string line)
        {
            string[] blocks = line.Split(' ');
            string[] kv = blocks[blocks.Length - 1].Split('=');
            if (kv.Length > 1)
            {
                return kv[0];
            }
            else
            {
                return blocks[blocks.Length - 3];
            }
        }

        private string ParseJavaFieldValue(string line)
        {
            string[] blocks = line.Split(' ');
            string[] kv = blocks[blocks.Length - 1].Split('=');

            if(kv.Length > 1)
            {
                return kv[1].Replace(";","");
            }
            else
            {
                return blocks[blocks.Length - 1].Replace(";", "");
            }
        }

        private string ParseArrayNameByFill_Array_data(string line)
        {
            string[] blocks = line.Split(' ');
            if (blocks.Length > 1)
            {
                return blocks[2].Replace(":","");
            }
            else
            {
                OutPut("解析 Field 失败 " + line);

                return null;
            }
        }

        private string ParseField(string line)
        {
            string[] blocks = line.Split(' ');
            if(blocks.Length >= 7)
            {
                string[] kv = blocks[blocks.Length - 3].Split(':');
                return kv[0];
            }
            else if(blocks.Length >= 4)
            {
                string[] kv = blocks[blocks.Length - 1].Split(':');
                return kv[0];
            }
            else
            {
                OutPut("解析 Field 失败 " + line);

                return null;
            }
        }

        private string ParseFieldBySput(string line)
        {
            string[] blocks = line.Split(' ');
            if (blocks.Length >= 3)
            {
                string[] kv = blocks[2].Split('>');
                string[] value = kv[1].Split(':');

                return value[0];
            }
            else
            {
                OutPut("解析 Field 失败 " + line);

                return null;
            }
        }

        private string ParseFieldValue(string line)
        {
            string[] blocks = line.Split(' ');
            return blocks[blocks.Length - 1];
        }

        string ReplaceFieldValue(string line,string value)
        {
            string[] blocks = line.Split(' ');

            if(blocks.Length >= 7)
            {
                line = line.Replace(ParseFieldValue(line), value); 
            }
            //else
            //{
            //    line += " = " + value;
            //}

            return line;
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
}
