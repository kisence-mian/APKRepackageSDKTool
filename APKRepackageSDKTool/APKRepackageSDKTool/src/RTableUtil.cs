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
        const string c_NoneField = "None";
        static public string AnalysisRJava(string path)
        {
            string result = "";
            try
            {
                string content = FileTool.ReadStringByFile(path);

                //取出所有0x7F开头的数字
                MatchCollection mc = Regex.Matches(content, @"0x7f\d+");

                //计算偏移值
                result += "偏移值 1000 \n";

                //增加一个偏移值
                foreach (Match match in mc)
                {
                    int number = Convert.ToInt32(match.Value, 16);
                    int newNumber = number + 1000;
                    content = content.Replace(match.Value, "0x"+Convert.ToString(newNumber, 16));

                    result += "替换R id -> " + match.Value + " -> " + Convert.ToString(newNumber, 16) + "\n";
                }

                //写回
                FileTool.WriteStringByFile(path, content);
            }
            catch (Exception e)
            {
                result += e.ToString();
            }

            return result;
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

                    if(!RID.ContainsKey(field))
                    {
                        RID.Add(field, value);
                    }

                    callBack("GenerateRKV " + field + " -> " + value);
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
                if(path.StartsWith("R$"))
                {
                    list[i] = list[i].Replace("\\", "/");
                    ReplaceSingleRTable(list[i]);
                }

            }
        }

        void ReplaceSingleRTable(string path)
        {
            string content = FileTool.ReadStringByFile(path);

            //Dictionary<string, string> allMethod = new Dictionary<string, string>();

            string[] lines = content.Split('\n');

            callBack("ReplaceSingleRTable " + path);

            //OutPut("smaliPath " + smaliPath + " " + lines.Length);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                string field = null;

                if (line.StartsWith(".field"))
                {
                    field = ParseField( line);
                }
                //判断存在这个字段
                if (field != null &&RID.ContainsKey(field) )
                {
                    callBack("ReplaceSingleRTable field" + field);

                    //替换
                    lines[i] = line.Replace(ParseFieldValue(line), RID[field]);

                    callBack(path +"-> ReplaceSingleRTable " + field + " -> " + RID[field]);
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

        private string ParseField(string line)
        {
            string[] blocks = line.Split(' ');
            if(blocks.Length >= 7)
            {
                string[] kv = blocks[blocks.Length - 3].Split(':');
                return kv[0];
            }
            else
            {
                return null;
            }

        }

        private string ParseFieldValue(string line)
        {
            string[] blocks = line.Split(' ');
            return blocks[blocks.Length - 1];
        }
    }
}
