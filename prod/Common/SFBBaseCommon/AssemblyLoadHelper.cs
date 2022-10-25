using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFBBaseCommon
{
    /** Example code
    static void Main()
    {
        SFBBaseCommon.AssemblyLoadHelper.InitAssemblyLoaderHelper();
        MainProxy();
    }
    static void MainProxy()
    {
        SFBCommon.NLLog.CLog.Init(kstrCurModuleName);

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SFBControlPanel());
    }
    */

    public class AssemblyLoadHelper
    {
        #region Private values
        static private readonly string kstrReloadDir = Common.kstrSFBInstallPath + "bin\\";
        #endregion

        // static private readonly string kstrReloadDir = CommonHelper.GetSFBInstallPath() + "\\bin\\";
        static public bool InitAssemblyLoaderHelper()
        {
            bool bRet = Directory.Exists(kstrReloadDir);
            if (bRet)
            {
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyLoadResolver;
            }
            Common.OutputTraceLog("Add loader helper:{0}, reload dir:{1}\n", bRet.ToString(), kstrReloadDir);
            return bRet;
        }
        static private System.Reflection.Assembly AssemblyLoadResolver(object sender, ResolveEventArgs args)
        {
            string[] szAssemblyInfo = args.Name.Split(',');
            if ((null != szAssemblyInfo) && (0 < szAssemblyInfo.Length))
            {
                string strCurAssemblyFullName = szAssemblyInfo[0] + ".dll";
                string strSFBAssemblyFullPathName = kstrReloadDir + strCurAssemblyFullName;
                Common.OutputTraceLog("AssemblyLoadResolver {0},{1},{2}:\n", args.Name, strSFBAssemblyFullPathName, strCurAssemblyFullName);
                if (File.Exists(strSFBAssemblyFullPathName))
                {
                    return System.Reflection.Assembly.LoadFrom(strSFBAssemblyFullPathName);
                }
            }
            return null;
        }
    }
}
