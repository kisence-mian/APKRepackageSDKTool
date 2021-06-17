using System;
using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace APKRepackageSDKTool.src
{
    public class YmlTool
    {
        YamlStream yaml = new YamlStream();
        YamlMappingNode mapping;

        object yamlObject;


        public YmlTool(string path)
        {
            string content = FileTool.ReadStringByFile(path);
            var input = new StringReader(content);
            yaml.Load(input);
            mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        }

        public string ReadItemList(string key)
        {
            string result = "";
            YamlSequenceNode items = (YamlSequenceNode)mapping.Children[new YamlScalarNode(key)];

            foreach (var entry in items)
            {
                YamlScalarNode node = (YamlScalarNode)entry;

                result += node.Value + "\n";
            }

            return result;
        }

        public int ReadItemListCount(string key)
        {
            YamlSequenceNode items = (YamlSequenceNode)mapping.Children[new YamlScalarNode(key)];
            int count = 0;

            foreach (var entry in items)
            {
                count++;
            }

            return count;
        }

        public void RemoveItemList(string key)
        {
            YamlSequenceNode items = (YamlSequenceNode)mapping.Children[new YamlScalarNode(key)];
        }

        public void Modify(string key,string value)
        {
            foreach (var entry in mapping.Children)
            {
                YamlScalarNode node = (YamlScalarNode)entry.Key;
                if (node.Value == key)
                {
                    YamlScalarNode tmp = (YamlScalarNode)entry.Value;
                    tmp.Value = value;
                }
            }
        }

        public void Save()
        {
            //yaml.Save();
        }
    }
}
