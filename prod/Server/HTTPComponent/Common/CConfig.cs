using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFBCommon.Common;
using SFBCommon.SFBObjectInfo;
using SFBCommon.NLLog;

namespace Nextlabs.SFBServerEnforcer.HTTPComponent.Common
{
    class CConfig
    {
        #region Const values
        public const string kstrDefaultTextEnableEnforce = "Enable this room to be enforced by NextLabs Enforcer.";
        public const string kstrDefaultTextEnforceDescYes = "This room will be enforced by NextLabs Enforcer.";
        public const string kstrDefaultTextEnforceDescNo = "This room will not be enforced by NextLabs Enforcer.";
        public const string kstrDefaultTextEnforcePersistentChatRoomPathHeadFlag = "";
        #endregion

        static private ConfigureFileManager s_obCfgFileMgr = null;
        static private STUSFB_PROMPTMSG s_stuPormptMsg = null;

        #region config value
        public static string strTextEnableEnforce = kstrDefaultTextEnableEnforce;
        public static string strTextEnforceDescYes = kstrDefaultTextEnforceDescYes;
        public static string strTextEnforceDescNo = kstrDefaultTextEnforceDescNo;
        public static string strTextFileUri = kstrDefaultTextEnforcePersistentChatRoomPathHeadFlag;
        private static readonly Dictionary<EMSFB_ENDPOINTTTYPE, STUSFB_ENDPOINTPROXYTCPINFO> s_dicEndpointTcpInfo = null;
        #endregion

        static CConfig()
        {
            s_obCfgFileMgr = new ConfigureFileManager(EMSFB_MODULE.emSFBModule_HTTPComponent);

            // Get endpointproxy config
            s_dicEndpointTcpInfo = s_obCfgFileMgr.GetEndpointProxyTcpInfo();

            // Get prompt message
            s_stuPormptMsg = s_obCfgFileMgr.GetPromptMsg(EMSFB_CFGINFOTYPE.emCfgInfoHttp);
            if (null != s_stuPormptMsg)
            {
                strTextEnableEnforce = CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLTextSetEnforcerFlag, kstrDefaultTextEnableEnforce);
                strTextEnforceDescYes = CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLTextEnforceStatusYesFlag, kstrDefaultTextEnforceDescYes);
                strTextEnforceDescNo = CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLTextEnforceStatusNoFlag, kstrDefaultTextEnforceDescNo);
                strTextFileUri = CommonHelper.GetValueByKeyFromDir(s_stuPormptMsg.m_dirRuntimeInfo, ConfigureFileManager.kstrXMLTextEnforcePersistentChatRoomPathHeadFlag, kstrDefaultTextEnforcePersistentChatRoomPathHeadFlag);
            }
        }

        public static STUSFB_ENDPOINTPROXYTCPINFO GetEndpointProxyTcpInfo(EMSFB_ENDPOINTTTYPE emEndpointType)
        {
            return CommonHelper.GetValueByKeyFromDir(s_dicEndpointTcpInfo, emEndpointType, null);
        }
    }
}
