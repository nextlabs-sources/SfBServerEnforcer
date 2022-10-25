using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFBCommon;
using SFBCommon.Common;
using SFBCommon.NLLog;

namespace TestProject.SFBCommonTest
{
    class ConfigureFileManagerTester : Logger
    {
        static public void Test()
        {
            ConfigureFileManager obCfgFileMgr = new ConfigureFileManager(EMSFB_MODULE.emSFBModule_Unknown);

            STUSFB_DBACCOUNT stuDBAccount = obCfgFileMgr.GetDBAccount();
            Dictionary<EMSFB_ENDPOINTTTYPE,STUSFB_ENDPOINTPROXYACCOUNT> dicEndpointProxyAccount = obCfgFileMgr.GetEndpointProxyAccount();
            Dictionary<EMSFB_ENDPOINTTTYPE,STUSFB_ENDPOINTPROXYTCPINFO> dicEndpointTcpInfo = obCfgFileMgr.GetEndpointProxyTcpInfo();

            STUSFB_PROMPTMSG stuSipPromptMsg = obCfgFileMgr.GetPromptMsg(EMSFB_CFGINFOTYPE.emCfgInfoSip);
            STUSFB_PROMPTMSG stuHttpPromptMsg = obCfgFileMgr.GetPromptMsg(EMSFB_CFGINFOTYPE.emCfgInfoHttp);
            STUSFB_PROMPTMSG stuSFBCtlPanelPromptMsg = obCfgFileMgr.GetPromptMsg(EMSFB_CFGINFOTYPE.emCfgInfoSFBCtlPanel);
            STUSFB_PROMPTMSG stuEndpointProxyAgentPromptMsg = obCfgFileMgr.GetPromptMsg(EMSFB_CFGINFOTYPE.emCfgInfoNLLyncEndpointProxy_Agent);
            STUSFB_PROMPTMSG stuEndpointProxyAssistantPromptMsg = obCfgFileMgr.GetPromptMsg(EMSFB_CFGINFOTYPE.emCfgInfoNLLyncEndpointProxy_Assistant);

            theLog.OutputLog(EMSFB_LOGLEVEL.emLogLevelDebug, "Info end", stuDBAccount, dicEndpointProxyAccount, dicEndpointTcpInfo, stuSipPromptMsg, stuHttpPromptMsg, stuSFBCtlPanelPromptMsg, stuEndpointProxyAgentPromptMsg, stuEndpointProxyAssistantPromptMsg); // Just make all object are used and can set debug point to check the values
        }
    }
}
