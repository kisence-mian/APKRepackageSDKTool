using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RarTool
{
    OutPutCallBack callBack;
    OutPutCallBack errorCallBack;

    CmdService cs;

    string rarCmd = @"360zip.exe -x {RarPath} {AimPath}";

    public RarTool(string rarCmd,OutPutCallBack callBack, OutPutCallBack errorCallBack)
    {
        this.callBack = callBack;
        this.errorCallBack = errorCallBack;

        cs = new CmdService(callBack, errorCallBack);

        this.rarCmd = rarCmd;
    }

    public void Decompression(string rarFile)
    {
        string aimPath = FileTool.RemoveExpandName(rarFile);

        if(!Directory.Exists(aimPath))
        {
            string cmd = rarCmd.Replace("{AimPath}", aimPath).Replace("{RarPath}", rarFile);

            cs.Execute(cmd, outPutCmd: false);
        }
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
