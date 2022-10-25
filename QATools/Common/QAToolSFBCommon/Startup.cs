using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QAToolSFBCommon.Common;

namespace QAToolSFBCommon
{
    public class Startup
    {
        static private ConfigureFileManager obCfgFileMgr = null;
        static private EMSFB_MODULE s_emModule = EMSFB_MODULE.emSFBModule_Unknown;

        static public void InitSFBCommon(EMSFB_MODULE emModule)
        {
            // Save current module name
            s_emModule = emModule;
            obCfgFileMgr = new ConfigureFileManager(s_emModule);

            // Init logger
            QAToolSFBCommon.NLLog.CLog.Init(s_emModule);

            // Init database connection thread
            QAToolSFBCommon.Database.AbstractDBOpHelper.StartDbConnectionMgrThread();
        }
        static public void UninitSFBCommon()
        {
            QAToolSFBCommon.Database.AbstractDBOpHelper.EndDbConnectionMgrThread();
        }
        static public ConfigureFileManager GetConfigureFileManager() 
        {
            if (null == obCfgFileMgr)
            {
                obCfgFileMgr = new ConfigureFileManager(s_emModule);
            }
            return obCfgFileMgr;
        }

        static public EMSFB_MODULE GetCurModuleType()
        {
            return s_emModule;
        }
    }
}
