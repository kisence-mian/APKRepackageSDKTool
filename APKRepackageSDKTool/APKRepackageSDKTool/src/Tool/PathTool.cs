using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKRepackageSDKTool
{
    public static class PathTool
    {
        public static string GetCurrentPath()
        {
            return Pri.LongPath.Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// 判断一个路径是否是合法路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool JudgeIsLegalPath(string path)
        {
            try
            {
                FileTool.CreatePath(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
