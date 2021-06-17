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
            return System.IO.Directory.GetCurrentDirectory();
        }
    }
}
