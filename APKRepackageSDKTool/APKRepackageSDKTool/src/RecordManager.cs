using FrameWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace APKRepackageSDKTool
{
    public static class RecordManager
    {
        const string c_directoryName = "Record";
        static Dictionary<string, RecordData> m_cache = new Dictionary<string, RecordData>();

        public static string GetRecord(string recordName,string key, string defaultValue)
        {
            RecordData data = GetRecord(recordName);

            if (data.ContainsKey(key))
            {
                return data[key];
            }
            else
            {
                return defaultValue;
            }
        }

        static RecordData GetRecord(string recordName)
        {
            RecordData data = null;
            if (m_cache.ContainsKey(recordName))
            {
                data = m_cache[recordName];
            }
            else
            {
                data = LoadData(recordName);
                m_cache.Add(recordName, data);
                
            }

            return data;
        }

        public static void SaveRecord(string recordName, string key, string value)
        {
            RecordData data = GetRecord(recordName);

            if(data.ContainsKey(key))
            {
                data[key] = value;
            }
            else
            {
                data.Add(key, value);
            }

            SaveData(recordName, data);
        }

        static Deserializer des = new Deserializer();
        static RecordData LoadData(string recordName)
        {
            string path = GetSavePath(recordName);
            if ( File.Exists(path))
            {
                string content = FileTool.ReadStringByFile(path);

                Dictionary<string, string> table = des.Deserialize<Dictionary<string,string>>(content);
                RecordData data = new RecordData();
                foreach (var item in table)
                {
                    data.Add(item.Key, item.Value);
                }

                return data;
            }
            else
            {
                return new RecordData();
            }
        }

        static void SaveData(string recordName, RecordData data)
        {
            string content = Serializer.Serialize(data);
            string path = GetSavePath(recordName);
            FileTool.WriteStringByFile(path, content);
        }

        static string GetSavePath(string recordName)
        {
            string path = PathTool.GetCurrentPath() + "\\" + c_directoryName + "\\" + recordName + ".json";
            return path;
        }
    }

    [Serializable]
    public class RecordData : Dictionary<string,string>
    {

    }
}
